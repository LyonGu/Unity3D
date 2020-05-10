Shader "Custom/CustomPBR"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Albedo", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0.0, 1.0)) = 0.5
        _RoughnessMap ("Roughness", 2D) = "white" {}
        _SpecColor ("Specular", Color) = (0.2, 0.2, 0.2)
        _SpecGlossMap ("Specular (RGB) Smoothness (A)", 2D) = "white" {}
        _BumpScale ("Bump Scale", Float) = 1.0
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _EmissionColor ("EmissionColor", Color) = (0, 0, 0)
        _EmissionMap ("Emission", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 300
        CGINCLUDE
            /*
                在上面的代码中我们通过使用#pragma target 3.0 来指明使用 Shader Target 3.0，这是因
                为基于物理渲染涉及了较多的公式，因此需要较多的数学指令来进行计算，这可能会超过 Shader
                Target 2.0 对指令数目的规定，因此我们选择使用更高的 Shader Target 3.0
            */
            #pragma target 3.0
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "HLSLSupport.cginc"
            #include "Lighting.cginc"


            fixed4 _Color;
            half _Glossiness;
            //fixed4 _SpecColor;

            sampler2D _MainTex;
            sampler2D _SpecGlossMap;
            float _BumpScale;
            sampler2D _BumpMap;
            fixed4 _EmissionColor;
            sampler2D _EmissionMap;
            sampler2D _RoughnessMap;


            //对应的纹理坐标
            float4 _MainTex_ST;

            struct a2v{
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float4 texcoord: TEXCOORD0;
            };


            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 TtoW0 : TEXCOORD1;
                float4 TtoW1 : TEXCOORD2;
                float4 TtoW2 : TEXCOORD3;
                SHADOW_COORDS(4)
                UNITY_FOG_COORDS(5)
            };

            //BRDF中漫反射计算函数
            inline half3 CustomDisneyDiffuseTerm(half NdotV, half NdotL, half LdotH, half roughness, half3 baseColor)
            {
                half fd90 = 0.5 + 2 * LdotH * LdotH * roughness;

                // Two schlick fresnel term
                half lightScatter  = (1 + (fd90 - 1) * pow(1 - NdotL, 5));
                half viewScatter  = (1 + (fd90 - 1) * pow(1 - NdotV, 5));
                return baseColor * UNITY_INV_PI * lightScatter * viewScatter;
            }

            //几何阴影遮蔽函数
            inline half CustomSmithJointGGXVisibilityTerm(half NdotL, half NdotV, half roughness)
            {
                // Original formulation:
                //  lambda_v  = (-1 + sqrt(a2 * (1 - NdotL2) / NdotL2 + 1)) * 0.5f;
                //  lambda_l  = (-1 + sqrt(a2 * (1 - NdotV2) / NdotV2 + 1)) * 0.5f;
                //  G  = 1 / (1 + lambda_v + lambda_l);
                // Approximation of the above formulation (simplify the sqrt, not mathematically correct but close enough)
                half a2 = roughness * roughness;
                half lambdaV = NdotL * (NdotV * (1 - a2) + a2);
                half lambdaL = NdotV * (NdotL * (1 - a2) + a2);
                return 0.5f / (lambdaV + lambdaL + 1e-5f);
            }
            //法线分布函数
            inline half CustomGGXTerm(half NdotH, half roughness) {
                half a2 = roughness * roughness;
                half d = (NdotH * a2 - NdotH) * NdotH + 1.0f;
                return UNITY_INV_PI * a2 / (d * d + 1e-7f);
            }

            //菲尼尔反射
            inline half3 CustomFresnelTerm(half3 c, half cosA) {
                half t = pow(1 - cosA, 5);
                return c + (1 - c) * t;
            }

            inline half3 CustomFresnelLerp(half3 c0, half3 c1, half cosA) {
                half t = pow(1 - cosA, 5);
                return lerp (c0, c1, t);
            }

            v2f vertBase(a2v v) {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                //计算世界空间中的切线正交基的矢量
                fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
                fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                fixed3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w;

                //构建从切线到世界的转换矩阵（因为后面会使用到环境贴图，所以必须转换到世界空间）
                o.TtoW0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
                o.TtoW1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
                o.TtoW2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);

                //阴影纹理坐标计算
                TRANSFER_SHADOW(o);

                //雾效
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            half4 fragBase(v2f i) : SV_Target {
                //高光颜色计算：直接使用高光贴图
                half4 specGloss = tex2D(_SpecGlossMap, i.uv);
                _Glossiness = _Glossiness * (1 - tex2D(_RoughnessMap, i.uv).r);
                // specGloss.a *= _Glossiness;
                half3 specColor = specGloss.rgb * _SpecColor.rgb;

                //粗糙度
                half roughness = 1 - _Glossiness;

                //1-反射比例--> 漫反射比例
                half oneMinusReflectivity = 1 - max(max(specColor.r, specColor.g), specColor.b);
                half3 diffColor = _Color.rgb * tex2D(_MainTex, i.uv).rgb * oneMinusReflectivity;

                //使用法线贴图和_BumpScale调整表面凹凸程度,并且把切线空间的法线转到世界空间
                half3 normalTangent = UnpackNormal(tex2D(_BumpMap, i.uv));
                normalTangent.xy *= _BumpScale;
                normalTangent.z = sqrt(1.0 - saturate(dot(normalTangent.xy, normalTangent.xy)));
                half3  normalWorld  =  normalize(half3(dot(i.TtoW0.xyz,  normalTangent),
                    dot(i.TtoW1.xyz, normalTangent), dot(i.TtoW2.xyz, normalTangent)));

                //计算世界坐标，光方向，视觉方向，反射方向
                float3 worldPos = float3(i.TtoW0.w, i.TtoW1.w, i.TtoW2.w);
                half3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
                half3 viewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                half3 reflDir = reflect(-viewDir, normalWorld);

                //计算阴影和衰减
                UNITY_LIGHT_ATTENUATION(atten, i, worldPos);


                //计算BDRF模型需要的各种变量值
                half3 halfDir = normalize(lightDir + viewDir);
                half nv = saturate(dot(normalWorld, viewDir));
                half nl = saturate(dot(normalWorld, lightDir));
                half nh = saturate(dot(normalWorld, halfDir));
                half lv = saturate(dot(lightDir, viewDir));
                half lh = saturate(dot(lightDir, halfDir));


                // BDRF中的漫反射部分
                half3 diffuseTerm = CustomDisneyDiffuseTerm(nv, nl, lh, roughness, diffColor);


                // BDRF中的高光部分：这里使用的是GGX模型，
                half V = CustomSmithJointGGXVisibilityTerm(nl, nv, roughness); //几何阴影遮蔽函数
                half D = CustomGGXTerm(nh, roughness * roughness); //法线分布函数
                half3 F = CustomFresnelTerm(specColor, lh); //菲尼尔反射
                half3 specularTerm = F * V * D;

                //自发光部分
                half3 emisstionTerm = tex2D(_EmissionMap, i.uv).rgb * _EmissionColor.rgb;


                //为了得到更加真实的光照，我们还需要计算基于图像的光照部分(IBL),
                //这部分主要是把环境贴图当做光源，来计算间接光
                //unity_SpecCube0 包含了该物体周围当前活跃的反射探针,如果场景里没有放置，天空盒会自己生成一个默认的反射探针
                /*
                    一般使用 samplerCUBE 来声明一个立方体贴图并使用 texCUBE 来采样
                    它，但是 Unity 内置反射探针的立方体贴图则是以一种特殊的方式声明的，这主要是为了在某些
                    平台下可以节省 sampler slots，使用 UNITY_DECLARE_TEXCUBE进行采样

                    由于这样的特殊性，在采样 unity_SpecCube0 时我们也应该使用内置宏如
                    UNITY_SAMPLE_TEXCUBE（在 HLSLSupport.cginc 文件中被定义）来采样。由于在这里我们还
                    需要对指定级数的多级渐远纹理采样，因此我们使用内置宏 UNITY_SAMPLE_TEXCUBE_LOD
                */
                half perceptualRoughness = roughness * (1.7 - 0.7 * roughness);
                half mip = perceptualRoughness * 6;
                half4 envMap = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflDir, mip);
                half grazingTerm = saturate((1 - roughness) + (1 - oneMinusReflectivity));
                half surfaceReduction = 1.0 / (roughness * roughness + 1.0);
                half3 indirectSpecular = surfaceReduction * envMap.rgb * CustomFresnelLerp(specColor,
                grazingTerm, nv);


                // 合并所有的计算： 自发光 + BRDF + 间接光（IBL）
                half3 col = emisstionTerm + UNITY_PI * (diffuseTerm + specularTerm) * _LightColor0.rgb
                * nl * atten + indirectSpecular;
                UNITY_APPLY_FOG(i.fogCoord, c.rgb);
                return half4(col, 1);
            }

            v2f vertAdd(a2v v) {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                //计算世界空间中的切线正交基的矢量
                fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
                fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                fixed3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w;

                //构建从切线到世界的转换矩阵（因为后面会使用到环境贴图，所以必须转换到世界空间）
                o.TtoW0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
                o.TtoW1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
                o.TtoW2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);

                //阴影纹理坐标计算
                TRANSFER_SHADOW(o);
                return o;
            }

            //不需要计算雾效、自发光和 IBL 的部分
            half4 fragAdd(v2f i) : SV_Target {
                //高光颜色计算：直接使用高光贴图
                half4 specGloss = tex2D(_SpecGlossMap, i.uv);
                _Glossiness = _Glossiness * (1 - tex2D(_RoughnessMap, i.uv).r);
                //specGloss.a *= _Glossiness;
                half3 specColor = specGloss.rgb * _SpecColor.rgb;

                //粗糙度
                half roughness = 1 - _Glossiness;

                //1-反射比例--> 漫反射比例
                half oneMinusReflectivity = 1 - max(max(specColor.r, specColor.g), specColor.b);
                half3 diffColor = _Color.rgb * tex2D(_MainTex, i.uv).rgb * oneMinusReflectivity;

                //使用法线贴图和_BumpScale调整表面凹凸程度,并且把切线空间的法线转到世界空间
                half3 normalTangent = UnpackNormal(tex2D(_BumpMap, i.uv));
                normalTangent.xy *= _BumpScale;
                normalTangent.z = sqrt(1.0 - saturate(dot(normalTangent.xy, normalTangent.xy)));
                half3  normalWorld  =  normalize(half3(dot(i.TtoW0.xyz,  normalTangent),
                    dot(i.TtoW1.xyz, normalTangent), dot(i.TtoW2.xyz, normalTangent)));

                //计算世界坐标，光方向，视觉方向，反射方向
                float3 worldPos = float3(i.TtoW0.w, i.TtoW1.w, i.TtoW2.w);
                half3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
                half3 viewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                half3 reflDir = reflect(-viewDir, normalWorld);

                //计算阴影和衰减
                UNITY_LIGHT_ATTENUATION(atten, i, worldPos);


                //计算BDRF模型需要的各种变量值
                half3 halfDir = normalize(lightDir + viewDir);
                half nv = saturate(dot(normalWorld, viewDir));
                half nl = saturate(dot(normalWorld, lightDir));
                half nh = saturate(dot(normalWorld, halfDir));
                half lv = saturate(dot(lightDir, viewDir));
                half lh = saturate(dot(lightDir, halfDir));


                // BDRF中的漫反射部分
                half3 diffuseTerm = CustomDisneyDiffuseTerm(nv, nl, lh, roughness, diffColor);


                // BDRF中的高光部分：这里使用的是GGX模型，
                half V = CustomSmithJointGGXVisibilityTerm(nl, nv, roughness); //几何阴影遮蔽函数
                half D = CustomGGXTerm(nh, roughness * roughness); //法线分布函数
                half3 F = CustomFresnelTerm(specColor, lh); //菲尼尔反射
                half3 specularTerm = F * V * D;

                // 合并所有的计算：BRDF
                half3 col =  UNITY_PI * (diffuseTerm + specularTerm) * _LightColor0.rgb * nl * atten;
                return half4(col, 1);
            }

        ENDCG
        Pass {
            Tags { "LightMode" = "ForwardBase" }

            CGPROGRAM
                #pragma vertex  vertBase
			    #pragma fragment fragBase
            ENDCG
        }

        Pass {
            Tags { "LightMode" = "ForwardAdd" }

            CGPROGRAM

                #pragma vertex  vertAdd
			    #pragma fragment fragAdd
            ENDCG
        }
    }
}
