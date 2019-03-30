Shader "CookBookCustom/SimpleAlpha_BSC" {

	//使用透明度混合
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_TransVal ("Transparency Value", Range(0,1)) = 0.5

		_Brightness ("Brightness", Float) = 1
		_Saturation("Saturation", Float) = 1
		_Contrast("Contrast", Float) = 1
	}
	SubShader {
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}

		Cull Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
    
		LOD 200

		CGPROGRAM
		
		//开启透明通道 alpha是代表alpha混合意思  alphaTest才是代表alpha测试 效率低
		#pragma surface surf Lambert alpha
		
		#pragma target 3.0

		sampler2D _MainTex;
		fixed _TransVal;

		half _Brightness;
		half _Saturation;
		half _Contrast;

		struct Input {
			float2 uv_MainTex;
		};

	
		void surf (Input IN, inout SurfaceOutput o) {
			
			fixed2 uv = IN.uv_MainTex;
			fixed4 renderTex = tex2D (_MainTex, uv);

			//简单使用了一个用surfaceshader实现了vert/frag实现的功能，其实就是把finalColor最后给o.Albedo 赋值
			
			// Apply brightness
			fixed3 finalColor = renderTex.rgb * _Brightness;
			
			// Apply saturation
			fixed luminance = 0.2125 * renderTex.r + 0.7154 * renderTex.g + 0.0721 * renderTex.b;
			fixed3 luminanceColor = fixed3(luminance, luminance, luminance);
			finalColor = lerp(luminanceColor, finalColor, _Saturation);
			
			// Apply contrast
			fixed3 avgColor = fixed3(0.5, 0.5, 0.5);
			finalColor = lerp(avgColor, finalColor, _Contrast);


			o.Albedo = finalColor.rgb;
			o.Alpha = renderTex.a * _TransVal;  //这个是用来控制alpha测试是否通过的，o.Alpha=0你就看不见了
		}
		ENDCG
	}
	FallBack "Diffuse"
}
