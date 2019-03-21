Shader "Shaders/Chapter9/ForwardRenderingShadowAttenuationInfunction"
{
	//两个Pass,第一个pass为ForwardBase,第二个pass为ForwardAdd
	//BassPass只会执行一次，一个additional Pass会根据影响该物体的其他逐像素光源数目被调用多次
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
				SHADOW_COORDS(2)  //声明一个用于阴影纹理采样的坐标
			};

			v2f vert(a2v i){
				v2f o;
				o.pos = UnityObjectToClipPos(i.vertex);
				o.worldNormal = UnityObjectToWorldNormal(i.normal);
				o.worldPos = mul(unity_ObjectToWorld, i.vertex).xyz;

				//计算声明的阴影纹理坐标
			 	TRANSFER_SHADOW(o);

				return o;
			}

			fixed4 frag(v2f v): SV_Target{
				fixed3 worldNormal = normalize(v.worldNormal);

				// 一般会用内置方法UnityWorldSpaceLightDir(o.worldPos)
				fixed3 worldLightDir = normalize(UnityWorldSpaceLightDir(v.worldPos));
			
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;

				fixed3 diffuse = _LightColor0.rgb * _Diffuse.rgb * max(dot(worldNormal,worldLightDir),0);

				fixed3 viewDir = normalize(_WorldSpaceCameraPos.xyz - v.worldPos.xyz);

				fixed3 halfDir = normalize(worldLightDir + viewDir);

				fixed3 specular = _LightColor0.rgb * _Specular.rgb * pow(max(0,dot(worldNormal,halfDir)),_Gloss);

				//使用内置宏得到阴影值
				//fixed shadow = SHADOW_ATTENUATION(v);

				//同时计算衰减值和阴影值 atten就包含了阴影值 理解为atten = atten *shadow
				UNITY_LIGHT_ATTENUATION(atten, v, v.worldPos);

				fixed3 color = ambient + (diffuse + specular) * atten;

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
				//内置方法UnityWorldSpaceLightDir(o.worldPos)里已经区分了
				fixed3 worldLightDir = normalize(UnityWorldSpaceLightDir(v.worldPos));

				fixed3 diffuse = _LightColor0.rgb * _Diffuse.rgb * max(dot(worldNormal,worldLightDir),0);

				fixed3 viewDir = normalize(_WorldSpaceCameraPos.xyz - v.worldPos.xyz);

				fixed3 halfDir = normalize(worldLightDir + viewDir);

				fixed3 specular = _LightColor0.rgb * _Specular.rgb * pow(max(0,dot(worldNormal,halfDir)),_Gloss);

				//使用了内置函数就不用判断光源的类型来分别计算了
				UNITY_LIGHT_ATTENUATION(atten, v, v.worldPos);

				//只计算漫反射和高光，不计算环境光
				fixed3 color = (diffuse + specular) * atten;

				return fixed4(color, 1.0);
			}

			ENDCG
		}
	}
	FallBack "Specular"
}
