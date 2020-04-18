Shader "Shaders/Chapter12/BloomHDR"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Bloom ("Bloom (RGB)", 2D) = "black" {}
		_BrightThreshold ("BrightThreshold", Float) = 1.0
		_BlurSize ("Blur Size", Float) = 1.0
		_Exposure("Exposure", Float) = 1.0

		_Exp("Exp", Float) = 1.0
		_BM("BM", Float) = 1.0
		_Lum("Lum", Float) = 1.0
		_IsGamma("IsGamma", Int) = 0

	}
	SubShader
	{
		CGINCLUDE
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			half4 _MainTex_TexelSize;
			sampler2D _Bloom;
			float _BrightThreshold;
			float _BlurSize;
			float _Exposure;
			float _BM;
			float _Exp;
			float _Lum;
			int _IsGamma;

			struct v2f{
				float4 pos: SV_POSITION;
				half2 uv:TEXCOORD0;
			};

			//提取亮度顶点着色器
			//appdata_img为UINITY提供的内置结构体，包含了顶点坐标和纹理坐标
			v2f vertExtractBright(appdata_img v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				return o;

			}

			fixed luminance(fixed4 color) {
				return  0.2125 * color.r + 0.7154 * color.g + 0.0721 * color.b;
			}

			//提取亮度像素着色器
			fixed4 fragExtractBright(v2f i) : SV_Target {
				fixed4 c = tex2D(_MainTex, i.uv);
				//使用HDR
				float val = luminance(c);
				if (val > _BrightThreshold)  //默认应该是_BrightThreshold为1.0
				{
					return c;
				}
				return fixed4(0.0,0.0,0.0,1.0);
			}


			//最后混合bloom效果数据结构
			struct v2fBloom {
				float4 pos : SV_POSITION;
				half4 uv : TEXCOORD0;
			};

			//最后得到bloom效果的顶点着色器
			v2fBloom vertBloom(appdata_img v) {
				v2fBloom o;

				o.pos = UnityObjectToClipPos (v.vertex);
				o.uv.xy = v.texcoord;
				o.uv.zw = v.texcoord;

				//DirectX平台下，因为Unity开启了抗锯齿,主纹理和亮度纹理在竖直方向上是不一样的，亮部纹理需要翻转Y坐标
				//主纹理调用Graphics.Blit这个方法，Unity默认帮我们做好了
				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0.0)
					o.uv.w = 1.0 - o.uv.w;
				#endif

				return o;
			}

			//最后得到bloom效果的像素着色器
			fixed4 fragBloom(v2fBloom i) : SV_Target {

				//使用HDR后，最后输出结果还要进行色调映射 ，如果在非线性空间下还得进行gamma矫正
				fixed4 hdrColor =  tex2D(_MainTex, i.uv.xy) + tex2D(_Bloom, i.uv.zw);
				// fixed3 result = fixed3(1.0,1.0,1.0) - exp(-hdrColor * _Exposure);

				// float y = dot(float4(0.3,0.59,0.11,1), hdrColor);
				// float yd = _Exp * (_Exp / _BM + 1) / (_Exp + 1);
				// fixed3 result = (hdrColor*yd).xyz;
				/*
					// Reinhard色调映射
					vec3 mapped = hdrColor / (hdrColor + vec3(1.0));

					// 曝光色调映射
    				vec3 mapped = vec3(1.0) - exp(-hdrColor * _Exposure);

					//Unity用的是这个色调映射 从HDR到LDR
					float4 frag(v2f i) :COLOR
					{
						float4 c = tex2D(_MainTex, i.uv_MainTex);
						float y = dot(float4(0.3,0.59,0.11,1),c);
						float yd = _Exp * (_Exp / _BM + 1) / (_Exp + 1);
						return c*yd;
					}

				*/




				//使用Unity的内置函数进行色调映射
				fixed3 color = tex2D(_MainTex, i.uv.xy).xyz;
				fixed3 result = tex2D(_Bloom, i.uv.zw).xyz;
				float lum = Luminance(result);
				result = color + result * (lum+0.1) * _Lum;


				// also gamma correct while we're at it
				if(_IsGamma == 1)
				{
					const float gamma = 2.2;
					result.x = pow(result.x, 1.0 / gamma);
					result.y = pow(result.y, 1.0 / gamma);
					result.z = pow(result.z, 1.0 / gamma);
				}

				return fixed4(result, 1.0);

				// return float4(result,1);
			}

		ENDCG

		ZTest Always Cull Off ZWrite Off

		//第一个pass 序号为0 提取图片亮度
		Pass {
			CGPROGRAM

			//定义使用顶点着色器和像素着色器
			#pragma vertex vertExtractBright
			#pragma fragment fragExtractBright

			ENDCG
		}


		//这里使用的是shader名叫“Shaders/Chapter12/GaussianBlur”里的pass名叫“GAUSSIAN_BLUR_VERTICAL”的pass

		//第二个pass, 序号为1 , 使用其他shader里的pass
		UsePass "Shaders/Chapter12/GaussianBlur/GAUSSIAN_BLUR_VERTICAL"

		//第三个pass, 序号为2 , 使用其他shader里的pass
		UsePass "Shaders/Chapter12/GaussianBlur/GAUSSIAN_BLUR_HORIZONTAL"

		//第四个pass 序号为3 得到最终的bloom效果
		Pass {
			CGPROGRAM
			#pragma vertex vertBloom
			#pragma fragment fragBloom

			ENDCG
		}
	}
}
