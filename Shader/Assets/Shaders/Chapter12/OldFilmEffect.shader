Shader "Shaders/Chapter12/OldFilmEffect"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_VignetteTex ("Vignette Texture", 2D) = "white" {}
		_VignetteAmount ("Vignette Opacity", Range(0, 1)) = 1
		_ScratchesTex ("Scraches Texture", 2D) = "white" {}
		_ScratchesXSpeed ("Scraches X Speed", Float) = 10.0
		_ScratchesYSpeed ("Scraches Y Speed", Float) = 10.0
		_DustTex ("Dust Texture", 2D) = "white" {}
		_DustXSpeed ("Dust X Speed", Float) = 10.0
		_DustYSpeed ("Dust Y Speed", Float) = 10.0
		_SepiaColor ("Sepia Color", Color) = (1, 1, 1, 1)
		_EffectAmount ("Old Film Effect Amount", Range(0, 1)) = 1
		_RandomValue ("Random Value", Float) = 1.0

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
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
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

			sampler2D _MainTex;
			sampler2D _VignetteTex;
			sampler2D _ScratchesTex;
			sampler2D _DustTex;
			fixed4 _SepiaColor;
			fixed _VignetteAmount;
			fixed _ScratchesXSpeed;
			fixed _ScratchesYSpeed;
			fixed _DustXSpeed;
			fixed _DustYSpeed;
			fixed _EffectAmount;
			fixed _RandomValue;

			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				//每一帧调整render texture的UV坐标，来模拟一个闪烁的效果
				/*
					第一、二行对render texture的Y方向添加了一些偏移来达到上述的闪烁效果。它使用了Unity内置的_SinTime变量，来得到一个范围在-1到1的正弦值。然后再乘以了一个很小的值0.005，来得到一个小范围的偏移（-0.005, +0.005）。最后的值又乘以了_RandomValue变量，这是我们在脚本中定义的变量，它在Update函数中被随机生成为-1到1中的某一个值，来实现上下随机弹动的效果。在得到UV坐标后，我们在第二行使用了tex2D()函数在render texture上进行采样
				*/

				half2 renderTexUV = half2(i.uv.x, i.uv.y + (_RandomValue * _SinTime.z * 0.005));
				fixed4 renderTex = tex2D(_MainTex, renderTexUV);
				
				//眩晕纹理
				fixed4 vignetteTex = tex2D(_VignetteTex, i.uv);

				// 得到刮痕UV坐标，水平竖直随机
				half2 scratchesUV = half2(i.uv.x + (_RandomValue * _SinTime.z * _ScratchesXSpeed), 
														i.uv.y + (_Time.x * _ScratchesYSpeed));
				fixed4 scratchesTex = tex2D(_ScratchesTex, scratchesUV);
				
				// 得到灰尘纹理UV
				half2 dustUV = half2(i.uv.x + (_RandomValue * _SinTime.z * _DustXSpeed), 
														i.uv.y + (_Time.x * _DustYSpeed));
				fixed4 dustTex = tex2D(_DustTex, dustUV);


				//处理棕褐色调 先转成灰度图再给一个颜色色调
				// Get the luminosity values from the render texture using the YIQ values
				fixed lum = dot(fixed3(0.299, 0.587, 0.114), renderTex.rgb);
				
				// Add the constant calor to the lum values
				fixed4 finalColor = lum + lerp(_SepiaColor, _SepiaColor + fixed4(0.1f, 0.1f, 0.1f, 0.1f), _RandomValue);

				//合并最终颜色
				// Create a constant white color we can use to adjust opacity of effects
				fixed3 constantWhite = fixed3(1, 1, 1);
				
				// Composite together the different layers to create final Screen Effect
				finalColor = lerp(finalColor, finalColor * vignetteTex, _VignetteAmount);
				finalColor.rgb *= lerp(scratchesTex, constantWhite, _RandomValue);
				finalColor.rgb *= lerp(dustTex, constantWhite, (_RandomValue * _SinTime.z));
				finalColor = lerp(renderTex, finalColor, _EffectAmount);
				
				return finalColor;

			}
			ENDCG
		}
	}
}
