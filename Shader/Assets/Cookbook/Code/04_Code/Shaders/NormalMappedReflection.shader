Shader "CookbookShaders/Chapter04/NormalMappedReflection"
{
	Properties
	{
		_MainTint ("Diffuse Tint", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_NormalMap ("Normal Map", 2D) = "bump" {}
		_Cubemap ("Cubemap", CUBE) = ""{}
		_ReflAmount ("Reflection Amount", Range(0,1)) = 0.5
	}

	SubShader
	 {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Lambert

		samplerCUBE _Cubemap;
		sampler2D _MainTex;
		sampler2D _NormalMap;
		float4 _MainTint;
		float _ReflAmount;

		struct Input
		{
			float2 uv_MainTex;
			float2 uv_NormalMap;
			float3 worldRefl;
			INTERNAL_DATA   //使用法线贴图里的法线一定要加这个，WorldReflectionVector
		};

		void surf (Input IN, inout SurfaceOutput o)
		{
			half4 c = tex2D (_MainTex, IN.uv_MainTex);

			/*
				Normal变量 是值surf函数返回的结构体SurfaceOutput 或者 SurfaceStanderOutput

				float3 worldRefl; INTERNAL_DATA 世界坐标系反射向量（修改了Normal变量时使用此方式声明），使用的时候要用WorldReflectionVector (IN, o.Normal)去包装下
				float3 worldNormal; INTERNAL_DATA 世界坐标系法线向量（修改了Normal变量时使用此方式声明，WorldNormalVector (IN, o.Normal)


				INTERNAL_DATA
				计算世界反射向量的时候 使用了法线贴图就要加上，INTERNAL_DATA 就是一个宏…
				加了后，Input结构变化
				struct Input   {   float2 uv_MainTex;   float2 uv_NormalMap;   float3 worldRefl;   half3 TtoW0; half3 TtoW1; half3 TtoW2;     };

				https://blog.csdn.net/qq_43667944/article/details/87442356

				如果表面shader为SurfaceOutput结构中的Normal赋值了，此它会包含 世界空间坐标系中法线的向量，
				通过调用方法 WorldNormalVector(IN,o.Normal) 即可得到此值。
				如果没有为Normal赋值，则直接调用IN.worldNormal即可得到此值。具体用法如：

			*/
			float3 normals = UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap)).rgb;
			o.Normal = normals; //Normal 被赋值了，后面要用WorldNormalVector得到世界坐标下的法线向量

			o.Emission = texCUBE (_Cubemap, WorldReflectionVector (IN, o.Normal)).rgb * _ReflAmount;
			o.Albedo = c.rgb * _MainTint;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
