Shader "Shaders/Chapter9/ForwardRenderingShadow"
{
	//两个Pass,第一个pass为ForwardBase,第二个pass为ForwardAdd
	Properties
	{
		_Diffuse ("Diffuse", Color) = (1,1,1,1)
		_Specular ("Specular", Color) = (1,1,1,1)
		_Gloss ("Gloss", Range(8.0, 256)) = 20
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		Pass
		{
			//平行光处理
			Tags { "LightMode"="ForwardBase" }

			CGPROGRAM

			//开启这个编译命令，是为了让Unity自动赋予shader需要的光照计算变量
			#pragma multi_compile_fwdbase

			#pragma vertex vert
			#pragma fragment frag

			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			fixed4 _Diffuse;
			fixed4 _Specular;
			float  _Gloss;

			struct a2v{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f{
				float4 pos : SV_POSITION;
				float3 worldNormal: TEXCOORD0;
				float3 worldPos: TEXCOORD1;
				SHADOW_COORDS(2)  //声明一个用于阴影纹理采样的坐标，2就是下一个采样纹理的索引值 TEXCOORD2
			};

			v2f vert(a2v v){
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

				//计算声明的阴影纹理坐标

				//使用TRANSFER_SHADOW 注意：
					// 1 必须保证a2v中顶点坐标名为vertex 
					// 2 顶点着色器的输入形参名必须为v
					// 3 v2f的顶点变量名必须为pos

					//总结下：a2v中必须要有vertex表示顶点位置 v2f中必须有pos表是裁剪空间的位置 形参必须得是v
			 	TRANSFER_SHADOW(o);

				return o;
			}

			fixed4 frag(v2f v): SV_Target{
				fixed3 worldNormal = normalize(v.worldNormal);

				//因为这个pass 只处理平行光 一般会用内置方法UnityWorldSpaceLightDir(o.worldPos)
				fixed3 worldLightDir = normalize(_WorldSpaceLightPos0.xyz);

				//环境光
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;

				fixed3 diffuse = _LightColor0.rgb * _Diffuse.rgb * max(dot(worldNormal,worldLightDir),0);

				fixed3 viewDir = normalize(_WorldSpaceCameraPos.xyz - v.worldPos.xyz);

				fixed3 halfDir = normalize(worldLightDir + viewDir);

				fixed3 specular = _LightColor0.rgb * _Specular.rgb * pow(max(0,dot(worldNormal,halfDir)),_Gloss);

				//平行光没有衰减，所以这里设置成1.0
				fixed atten = 1.0;

				//使用内置宏得到阴影值
				fixed shadow = SHADOW_ATTENUATION(v);

				fixed3 color = ambient + (diffuse + specular) * atten * shadow;

				return fixed4(color, 1.0);
			}


			ENDCG
		}

		Pass
		{
			//其他像素光源：点光源（渲染模型设置为重要的）+平行光（最强的）
			Tags { "LightMode"="ForwardAdd" }

			//开启混合，为了跟之前的计算结果进行混合
			Blend One One
			CGPROGRAM

			//开启这个编译命令，是为了让Unity自动赋予shader需要的光照计算变量
			#pragma multi_compile_fwdadd

			//为点光源与聚光添加阴影
			#pragma multi_compile_fwdadd_fullshadows

			#pragma vertex vert
			#pragma fragment frag

			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			fixed4 _Diffuse;
			fixed4 _Specular;
			float  _Gloss;

			struct a2v{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f{
				float4 pos : SV_POSITION;
				float3 worldNormal: TEXCOORD0;
				float3 worldPos: TEXCOORD1;
			};

			v2f vert(a2v i){
				v2f o;
				o.pos = UnityObjectToClipPos(i.vertex);
				o.worldNormal = UnityObjectToWorldNormal(i.normal);
				o.worldPos = mul(unity_ObjectToWorld, i.vertex).xyz;
				return o;
			}

			fixed4 frag(v2f v): SV_Target{
				fixed3 worldNormal = normalize(v.worldNormal);

				//根据光源的类型计算光的方向
				//内置方法UnityWorldSpaceLightDir(o.worldPos)里已经区分了
				#ifdef USING_DIRECTIONAL_LIGHT
					//平行光光源
					fixed3 worldLightDir = normalize(_WorldSpaceLightPos0.xyz);
				#else
					fixed3 worldLightDir = normalize(_WorldSpaceLightPos0.xyz - v.worldPos.xyz);
				#endif

				fixed3 diffuse = _LightColor0.rgb * _Diffuse.rgb * max(dot(worldNormal,worldLightDir),0);

				fixed3 viewDir = normalize(_WorldSpaceCameraPos.xyz - v.worldPos.xyz);

				fixed3 halfDir = normalize(worldLightDir + viewDir);

				fixed3 specular = _LightColor0.rgb * _Specular.rgb * pow(max(0,dot(worldNormal,halfDir)),_Gloss);

				//根据光源类型计算衰减值: 平行光没有衰减 点光源和聚光有衰减
				#ifdef USING_DIRECTIONAL_LIGHT
					fixed atten = 1.0; //平行光没有衰减，所以这里设置成1.0
				#else
					#if defined (POINT)
						//点光源
						float3 lightCoord = mul(unity_WorldToLight, float4(v.worldPos, 1)).xyz;
				        fixed atten = tex2D(_LightTexture0, dot(lightCoord, lightCoord).rr).UNITY_ATTEN_CHANNEL;
					#elif defined (SPOT)
						//聚光
						float4 lightCoord = mul(unity_WorldToLight, float4(v.worldPos, 1));
				        fixed atten = (lightCoord.z > 0) * tex2D(_LightTexture0, lightCoord.xy / lightCoord.w + 0.5).w * tex2D(_LightTextureB0, dot(lightCoord, lightCoord).rr).UNITY_ATTEN_CHANNEL;
					#else
						//其他光源
					   fixed atten = 1.0;
					#endif
				#endif

				//只计算漫反射和高光，不计算环境光
				fixed3 color = (diffuse + specular) * atten;

				return fixed4(color, 1.0);
			}

			ENDCG
		}
	}
	FallBack "Specular"
}
