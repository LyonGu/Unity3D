Shader "Shaders/Common/MultipleVariants_PropertyDrawer" {
	//只有单纯的漫反射贴图
	Properties {
		_Color ("Color Tint", Color) = (1, 1, 1, 1)
		_MainTex ("Main Tex", 2D) = "white" {}
		_BumpMap ("Normal Map", 2D) = "bump" {}
		_BumpScale ("Bump Scale", float) 		= 1.0
		[KeywordEnum(MainTex, Light, Normal)] _Custom ("Custom", Float) = 0
		[Toggle(_ENABLE_SHADOW)] _ENABLE_SHADOW ("ENABLE_SHADOW", Float) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Geometry"}

		Pass {

			Tags { "LightMode"="ForwardBase" }
			CGPROGRAM

				//定义变体

				/*
					shader_feature          //声明gloabl变体     Material.EnableKeyword 有效/  Shader.EnableKeyword 有效
					shader_feature_local    //声明local变体      Material.EnableKeyword 有效/  Shader.EnableKeyword 无效
				*/

				// _MainTexOn _LightOn 都为false，默认使用_MainTexOn

				/*
					EnableKeyword过，只要不DisableKeyword 就一直打开着
				*/


				#pragma multi_compile _CUSTOM_MAINTEX _CUSTOM_LIGHT _CUSTOM_NORMAL
				#pragma shader_feature _ENABLE_SHADOW



				#if _CUSTOM_LIGHT || _CUSTOM_NORMAL
					// #pragma multi_compile_fwdbase
					#include "Lighting.cginc"
					#include "AutoLight.cginc"
				#endif

				#pragma vertex vert
				#pragma fragment frag


				fixed4 _Color;
				sampler2D _MainTex;
				float4 _MainTex_ST;
				sampler2D _BumpMap;
				float4 _BumpMap_ST;
				float _BumpScale;


				struct a2v {
					float4 vertex : POSITION;
					float3 normal : NORMAL;
					float4 tangent : TANGENT;
					float4 texcoord : TEXCOORD0;
				};

				struct v2f {
					float4 pos : SV_POSITION;
					float4 uv : TEXCOORD0;

					#if _CUSTOM_LIGHT
						float3 worldPos: TEXCOORD1;
						float3 worldNormal:TEXCOORD2;
						#if _ENABLE_SHADOW
							SHADOW_COORDS(3)
						#endif
					#elif _CUSTOM_NORMAL
						float4 TtoW0 : TEXCOORD1;
						float4 TtoW1 : TEXCOORD2;
						float4 TtoW2 : TEXCOORD3;
						#if _ENABLE_SHADOW
							SHADOW_COORDS(4)
						#endif
					#endif

				};

				v2f vert(a2v v) {

					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					#if _CUSTOM_MAINTEX
						o.uv.xy = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
					#endif

					#if _CUSTOM_LIGHT || _CUSTOM_NORMAL
						o.uv.xy = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
						#if _CUSTOM_LIGHT
							float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
							fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
							o.worldPos = worldPos;
							o.worldNormal = worldNormal;

							#if _ENABLE_SHADOW
								TRANSFER_SHADOW(o);
							#endif
						#else
							o.uv.zw = v.texcoord.xy * _BumpMap_ST.xy + _BumpMap_ST.zw;
							float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
							fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
							fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
							fixed3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w;

							o.TtoW0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
							o.TtoW1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
							o.TtoW2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
							#if _ENABLE_SHADOW
								TRANSFER_SHADOW(o);
							#endif
						#endif
					#endif

					return o;
				}

				fixed4 frag(v2f i) : SV_Target {
					fixed3 albedo;
					#if _CUSTOM_MAINTEX
						albedo = tex2D(_MainTex, i.uv.xy).rgb * _Color.rgb;
					#else
					 	albedo = _Color.rgb;
					#endif

					#if _CUSTOM_LIGHT || _CUSTOM_NORMAL
						// //测试_LightOn开启的时候  _MainTexOn是否关闭
						// #if _CUSTOM_MAINTEX
						// 	return fixed4(1,0,0, 1.0);
						// #else
						// 	#if _TestOn
						// 		return fixed4(0,1,0, 1.0);
						// 	#else
						// 		return fixed4(0,0,0, 1.0);
						// 	#endif

						// #endif
						albedo = tex2D(_MainTex, i.uv.xy).rgb * _Color.rgb;
						#if _CUSTOM_LIGHT
							float3 worldPos = i.worldPos;
							fixed3 worldLightDir = normalize(UnityWorldSpaceLightDir(worldPos));
							fixed3 worldNormal = normalize(i.worldNormal);
							fixed3 ambient = albedo;

							fixed3 diffuse = _LightColor0.rgb * albedo * max(0, dot(worldNormal, worldLightDir));

							#if _ENABLE_SHADOW
								// fixed shadow = SHADOW_ATTENUATION(i);
								// return fixed4(shadow,shadow,shadow 1.0);
								UNITY_LIGHT_ATTENUATION(atten, i, worldPos);
								return fixed4(ambient + diffuse * atten, 1.0);
							#else
								return fixed4(ambient + diffuse, 1.0);
							#endif

						#else
							float3 worldPos = float3(i.TtoW0.w, i.TtoW1.w, i.TtoW2.w);
							fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
							fixed3 viewDir = normalize(UnityWorldSpaceViewDir(worldPos));

							fixed3 bump = UnpackNormal(tex2D(_BumpMap, i.uv.zw));
							bump.xy *= _BumpScale;
							bump.z = sqrt(1.0 - saturate(dot(bump.xy, bump.xy)));
							//切线到世界
							bump = normalize(half3(dot(i.TtoW0.xyz, bump), dot(i.TtoW1.xyz, bump), dot(i.TtoW2.xyz, bump)));

							fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * albedo;

							fixed3 diffuse = _LightColor0.rgb * albedo * max(0, dot(bump, lightDir));
							#if _ENABLE_SHADOW
								UNITY_LIGHT_ATTENUATION(atten, i, worldPos);
								return fixed4(ambient + diffuse * atten, 1.0);
							#else
								return fixed4(ambient + diffuse, 1.0);
							#endif
						#endif
					#else
						return fixed4(albedo, 1.0);
					#endif

				}

			ENDCG
		}
		Pass {
			Tags { "LightMode" = "ShadowCaster" }

			CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag

				#pragma multi_compile_shadowcaster

				#include "UnityCG.cginc"
				struct v2f {
					V2F_SHADOW_CASTER;

				};

				v2f vert(appdata_base v) {
					v2f o;

					//使用TRANSFER_SHADOW_CASTER_NORMALOFFSET注意
					// 1 顶点着色器函数形参为v
					// 2 形参类型必须含有vertex 和 normal字段
					TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)

					return o;
				}

				fixed4 frag(v2f i) : SV_Target {
					SHADOW_CASTER_FRAGMENT(i)

				}
			ENDCG
		}

	}
	//FallBack "Diffuse"

}

