// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "GPUInstance/GPUInstance_LightMap_Shadow"
{
    /*
        使用GPUInstance技术 配合lightMap以及Shadow
    */
    Properties
    {
        _DiffuseTexture ("DiffuseTexture", 2D) = "white" {}
        _DiffuseTint("DiffuseTint", Color) =  (1, 1, 1, 1)
        _LightMap ("LightMap", 2D) = "white" {}
        _Shadow("Shadow Map", 2D) = "white"{}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Tags{"LightMode" = "ForwardBase"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            //前向渲染
            #pragma multi_compile_fwdbase
            //支持GPUInstace
            #pragma multi_compile_instancing

            //开关编译选项
            /*
                对Unity内置lightmap的获取。我们定义两个编译开关，然后在自定义顶点输入输出结构包含lightmap的uv。
            */
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal 	: NORMAL;     //模型空间的法线信息
                float2 uv : TEXCOORD0;

                //在结构体中定义SV_InstanceID
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4   pos : SV_POSITION;
                float3   lightDir : TEXCOORD0;
                float3   normal : TEXCOORD1;
                float2   uv : TEXCOORD2;

                //LIGHTING_COORDS 这个宏指令定义了对阴影贴图和光照贴图采样所需的参数
                LIGHTING_COORDS(3, 4)
                #ifdef LIGHTMAP_ON
                    float2 uv_LightMap:TEXCOORD5;
                #endif

                //在结构体中定义SV_InstanceID
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color) // Make _Color an instanced property (i.e. an array)
            UNITY_INSTANCING_BUFFER_END(Props)


            sampler2D _DiffuseTexture;
            float4 _DiffuseTexture_ST;
            float4 _DiffuseTint;
            float4 _LightColor0;
            //sampler2D _LightMap;//传进来的lightmap
            UNITY_DECLARE_TEX2D(_LightMap);
            float4 _LightMap_ST;//

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _DiffuseTexture);
                o.lightDir = normalize(ObjSpaceLightDir(v.vertex)); //局部空间的方向
                o.normal = normalize(v.normal).xyz;
                #ifdef LIGHTMAP_ON
                o.uv_LightMap = v.uv.xy * _LightMap_ST.xy + _LightMap_ST.zw;
                #endif
                TRANSFER_VERTEX_TO_FRAGMENT(o);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                float3 L = normalize(i.lightDir);
                float3 N = normalize(i.normal);

                float attenuation = LIGHT_ATTENUATION(i) * 2; //光的衰减
                float4 ambient =  UNITY_LIGHTMODEL_AMBIENT * 2; //环境光
                float NdotL = saturate(dot(N, L));
                float4 diffuseTerm = NdotL * _LightColor0 * _DiffuseTint * attenuation; //漫反射系数

                float4 diffuse = tex2D(_DiffuseTexture, i.uv)*UNITY_ACCESS_INSTANCED_PROP(Props,_Color);//这里用宏访问Instance的颜色属性
                float4 finalColor = (ambient + diffuseTerm) * diffuse;
                #ifdef LIGHTMAP_ON
                //DecodeLightmap函数可以针对不同的平台对光照贴图进行解码。
                // fixed3 lm = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uv_LightMap.xy));
                fixed3 lm = DecodeLightmap(UNITY_SAMPLE_TEX2D(_LightMap, i.uv_LightMap.xy));
                finalColor.rgb *= lm;
                #endif
                return finalColor;
            }
            ENDCG
        }

        /*
            阴影投射需要自定义，否则不支持GPU Instance同时需要包括指令multi_compile_instancing以及
            在vert及frag函数中取instance id否则多个对象将得不到阴影投射
        */
        Pass
        {
            Tags{"LightMode" = "ShadowCaster"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            //前向渲染
            #pragma multi_compile_shadowcaster
            //支持GPUInstace
            #pragma multi_compile_instancing



            #include "UnityCG.cginc"


            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal 	: NORMAL;     //模型空间的法线信息
                float2 uv : TEXCOORD0;

                //在结构体中定义SV_InstanceID
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                V2F_SHADOW_CASTER;
                float2 uv:TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
              };



            sampler2D _Shadow;


            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);//
                o.uv = v.uv.xy;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o);
                return o;

            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                fixed alpha = tex2D(_Shadow, i.uv).a;
                clip(alpha - 0.5);
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
}
