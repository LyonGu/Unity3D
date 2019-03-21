Shader "Shaders/Chapter10/Refraction"
{
	//折射效果
	Properties
	{
		_Color ("Color Tint", Color) = (1,1,1,1)

		//折射颜色
		_RefractColor ("Refraction Color", Color) = (1,1,1,1) 

		//材质的折射程度
		_RefractAmount ("Refraction Amount", Range(0,1)) = 1
		_RefractRatio ("Refraction Ratio", Range(0.1, 1)) = 0.5
		_Cubemap ("Refraction Cubemap", Cube) = "_Skybox" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Geometry"}
		Pass
		{
			Tags { "LightMode"="ForwardBase" }
			CGPROGRAM
			
			#pragma multi_compile_fwdbase

			#pragma vertex vert
			#pragma fragment frag

			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			fixed4 _Color;
			fixed4 _RefractColor;
			float _RefractAmount;
			fixed _RefractRatio;
			samplerCUBE _Cubemap;

			struct a2v{
				float4 vertex: POSITION;
				float3 normal: NORMAL;
			};

			struct v2f{

				float4 pos : SV_POSITION;
				float3 worldPos : TEXCOORD0;
				float3 worldNormal : TEXCOORD1;
				float3 worldViewDir : TEXCOORD2;
				float3 worldRefr : TEXCOORD3;
				SHADOW_COORDS(4)
			};

			v2f vert(a2v i){
				v2f o;
				o.pos = UnityObjectToClipPos(i.vertex);
				o.worldNormal = UnityObjectToWorldNormal(i.normal);

				o.worldPos = mul(unity_ObjectToWorld, i.vertex).xyz;

				o.worldViewDir = UnityWorldSpaceViewDir(o.worldPos);

				//计算折射向量 reflect(-视线方向，法线方向)
				o.worldRefr = refract(-normalize(o.worldViewDir), normalize(o.worldNormal), _RefractRatio);

				TRANSFER_SHADOW(o);
				return o;

			}

			fixed4 frag(v2f o): SV_Target{
				fixed3 worldNormal = normalize(o.worldNormal);
				fixed3 worldLightDir = normalize(UnityWorldSpaceLightDir(o.worldPos));	
				fixed3 worldViewDir = normalize(o.worldViewDir);

				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;
				
				fixed3 diffuse = _LightColor0.rgb * _Color.rgb * max(0, dot(worldNormal, worldLightDir));


				// Use the refract dir in world space to access the cubemap
				fixed3 refraction = texCUBE(_Cubemap, o.worldRefr).rgb * _RefractColor.rgb;

				//计算衰减以及阴影值
				UNITY_LIGHT_ATTENUATION(atten, o, o.worldPos);

				// Mix the diffuse color with the refract color
				fixed3 color = ambient + lerp(diffuse, refraction, _RefractAmount) * atten;

				return fixed4(color, 1.0);

			}
			
			ENDCG
		}
	}
	FallBack "Reflective/VertexLit"
}
