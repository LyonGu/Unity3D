Shader "CookBookCustom/BRDF" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}

		_SpecPower ("Specular Power", Range(0,1)) = 0.5
		_SpecularColor ("SpecColor", Color) = (1,1,1,1)

	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf BRDF 

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		struct Input {
			float2 uv_MainTex;
		};


		fixed4 _Color;
		float4 _SpecularColor;
        float _SpecPower;


		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}

		#define PI 3.141592653



		inline fixed4 LightingBRDF(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten)
		{
			// 归一化
			lightDir = normalize(lightDir);
			viewDir = normalize(viewDir);
			fixed3 normal = normalize(s.Normal);

			fixed3 h = normalize(lightDir + viewDir);

			fixed NDotH = max(dot(normal,h),0);
			fixed HDotL = max(dot(lightDir,h),0);
			fixed NDotL = max(dot(normal,lightDir),0);
			fixed NDotV = max(dot(normal,viewDir),0);

			//分布
			float D = (_SpecPower + 2) * pow(NDotH ,_SpecPower) /8.0;

			//菲尼尔系数
			float F = _SpecularColor + (1 - _SpecularColor) * pow(1-HDotL, 5);

			//高光遮盖系数
			float K = 2/sqrt( PI  * (_SpecPower + 2));
			float V = (NDotL*(1- K)+ K)*(NDotV*(1-K)+K);
			V = 1/V;

			//最终高光系数
			float allSpec = D * F * V;

			fixed diff = NDotL;

			//能量守恒：allSpec 高光部分  (1 - allSpec) * diff 漫反射部分
			float tmpResult = allSpec  + (1 - allSpec) * diff;


			fixed4 c;
            c.rgb = tmpResult * s.Albedo * _LightColor0.rgb;
            c.a = s.Alpha;

            return c;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
