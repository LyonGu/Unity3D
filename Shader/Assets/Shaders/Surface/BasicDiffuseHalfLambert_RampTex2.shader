Shader "CookBookCustom/BasicDiffuseHalfLambert_RampTex2" {
	Properties {
	
		_RampTex ("Ramp Texture", 2D) = "white" {}
		_EmissiveColor ("Emissive Color", Color) = (1,1,1,1)
		_AmbientColor ("Ambient Color", Color) = (1,1,1,1)
		_MySliderValue  ("This is a Slider", Range(0,10)) = 2.5

	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		
		//自定义光照函数
		#pragma surface surf BasicDiffus

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		fixed4 _EmissiveColor;
		fixed4 _AmbientColor;
		fixed  _MySliderValue;
		sampler2D _RampTex;

		struct Input {
			float2 uv_RampTex;
		};

		void surf (Input IN, inout SurfaceOutput  o) {
			
			fixed4 c =  pow((_EmissiveColor + _AmbientColor),  _MySliderValue);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}


		//lightDir 光的方向  atten 衰减阴影值
		inline fixed4 LightingBasicDiffus(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten)
		{
			float difLight = max(0, dot (s.Normal, lightDir));
			float rimLight = max(0, dot (s.Normal, viewDir));

			//半兰伯特光照模型
			float dif_hLambert  = difLight * 0.5 + 0.5;
			float rim_hLambert = rimLight * 0.5 + 0.5;

			float3 ramp = tex2D(_RampTex, fixed2(dif_hLambert ,rim_hLambert)).rgb;

       	  	fixed4 col;
       	  	col.rgb = s.Albedo * _LightColor0.rgb * (ramp);
       	  	col.a = s.Alpha;
       	  	return col;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
