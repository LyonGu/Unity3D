Shader "SampleUV/FlashEffect"
{
	Properties
	{
		_MainTex("MainTex(RGB)", 2D) = "white" {}
		_FlashTex("FlashTex", 2D) = "black" {}
		_FlashColor("FlashColor",Color) = (1,1,1,1)
		_FlashSpeedX("FlashSpeedX", Range(-5, 5)) = 0
		_FlashSpeedY("FlashSpeedY", Range(-5, 5)) = 0.5
		_FlashFactor ("FlashFactor", Range(0, 5)) = 1

	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _FlashTex;
			fixed4 _FlashColor;
			fixed _FlashSpeedX;
			fixed _FlashSpeedY;
			fixed _FlashFactor;



			struct a2v
			{
				float4 vertex : POSITION;
				fixed2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float3 worldLightDir : TEXCOORD1;
				float4 pos : SV_POSITION;
				float3 worldNormal : NORMAL;
			};

	
			
			v2f vert (a2v v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.worldLightDir = WorldSpaceLightDir(v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
			
				fixed4 albedo = tex2D(_MainTex, i.uv);
				clip(albedo.a - 0.05);

				half3 normal = normalize(i.worldNormal);
				half3 light = normalize(i.worldLightDir);
				fixed diff = dot(normal, light);
				diff = diff * 0.5 + 0.5;
				//通过时间将采样flash的uv进行偏移
				half2 flashuv = i.uv + half2(_FlashSpeedX, _FlashSpeedY) * _Time.y;
				fixed4 flash = tex2D(_FlashTex, flashuv) * _FlashColor * _FlashFactor;
				fixed4 c;
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * albedo;
				//将flash图与原图叠加
				c.rgb = diff * albedo + ambient + flash.rgb;
				c.a = 1;
				return c;
			}
			ENDCG
		}
	}
}
