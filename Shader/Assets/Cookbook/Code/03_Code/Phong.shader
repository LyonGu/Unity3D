Shader "CookbookShaders/Chapter03/Phong" 
{
	Properties 
	{
		_MainTint ("Diffuse Tint", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_SpecularColor ("Specular Color", Color) = (1,1,1,1)
		_SpecPower ("Specular Power", Range(0.1,30)) = 1
		
	}
	
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Phong
		
		float4 _SpecularColor;
		sampler2D _MainTex;
		float4 _MainTint;
		float _SpecPower;
		
		inline fixed4 LightingPhong (SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten)
		{
			//Calculate diffuse and the reflection vector
			float diff = dot(s.Normal, lightDir);
			float3 reflectionVector = normalize((2.0 * s.Normal * diff) - lightDir);

			//为什么下面的计算反射方向,效果不一样
			//float3 worldNormal = UnityObjectToWorldNormal(s.Normal);
			//float3 reflectionVector = normalize(reflect(-viewDir, s.Normal));


			//Calculate the Phong specular
			float spec = pow(max(0,dot(reflectionVector, viewDir)), _SpecPower);

			//Create final color
			fixed4 c;
			c.rgb = (s.Albedo * _LightColor0.rgb * diff) + ( _SpecularColor.rgb *_LightColor0.rgb * spec) * (atten * 2);
			c.a = 1.0;
			return c;
		}

		struct Input 
		{
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) 
		{
			half4 c = tex2D (_MainTex, IN.uv_MainTex) * _MainTint;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
