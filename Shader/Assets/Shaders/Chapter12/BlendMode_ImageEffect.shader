Shader "Shaders/Chapter12/BlendMode_ImageEffect"
{
	//叠加  滤镜 正片叠底
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_BlendTex ("Blend Texture", 2D) = "white" {}
		_Opacity ("Blend Opacity", Range(0.0, 1.0)) = 1.0
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

			sampler2D _BlendTex;
			fixed _Opacity;

			struct appdata
			{
				half4 vertex : POSITION;
				fixed2 uv : TEXCOORD0;
			};

			struct v2f
			{
				fixed2 uv : TEXCOORD0;
				half4 vertex : SV_POSITION;
			};			
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				

				fixed4 renderTex = tex2D(_MainTex, i.uv);
				fixed4 blendTex = tex2D(_BlendTex, i.uv);
				
				// Perform a multiply Blend mode   正片叠底:两层图像的像素相乘，最后会得到一个更暗的图像
				//fixed4 blendedMultiply = renderTex * blendTex;

				fixed4 blendedAdd = renderTex + blendTex;

				// 滤镜:两层图像的像素值取互补数，然后将它们相乘，最后再去互补数
				fixed4 blendedScreen = 1.0 - ((1.0 - renderTex) * (1.0 - blendTex));

				
				fixed4 blendColor = blendedScreen;
				// Adjust amount of Blend Mode with a lerp
				renderTex = lerp(renderTex, blendColor,  _Opacity);
				
				return renderTex;

			}
			ENDCG
		}
	}
}
