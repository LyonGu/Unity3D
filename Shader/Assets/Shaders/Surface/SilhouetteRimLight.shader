Shader "CookBookCustom/SilhouetteRimLight" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("MainTex", 2D) = "white" {}
		_DotProduct("Rim effect", Range(-1,1)) = 0.25
	}
	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 200
		Cull off

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Lambert alpha:fade nolighting

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0
		float _DotProduct;
		fixed4 _Color;
		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float3 worldNormal;
			float3 viewDir;
		};


		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;

			float border1 = 1 - (abs(dot(IN.viewDir, IN.worldNormal)));
			float alpha = (border1 * (1 - _DotProduct) + _DotProduct);
			o.Alpha = c.a * alpha;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
