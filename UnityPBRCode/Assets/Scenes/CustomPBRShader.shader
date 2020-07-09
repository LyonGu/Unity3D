
Shader "Custom/PBRShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}

        //金属贴图最后只获取r分量和a分量
        _MetallicTex ("Metallic(R), Smoothness(A)",2D) = "white"{}
        
        _Metallic ("Metallic", Range(0,1)) = 1.0        //金属度调整
        _Glossiness ("Smoothness", Range(0,1)) = 1.0    //平滑度调整
        [Normal] _Normal ("NormalMap", 2D) = "bump"{}   //法线贴图
        _OcclussionTex ("Occlusion", 2D) = "white" {}   //AO贴图
        _AO ("AO", Range(0,1)) = 1.0
        _Emission("Emission", Color) = (0,0,0,1)

        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque"}
        LOD 100

        Pass
        {
            Tags {"LightMode" = "ForwardBase"}
            CGPROGRAM
                //顶点片段着色器
                #pragma vertex vert
                #pragma fragment frag
                //指定平台，也可以省略
                #pragma target 3.0
                //雾效和灯光的关键字
                #pragma multi_compile_fog
                #pragma multi_compile_fwdbase
                //一些会用到的cginc文件
                #include "UnityCG.cginc"
                #include "Lighting.cginc"
                #include "UnityPBSLighting.cginc"
                #include "AutoLight.cginc"


                fixed4 _Color;
                sampler2D _MainTex;
                float4 _MainTex_ST;
                sampler2D _MetallicTex;
                fixed _Metallic;
                fixed _Glossiness;
                sampler2D _OcclussionTex;
                fixed _AO;
                half3 _Emission;
                sampler2D _Normal;

                /*
                    顶点着色器传递到片段着色器的数据有：
                        最终需要输出到屏幕的顶点位置pos，
                        贴图uv坐标uv，世界坐标worldPos，
                        用于切线空间法线计算的TNB矩阵，
                        雾效坐标，
                        阴影shadowmap采样坐标，
                        顶点光照球谐光照系数sh。
                */

                struct v2f
                {
                    float4 pos:SV_POSITION;
                    float2 uv:TEXCOORD0;
                    float3 worldPos: TEXCOORD1;
                    float3 tSpace0:TEXCOORD2;//TNB矩阵0
                    float3 tSpace1:TEXCOORD3;//TNB矩阵1
                    float3 tSpace2:TEXCOORD4;//TNB矩阵2
                    //TNB矩阵同时也传递了世界空间法线及世界空间切线
                    
                    UNITY_FOG_COORDS(5)//雾效坐标 fogCoord
                    UNITY_SHADOW_COORDS(6)//阴影坐标 _ShadowCoord
                    
                    //如果需要计算了顶点光照和球谐函数，则输入sh参数。
                    #if UNITY_SHOULD_SAMPLE_SH
                        half3 sh: TEXCOORD7; // SH
                    #endif    

                };

                //这里没有写appdata结构体，直接采用内置的appdata_Full
                v2f vert(appdata_full v)
                {
                    v2f o;
                    UNITY_INITIALIZE_OUTPUT(v2f,o);
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                    o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                    float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                    half3 worldTangent = UnityObjectToWorldDir(v.tangent);
                    //利用切线和法线的叉积来获得副切线，tangent.w分量确定副切线方向正负，
                    //unity_WorldTransformParams.w判定模型是否有变形翻转。
                    half3 worldBinormal = cross(worldNormal,worldTangent)*v.tangent.w *unity_WorldTransformParams.w;

                    //组合TBN矩阵，用于后续的切线空间法线计算。
                    o.tSpace0 = float3(worldTangent.x, worldBinormal.x, worldNormal.x);
                    o.tSpace1 = float3(worldTangent.y, worldBinormal.y, worldNormal.y);
                    o.tSpace2 = float3(worldTangent.z, worldBinormal.z, worldNormal.z);

                    // SH/ambient和顶点光照写入o.sh里
                    #ifndef LIGHTMAP_ON
                        #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                            o.sh = 0;
                            // Approximated illumination from non-important point lights
                            //如果有顶点光照的情况（超出系统限定的灯光数或者被设置为non-important灯光）
                            #ifdef VERTEXLIGHT_ON
                                o.sh += Shade4PointLights(
                                unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
                                unity_LightColor[0].rgb, unity_LightColor[1].rgb, 
                                unity_LightColor[2].rgb, unity_LightColor[3].rgb,
                                unity_4LightAtten0, o.worldPos, worldNormal);
                            #endif
                            //球谐光照计算（光照探针，超过顶点光照数量的球谐灯光）
                            o.sh = ShadeSHPerVertex(worldNormal, o.sh);
                        #endif
                    #endif

                    UNITY_TRANSFER_LIGHTING(o, v.texcoord1.xy); 
                    // pass shadow and, possibly, light cookie coordinates to pixel shader
                    //在appdata_full结构体里。v.texcoord1就是第二套UV，也就是光照贴图的UV。
                    //计算并传递阴影坐标

                    UNITY_TRANSFER_FOG(o, o.pos); // pass fog coordinates to pixel shader。计算传递雾效的坐标。
                    return o;
                }


                /*
                    struct SurfaceOutputStandard
                    {
                        fixed3 Albedo;      // base (diffuse or specular) color
                        float3 Normal;      // tangent space normal, if written//这里传入的是worldNormal，是官方写错了？
                        half3 Emission;
                        half Metallic;      // 0=non-metal, 1=metal
                        // Smoothness is the user facing name, it should be perceptual smoothness 
                        //but user should not have to deal with it.
                        // Everywhere in the code you meet smoothness it is perceptual smoothness
                        //smooth这里untiy官方费劲吧啦解释很多，其实就是smooth以及其转换的roughness并不是最终的roughness
                        //之后会做一些转换，在这里不用管太多。
                        half Smoothness;    // 0=rough, 1=smooth
                        half Occlusion;     // occlusion (default 1)
                        fixed Alpha;        // alpha for transparencies
                    };


                    struct UnityGI
                    {
                        UnityLight light;
                        UnityIndirect indirect;
                    };

                    struct UnityLight
                    {
                        half3 color;//光照颜色和强度
                        half3 dir;//光照方向
                        //ndotl已经被弃用，在这里只是为了旧版本兼容保证稳定。
                        half  ndotl; // Deprecated: Ndotl is now calculated on the fly and is no longer stored. Do not used it.
                    };

                    struct UnityIndirect
                    {
                        half3 diffuse;//漫反射部分
                        half3 specular;//高光直接反射部分
                    };


                    struct UnityGIInput
                    {
                        UnityLight light; // pixel light, sent from the engine

                        float3 worldPos;
                        half3 worldViewDir;
                        half atten;
                        half3 ambient;

                        // interpolated lightmap UVs are passed as full float precision data to fragment shaders
                        // so lightmapUV (which is used as a tmp inside of lightmap fragment shaders) should
                        // also be full float precision to avoid data loss before sampling a texture.
                        float4 lightmapUV; // .xy = static lightmap UV, .zw = dynamic lightmap UV

                        #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION) || defined(UNITY_ENABLE_REFLECTION_BUFFERS)
                        float4 boxMin[2];
                        #endif
                        #ifdef UNITY_SPECCUBE_BOX_PROJECTION
                        float4 boxMax[2];
                        float4 probePosition[2];
                        #endif
                        // HDR cubemap properties, use to decompress HDR texture
                        float4 probeHDR[2];
                    };
                    

                */
                fixed4 frag(v2f i): SV_Target
                {
                    half3 normalTex = UnpackNormal(tex2D(_Normal, i.uv));
                    half3 worldNormal = half3(dot(i.tSpace0, normalTex),dot(i.tSpace1, normalTex),dot(i.tSpace2, normalTex));
                    worldNormal = normalize(worldNormal);
                    //计算灯光方向：注意这个方法已经包含了对灯光的判定。
                    //其实在forwardbase pass中，可以直接用灯光坐标代替这个方法，因为只会计算Directional Light。
                    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));
                    float3 worldViewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));//片段指向摄像机方向viewDir

                    //计算SurfaceOutputStandard结构数据
                    SurfaceOutputStandard o;  //声明变量
                    UNITY_INITIALIZE_OUTPUT(SurfaceOutputStandard,o); //初始化里面的信息。避免有的时候报错干扰
                    fixed4 AlbedoColorSampler = tex2D(_MainTex, i.uv) * _Color; //采样颜色贴图，同时乘以控制的TintColor
                    o.Albedo = AlbedoColorSampler.rgb; //颜色分量，a分量在后面
                    o.Emission = _Emission; //自发光
                    fixed4 MetallicSmoothnessSampler = tex2D(_MetallicTex, i.uv); //采样Metallic-Smoothness贴图
                    o.Metallic = MetallicSmoothnessSampler.r * _Metallic; //r通道乘以控制色并赋予金属度
                    o.Smoothness = MetallicSmoothnessSampler.a * _Glossiness; //a通道乘以控制色并赋予光滑度
                    o.Alpha = AlbedoColorSampler.a; //单独赋予透明度
                    o.Occlusion = tex2D(_OcclussionTex,i.uv) * _AO; //采样AO贴图，乘以控制色，赋予AO
                    o.Normal = worldNormal; //赋予法线

                    //计算光照衰减和阴影
                    UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos)
                    //注意这个atten会在方法里进行声明，在外面就不必再声明过了


                    //计算UnityGI结构
                    UnityGI gi; //声明变量
                    UNITY_INITIALIZE_OUTPUT(UnityGI, gi); //初始化归零
                    gi.indirect.diffuse = 0; //indirect部分先给0参数，后面需要计算出来。这里只是示意
                    gi.indirect.specular = 0;
                    gi.light.color = _LightColor0.rgb; //unity内置的灯光颜色变量
                    gi.light.dir = lightDir; //赋予之前计算的灯光方向


                    //初始化giInput并赋予已有的值。此参数为gi计算所需要的输入参数。
                    // Call GI (lightmaps/SH/reflections) lighting function
                    UnityGIInput giInput;
                    UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);//初始化归零
                    giInput.light = gi.light;//之前这个light已经给过，这里补到这个结构体即可。
                    giInput.worldPos = i.worldPos;//世界坐标
                    giInput.worldViewDir = worldViewDir;//摄像机方向
                    giInput.atten = atten;//在之前的光照衰减里面已经被计算。其中包含阴影的计算了。
                    
                    //球谐光照和环境光照输入（已在顶点着色器里的计算，这里只是输入）
                    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                        giInput.ambient = i.sh;
                    #else//假如没有做球谐计算，这里就归零
                        giInput.ambient.rgb = 0.0;
                    #endif

                    //反射探针相关
                    /*
                        unity_SpecCube0_HDR是默认的反射探针的数据，unity会根据你场景内是否有自定义反射探针及物体所在的探针位置进行定义。
                        而假如你的物体在两个反射探针的融合处，则会再给你第二个探针的数据，也就是unity_SpecCube1_HDR。
                    */
                    giInput.probeHDR[0] = unity_SpecCube0_HDR;
                    giInput.probeHDR[1] = unity_SpecCube1_HDR;

                    //SPECCUBE_BLENDING是指反射探针融合的开启与否。假如没有开启BLENDING的话，当多个反射探针有过度时，会产生渲染效果的突然变化
                    #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
                        giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
                    #endif


                    /*
                     UNITY_SPECCUBE_BOX_PROJECTION
                        假如默认不开启，则反射探针的采样是把反射探针环境当做一个无限大的天空盒采样，物体的位置和采样坐标没有关系。
                        而开启后，会根据采样的片段在反射探针的实际位置来计算采样坐标，从而在反射探针内的不同位置，会有不同的采样值。
                        就好像一个金属球，在室内的不同位置，采样的环境是会不同的。而在室外的广阔空间下，移动一定的距离采样值几乎没有变化。
                    
                    */
                    #ifdef UNITY_SPECCUBE_BOX_PROJECTION
                        giInput.boxMax[0] = unity_SpecCube0_BoxMax;
                        giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
                        giInput.boxMax[1] = unity_SpecCube1_BoxMax;
                        giInput.boxMin[1] = unity_SpecCube1_BoxMin;
                        giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
                    #endif



                    //最终计算
                    //基于PBS的全局光照（gi变量）的计算函数。计算结果是gi的参数（Light参数和Indirect参数）。注意这一步还没有做真的光照计算。
                    LightingStandard_GI(o, giInput, gi);
                    fixed4 c = 0;
                    // realtime lighting: call lighting function
                    //PBS计算
                    c += LightingStandard(o, worldViewDir, gi);


                    //叠加雾效。
                    UNITY_EXTRACT_FOG(i);//此方法定义了一个片段着色器里的雾效坐标变量，并赋予传入的雾效坐标。
                    UNITY_APPLY_FOG(_unity_fogCoord, c); // apply fog

                    return c;

                }


            ENDCG
        }
    }
    FallBack "Diffuse"
}
