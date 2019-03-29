Shader "CookBookCustom/MaskedReflection" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_MainTint ("Diffuse Tint", Color) = (1,1,1,1)
		_ReflAmount ("Reflection Amount", Range(0, 1)) = 1
		_Cubemap ("Cubemap", CUBE) = ""{}

		//遮罩图
		_ReflMask ("Reflection Mask", 2D) = ""{}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		
		#pragma surface surf Lambert
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _ReflMask;
		samplerCUBE _Cubemap;
		float4 _MainTint;
		float _ReflAmount;

		struct Input {
			float2 uv_MainTex;
			float3 worldRefl;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;



		void surf (Input IN, inout SurfaceOutput o) {
		
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _MainTint; 

			fixed3 reflection = texCUBE(_Cubemap, IN.worldRefl).rgb;
			fixed4 reflMask = tex2D(_ReflMask, IN.uv_MainTex); // 返回的是float4

			o.Albedo = c.rgb;
			//_ReflMask图里所有的通道值一样
			o.Emission = (reflection * reflMask.r) * _ReflAmount; 
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
