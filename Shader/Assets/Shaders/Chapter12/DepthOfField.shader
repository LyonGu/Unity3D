Shader "Shaders/Chapter12/DepthOfField"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_BlurTex("Blur", 2D) = "white"{}
		_BlurSize ("Blur Size", Float) = 1.0

		_focalDistance("focalDistance",Range(0.0, 100.0)) = 10.0
		_nearBlurScale("nearBlurScale",Range(0.0, 100.0)) = 0
		_farBlurScale("farBlurScale",Range(0.0, 1000.0))  = 50


	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Geometry"}
		
		LOD 100

		ZTest Always Cull Off ZWrite Off

		//高斯模糊处理
		//第1个pass, 序号为0 , 使用其他shader里的pass
		UsePass "Shaders/Chapter12/GaussianBlur/GAUSSIAN_BLUR_VERTICAL"
		
		//第2个pass, 序号为1 , 使用其他shader里的pass
		UsePass "Shaders/Chapter12/GaussianBlur/GAUSSIAN_BLUR_HORIZONTAL"

		//景深效果
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			
			
			#include "UnityCG.cginc"

			struct a2v
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;
			sampler2D_float _CameraDepthTexture;
			sampler2D _BlurTex;

			
			float _focalDistance;
			float _nearBlurScale;
			float _farBlurScale;
			
			v2f vert (a2v v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				//dx中纹理从左上角为初始坐标，需要反向
				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					o.uv.y = 1 - o.uv.y;
				#endif

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				
				//取原始清晰图片进行uv采样
				fixed4 ori = tex2D(_MainTex, i.uv);

				//取模糊普片进行uv采样
				fixed4 blur = tex2D(_BlurTex, i.uv);

				//取当位置对应的深度值
				float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
				//将深度值转化到01线性空间
				depth = Linear01Depth(depth);
				
				//如果depth小于焦点的物体，那么使用原始清晰图像，否则使用模糊的图像与清晰图像的差值，通过差值避免模糊和清晰之间明显的边界，结果为远景模糊效果
				fixed4 final = (depth <= _focalDistance) ? ori : lerp(ori, blur, clamp((depth - _focalDistance) * _farBlurScale, 0, 1));
				//上面的结果，再进行一次计算，如果depth大于焦点的物体，使用上面的结果和模糊图像差值，得到近景模糊效果
				final = (depth > _focalDistance) ? final : lerp(ori, blur, clamp((_focalDistance - depth) * _nearBlurScale, 0, 1));
				//焦点位置是清晰的图像，两边分别用当前像素深度距离焦点的距离进行差值，这样就达到原理焦点位置模糊的效果
		 
				return final;

				
				
			}
			ENDCG
		}
	}
}
