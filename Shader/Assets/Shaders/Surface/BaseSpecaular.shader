Shader "CookBookCustom/BaseSpecaular" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}

		//_SpecColor是Unity内置的一个变量，在Properties里声明是为了可以在Inspector面板里调节它，所以在后面我们没有使用这个变量。它控制高光的颜色。
		_SpecColor ("Specular Color", Color) = (1,1,1,1)
        _SpecPower ("Specular Power", Range(0,1)) = 0.5

	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf BlinnPhong 

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		float _SpecPower;
		struct Input {
			float2 uv_MainTex;
		};


		fixed4 _Color;


		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
			o.Specular = _SpecPower;
			o.Gloss = 1.0;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
