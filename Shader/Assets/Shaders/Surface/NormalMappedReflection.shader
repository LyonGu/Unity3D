Shader "CookBookCustom/NormalMappedReflection" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_MainTint ("Diffuse Tint", Color) = (1,1,1,1)
		_ReflAmount ("Reflection Amount", Range(0, 1)) = 1
		_Cubemap ("Cubemap", CUBE) = ""{}

		//遮罩图
		_ReflMask ("Reflection Mask", 2D) = ""{}

		//法线图
		_NormalMap ("Normal Map", 2D) = "bump" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		
		#pragma surface surf Lambert
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _ReflMask;
		sampler2D _NormalMap;
		samplerCUBE _Cubemap;
		float4 _MainTint;
		float _ReflAmount;

		struct Input {
			float2 uv_MainTex;
			float2 uv_NormalMap;
			float3 worldRefl; //使用修改后的法线信息计算反射
			INTERNAL_DATA
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;



		void surf (Input IN, inout SurfaceOutput o) {
		
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _MainTint; 


			float3 normals = UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap)).rgb;
			o.Normal = normals; //物体的平面法线将被修改 因此，我们需要使用它来影响我们的反射

			//我们可以通过声明INTERNAL_DATA来访问修改后的法线信息，
			//然后使用WorldReflectionVector (IN, o.Normal)去查找Cubemap中对应的反射信息

			float3 refl= WorldReflectionVector (IN, o.Normal);

			fixed3 reflection = texCUBE(_Cubemap, refl).rgb;

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
