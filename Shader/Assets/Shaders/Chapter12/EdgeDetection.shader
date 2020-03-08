Shader "Shaders/Chapter12/EdgeDetection"
{
	//利用卷积实现边缘检测
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_EdgeOnly ("Edge Only", Float) = 1.0
		_EdgeColor ("Edge Color", Color) = (0, 0, 0, 1)
		_BackgroundColor ("Background Color", Color) = (1, 1, 1, 1)
	}
	SubShader
	{

		Pass
		{
			//标配
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragSobel


			#include "UnityCG.cginc"

			sampler2D _MainTex;

			//xxx_TexelSize是Unity提供的访问对应每个纹素的大小，
			//比如一张512x512,该值大约为 1/512
			uniform half4 _MainTex_TexelSize;

			fixed _EdgeOnly;
			fixed4 _EdgeColor;
			fixed4 _BackgroundColor;

			struct a2v{
				float4 vertex: POSITION;
				float2 texcoord: TEXCOORD0;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				half2 uv[9] : TEXCOORD0;
			};

			v2f vert(a2v v)
			{
				v2f o;
				o.pos =  UnityObjectToClipPos(v.vertex);
				half2 uv = v.texcoord;

				//采样纹理的坐标放到顶点着色器（一般会放到像素着色器）可以减少运算 提升性能
				o.uv[0] = uv + _MainTex_TexelSize.xy * half2(-1, -1);
				o.uv[1] = uv + _MainTex_TexelSize.xy * half2(0, -1);
				o.uv[2] = uv + _MainTex_TexelSize.xy * half2(1, -1);
				o.uv[3] = uv + _MainTex_TexelSize.xy * half2(-1, 0);
				o.uv[4] = uv + _MainTex_TexelSize.xy * half2(0, 0);
				o.uv[5] = uv + _MainTex_TexelSize.xy * half2(1, 0);
				o.uv[6] = uv + _MainTex_TexelSize.xy * half2(-1, 1);
				o.uv[7] = uv + _MainTex_TexelSize.xy * half2(0, 1);
				o.uv[8] = uv + _MainTex_TexelSize.xy * half2(1, 1);

				return o;
			}

			//自定义函数
			fixed luminance(fixed4 color) {
				return  0.2125 * color.r + 0.7154 * color.g + 0.0721 * color.b;
			}

			half Sobel(v2f i) {
				const half Gx[9] = {-1,  0,  1,
									-2,  0,  2,
									-1,  0,  1};
				const half Gy[9] = {-1, -2, -1,
									0,  0,  0,
									1,  2,  1};

				//for循环在某些andorid机上不支持，可以考虑展开
				half texColor;
				half edgeX = 0;
				half edgeY = 0;
				for (int it = 0; it < 9; it++) {
					texColor = luminance(tex2D(_MainTex, i.uv[it]));
					edgeX += texColor * Gx[it];
					edgeY += texColor * Gy[it];
				}

				half edge = abs(edgeX) + abs(edgeY);

				return edge;
			}

			fixed4 fragSobel(v2f i) : SV_Target {
				half edge = Sobel(i);

				fixed4 color = tex2D(_MainTex, i.uv[4]);
				fixed4 withEdgeColor = lerp(color, _EdgeColor,  edge);
				fixed4 onlyEdgeColor = lerp(_BackgroundColor, _EdgeColor, edge);
				return lerp(withEdgeColor, onlyEdgeColor, _EdgeOnly);
 			}


			ENDCG

		}
	}
	Fallback Off
}
