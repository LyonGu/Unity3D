Shader "CookBookCustom/SimpleAlpha" {

	//使用透明度混合
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_TransVal ("Transparency Value", Range(0,1)) = 0.5
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

		struct Input {
			float2 uv_MainTex;
		};

	
		void surf (Input IN, inout SurfaceOutput o) {
			
			fixed2 uv = IN.uv_MainTex;
			fixed4 c = tex2D (_MainTex, uv);
			o.Albedo = c.rgb;
			o.Alpha = c.a * _TransVal;  //这个是用来控制alpha测试是否通过的，o.Alpha=0你就看不见了
		}
		ENDCG
	}
	FallBack "Diffuse"
}
