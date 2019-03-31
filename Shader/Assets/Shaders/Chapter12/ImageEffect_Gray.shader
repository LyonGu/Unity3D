Shader "Shaders/Chapter12/ImageEffect_Gray"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_LuminosityAmount ("GrayScale Amount", Range(0.0, 1.0)) = 1.0
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


			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed _LuminosityAmount;

			struct a2v{
				float4 vertex:POSITION;
				half2 texcoord:TEXCOORD0;
			};

			struct v2f{
				half4 pos: SV_POSITION;
				half2 uv:TEXCOORD0;
			};
			
			v2f vert (a2v v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				// o.uv = v.texcoord * _MainTex_ST.xy + _MainTex_ST.zw
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				
				fixed4 col = tex2D(_MainTex, i.uv);
				float luminosity = 0.299 * col.r + 0.587 * col.g + 0.114 * col.b;
				fixed4 finalColor = lerp(col, luminosity, _LuminosityAmount);
				
				return finalColor;
			}
			ENDCG
		}
	}
}
