Shader "CookBookCustom/BasicDiffuseCustomLight" {
	Properties {
	
		//_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_EmissiveColor ("Emissive Color", Color) = (1,1,1,1)
		_AmbientColor ("Ambient Color", Color) = (1,1,1,1)
		_MySliderValue  ("This is a Slider", Range(0,10)) = 2.5

	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		
		//自定义光照函数
		#pragma surface surf Lambert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		fixed4 _EmissiveColor;
		fixed4 _AmbientColor;
		fixed  _MySliderValue;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput  o) {
			
			fixed4 c =  pow((_EmissiveColor + _AmbientColor),  _MySliderValue);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}


		//lightDir 光的方向  atten 衰减阴影值
		inline fixed4 LightingBasicDiffus(SurfaceOutput s, fixed3 lightDir, fixed atten)
		{
			float difLight = max(0, dot (s.Normal, lightDir));
       	  	fixed4 col;
       	  	col.rgb = s.Albedo * _LightColor0.rgb * (difLight * atten * 2);
       	  	col.a = s.Alpha;
       	  	return col;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
