Shader "Shaders/Surface/BumpedDiffuse" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpMap ("Normal Map",2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM

		//表面函数为surf, 光照函数为Lambert，Lambert为Unity内置的
		//Lambert为兰伯特模型，其实就是Phone模型
		#pragma surface surf Lambert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _BumpMap;
		fixed4 _Color;

		struct Input {

			//定义uv坐标 uv_xxxxx
			float2 uv_MainTex;
			float2 uv_BumpMap;
		};



		void surf (Input IN, inout SurfaceOutput o) {
			
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
		}
		ENDCG
	}
	FallBack "Legacy Shaders/Diffuse"
}
