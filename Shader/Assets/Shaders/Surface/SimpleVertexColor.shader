Shader "CookBookCustom/SimpleVertexColor" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)

	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		
		#pragma surface surf Lambert vertex:vert

		
		#pragma target 3.0

		#include "UnityCG.cginc"

		sampler2D _MainTex;
		fixed4 _Color;

		struct Input {
			float2 uv_MainTex;
			float4 vertColor;
		};

		
		void vert(inout appdata_full v, out Input o)
		{
			//一定要先初始化
			UNITY_INITIALIZE_OUTPUT(Input,o);
			o.vertColor = v.color;
		}


		void surf (Input IN, inout SurfaceOutput o) {
			o.Albedo = IN.vertColor.rgb * _Color.rgb;
		}

		ENDCG
	}
	FallBack "Diffuse"
}
