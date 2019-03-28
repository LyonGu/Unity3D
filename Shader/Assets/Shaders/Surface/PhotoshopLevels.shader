Shader "CookBookCustom/PhotoshopLevels" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}

		//Add the Input Levels Values
		_inBlack ("Input Black", Range(0,255)) = 0
		_inGamma ("Input Gamma", Range(0,2)) = 1.61
		_inWhite ("Input White", Range(0,255)) = 255

		//Add the Output Levels
		_outWhite ("Output White", Range(0,255)) = 255
		_outBlack ("Output Black", Range(0,255)) = 0

		_isAlphaTest("AlphaTest", Range(0,1)) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		
		#pragma surface surf Lambert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		fixed _inBlack;
		fixed _inGamma;
		fixed _inWhite;

		fixed _outWhite;
		fixed _outBlack;

		fixed  _isAlphaTest;

		struct Input {
			float2 uv_MainTex;
		};


		//参数为原帖图RGB通道中一个通道的像素值，返回经过调整色阶后的新的该通道像素值。
		float GetPixelLevel(float pixelColor)
		{
			float pixelResult;
			pixelResult = (pixelColor * 255.0);
			pixelResult = max(0, pixelResult - _inBlack);
			//伽马矫正
			pixelResult = saturate(pow(pixelResult / (_inWhite - _inBlack), _inGamma));
			pixelResult = (pixelResult * (_outWhite - _outBlack) + _outBlack)/255.0;	
			return pixelResult;

		}

		void surf (Input IN, inout SurfaceOutput o) {
			
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex);

			if(_isAlphaTest==1.0)
			{
				clip(c.a - 0.1);
			}

			float outRPixel = GetPixelLevel(c.r);
			float outGPixel = GetPixelLevel(c.g);
			float outBPixel = GetPixelLevel(c.b);

			o.Albedo = fixed3(outRPixel, outGPixel, outBPixel);
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
