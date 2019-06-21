Shader "Cookbook/Chapter07/SimpleVertexColor" 
{
	Properties 
	{
		_MainTint("Global Color Tint", Color) = (1,1,1,1)
	}
	
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert vertex:vert

		float4 _MainTint;

		struct Input 
		{
			float2 uv_MainTex;
			float4 vertColor;  //input里添加了自定义变量，一定要初始化
		};
		
		void vert(inout appdata_full v, out Input o)
		{
			//一定要初始化
			UNITY_INITIALIZE_OUTPUT(Input,o);
			o.vertColor = v.color;
		}

		void surf (Input IN, inout SurfaceOutput o) 
		{
			o.Albedo = IN.vertColor.rgb * _MainTint.rgb;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
