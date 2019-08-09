Shader "SampleUV/FlashEffectMode"
{
	Properties
	{
		_MainTex("MainTex(RGB)", 2D) = "white" {}
		_FlashTex("FlashTex", 2D) = "black" {}
		_FlashColor("FlashColor",Color) = (1,1,1,1)
		_FlashFactor("FlashFactor", Vector) = (0, 1, 0, 0.5)
		_FlashStrength ("FlashStrength", Range(0, 5)) = 1
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

			//改为一个vector4，减少传参次数消耗
	 		fixed4 _FlashFactor;
			fixed _FlashStrength;



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
				float4 worldPos : TEXCOORD2;
				float4 pos : SV_POSITION;
				float3 worldNormal : NORMAL;

			};

	
			
			v2f vert (a2v v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				//顶点转化到世界空间
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.worldLightDir = WorldSpaceLightDir(v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
			
				fixed4 albedo = tex2D(_MainTex, i.uv);
				half3 normal = normalize(i.worldNormal);
				half3 light = normalize(i.worldLightDir);
				fixed diff = dot(normal, light);
				diff = diff * 0.5 + 0.5;

				//通过时间偏移世界坐标对flashTex进行采样 (这里不太明白为什么世界空间坐标要*0.5  _FlashFactor.zw必须是0.5)
				half2 flashuv = i.worldPos.xy * _FlashFactor.zw + _FlashFactor.xy * _Time.y;
				fixed4 flash = tex2D(_FlashTex, flashuv) * _FlashColor * _FlashStrength;
				fixed4 c;
				//将flash图与原图叠加
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * albedo;
				c.rgb = _LightColor0.rgb * diff * albedo + ambient + flash.rgb;
				c.a = 1;

				return c;
			}
			ENDCG
		}
	}
}
