// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Shaders/Chapter12/OutLinePostEffect" {
	Properties {
		_MainTex ("Main Tex", 2D) = "white" {}
		_Outline ("Outline", Range(0, 1)) = 0.1
		_OutlineColor ("Outline Color", Color) = (1, 0, 0, 1)
		_BlurSize ("Blur Size", Float) = 1.0
		_blurTex ("Bloom (RGB)", 2D) = "black" {}
	
	}
    SubShader {
		Tags { "RenderType"="Opaque" "Queue"="Geometry"}
		
		ZTest Always Cull Off ZWrite Off
		
		//高斯模糊处理
		//第1个pass, 序号为0 , 使用其他shader里的pass
		UsePass "Shaders/Chapter12/GaussianBlur/GAUSSIAN_BLUR_VERTICAL"
		
		//第2个pass, 序号为1 , 使用其他shader里的pass
		UsePass "Shaders/Chapter12/GaussianBlur/GAUSSIAN_BLUR_HORIZONTAL"
		
		//第3个pass，抠图，利用原图减去模糊图
		Pass {
			
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
		
			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			sampler2D _BlurTex;
			float4 _BlurTex_TexelSize;

			struct a2v {
				float4 vertex : POSITION;
				float2 uv :TEXCOORD0;
			}; 
			
			struct v2f {
			    float4 pos : SV_POSITION;
			    float2 uv :TEXCOORD0;
			};
			
			v2f vert (a2v v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				#if UNITY_UV_STARTS_AT_TOP
					if (_MainTex_TexelSize.y < 0)
						o.uv.y = 1 - o.uv.y;
				#endif
				return o;
			}
			
			float4 frag(v2f i) : SV_Target { 
				fixed4 colorMain = tex2D(_MainTex, i.uv);
				fixed4 colorBlur = tex2D(_BlurTex, i.uv);
				//最后的颜色是_BlurTex - _MainTex，周围0-0=0，黑色；边框部分为描边颜色-0=描边颜色；中间部分为描边颜色-描边颜色=0。最终输出只有边框
				//return fixed4((colorBlur - colorMain).rgb, 1);
				return colorBlur - colorMain;        
			}
			
			ENDCG
		}


		//第4个pass，用抠图纹理与原图纹理叠加
		Pass {
			
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
		
			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			sampler2D _BlurTex;
	

			struct a2v {
				float4 vertex : POSITION;
				float2 uv :TEXCOORD0;
			}; 
			
			struct v2f {
			    float4 pos : SV_POSITION;
			    float2 uv :TEXCOORD0;
			};
			
			v2f vert (a2v v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				#if UNITY_UV_STARTS_AT_TOP
					if (_MainTex_TexelSize.y < 0)
						o.uv.y = 1 - o.uv.y;
				#endif
				return o;
			}
			
			float4 frag(v2f i) : SV_Target { 
				fixed4 colorMain = tex2D(_MainTex, i.uv);
				fixed4 colorBlur = tex2D(_BlurTex, i.uv);
				return colorBlur + colorMain;        
			}
			
			ENDCG
		}


	
	}
	FallBack "Diffuse"
}
