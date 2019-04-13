Shader "Shaders/Chapter12/DepthTest"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Geometry"}
		
		LOD 100

		ZTest Always Cull Off ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			
			
			#include "UnityCG.cginc"


			//仍然要声明一下_CameraDepthTexture这个变量，虽然Unity这个变量是unity内部赋值
			sampler2D _CameraDepthTexture;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4	  _MainTex_TexelSize;

			struct a2v
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			
			v2f vert (a2v v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				//直接根据UV坐标取该点的深度值
				float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
			
				//将深度值变为线性01空间
				depth = Linear01Depth(depth);

				//越远的地方越亮，越近的地方越暗，也就是我们shader中所写的，直接按照深度值来输出了一幅图片
				return float4(depth, depth, depth, 1);

				
			}
			ENDCG
		}
	}
}
