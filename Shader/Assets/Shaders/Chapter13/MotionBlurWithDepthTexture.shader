Shader "Shaders/Chapter13/MotionBlurWithDepthTexture"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}  //一定要有_MainTex属性
		_BlurSize ("Blur Size", Float) = 1.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		CGINCLUDE

			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			half4 _MainTex_TexelSize;
			half _BlurSize;

			//虽然Properties里没有声明属性，但是也能自己添加，外部可以传进来
			//_CameraDepthTexture是Unity专门用来存储深度信息的贴图
			sampler2D _CameraDepthTexture;
			float4x4 _CurrentViewProjectionInverseMatrix;
			float4x4 _PreviousViewProjectionMatrix;
			
			struct v2f {
				float4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
				half2 uv_depth : TEXCOORD1; //深度纹理的采样坐标
			};

			v2f vert(appdata_img v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				
				o.uv = v.texcoord;
				o.uv_depth = v.texcoord;
				
				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					o.uv_depth.y = 1 - o.uv_depth.y;
				#endif
						 
				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				// SAMPLE_DEPTH_TEXTURE从深度贴图里采样出深度信息，这里的深度信息是非线性的
				// 因为在投影矩阵下会有裁剪操作
				float d = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv_depth);
				#if defined(UNITY_REVERSED_Z)
					d = 1.0 - d;
				#endif

				// 把UV坐标转换到[-1,1]  当前像素的NDC坐标
				float4 H = float4(i.uv.x * 2 - 1, i.uv.y * 2 - 1, d * 2 - 1, 1);

				// 把像素坐标H转到世界空间
				float4 D = mul(_CurrentViewProjectionInverseMatrix, H);

				// 进行透视化处理 ？？？？？？这里为什么要除w分量 ==》为了方便计算
				float4 worldPos = D / D.w; //这里除不除w分量从效果上看不出什么区别
				
				// Current viewport position 当前像素的NDC坐标
				float4 currentPos = H;  
				// Use the world position, and transform by the previous view-projection matrix.  
				float4 previousPos = mul(_PreviousViewProjectionMatrix, worldPos); //仅仅得到了裁剪空间的坐标[-w,w]
				// Convert to nonhomogeneous points [-1,1] by dividing by w.
				previousPos /= previousPos.w;  //经过透视除法之后才能得到NDC坐标
				
				// Use this frame's position and last frame's to compute the pixel velocity.
				float2 velocity = (currentPos.xy - previousPos.xy)/2.0;
				
				float2 uv = i.uv;
				float4 c = tex2D(_MainTex, uv);
				uv += velocity * _BlurSize;
				for (int it = 1; it < 3; it++, uv += velocity * _BlurSize) {
					float4 currentColor = tex2D(_MainTex, uv);
					c += currentColor;
				}
				c /= 3;
				
				return fixed4(c.rgb, 1.0);
			}

		ENDCG

		Pass {      
			ZTest Always Cull Off ZWrite Off
			    	
			CGPROGRAM  
			
			#pragma vertex vert  
			#pragma fragment frag  
			  
			ENDCG  
		}
	}
	FallBack Off
}
