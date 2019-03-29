Shader "CookBookCustom/SimpleReflection" {
	Properties {
		
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_MainTint ("Diffuse Tint",Color) = (1,1,1,1)
		_Cubemap ("CubeMap",CUBE) = ""{}
		_ReflAmount ("Reflection Amount", Range(0.01,1)) = 0.5
		
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		
		#pragma surface surf Lambert 

		
		#pragma target 3.0

		sampler2D _MainTex;
		samplerCUBE _Cubemap;
		fixed4 _MainTint;
		fixed _ReflAmount;

		//Input 内置变量
		/*
		float3 viewDir 定义了视角方向
		float4 with COLOR semantic 每个顶点(per-vertex)颜色的插值。
		float4 screenPos 屏幕坐标位置
		float3 worldPos 世界坐标位置
		float3 worldRefl 世界坐标系反射向量（未修改Normal变量时要用此变量）
		float3 worldNormal 世界坐标系法线方向 （未修改Normal变量时要用此变量）
		float3 worldRefl; INTERNAL_DATA 世界坐标系反射向量（修改了Normal变量时使用此方式声明），使用的时候要用WorldReflectionVector (IN, o.Normal)去包装下
		float3 worldNormal; INTERNAL_DATA 世界坐标系反射向量（修改了Normal变量时使用此方式声明

		*/
		struct Input {
			float2 uv_MainTex;
			float3 worldRefl;
		};

		

		void surf (Input IN, inout SurfaceOutput o) {
			
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _MainTint;
			o.Emission = texCUBE(_Cubemap, IN.worldRefl).rgb * _ReflAmount; //自发光属性？？
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
