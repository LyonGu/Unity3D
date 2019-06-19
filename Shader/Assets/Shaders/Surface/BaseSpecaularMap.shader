Shader "CookBookCustom/BaseSpecaularMap" {

	//使用自定义的SurfaceOutput
	//使用高光贴图技术就是使用贴图来控制高光的颜色和强度
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}

		_SpecularColor ("SpecularColor", Color) = (1,1,1,1)
        _SpecularMask ("Specular Texture", 2D) = "white"{}
        _SpecPower("Specular Power", Range(0.1, 120)) = 3

	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf CustomPhong 

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0
		
		sampler2D _MainTex;
        sampler2D _SpecularMask;
        float4 _SpecularColor;
        float _SpecPower;
        fixed4 _Color;

		struct Input {
			float2 uv_MainTex;
		};

		struct SurfaceCustomOutput
        {
            fixed3 Albedo;
            fixed3 Normal;
            fixed3 Emission;
            fixed3 SpecularColor;
            half Specular;
            fixed Gloss;
            fixed Alpha;
        };

		//使用自定义surface结构
		void surf (Input IN, inout SurfaceCustomOutput o) {

			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			float4 specMask = tex2D(_SpecularMask, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
			o.Specular = specMask.r;
            o.SpecularColor = _SpecularColor;

			//o.Gloss = 1.0;
		}

		//自定义光照结构使用自定义surface结构
		inline fixed4 LightingCustomPhong(SurfaceCustomOutput s, fixed3 lightDir, half3 viewDir, fixed atten)
		{
			// 归一化
			lightDir = normalize(lightDir);
			viewDir = normalize(viewDir);
			fixed3 normal = normalize(s.Normal);

			fixed3 h = normalize(lightDir + viewDir);

			//漫反射系数
			fixed diff = max(dot(normal, lightDir),0);

			//高光系数
			float spec = pow(max(0.0f, dot(normal, h)), _SpecPower); 

			//漫反射颜色
			fixed3 diffColor = s.Albedo * _LightColor0.rgb * diff;

			//高光颜色
			fixed3 specColor = spec * _SpecularColor.rgb * s.Specular * _LightColor0.rgb; //s.Specular里面存的就是高光贴图里的值

			fixed4 c;
            c.rgb = diffColor + specColor;
            c.a = s.Alpha;

            return c;
		}

		ENDCG
	}
	FallBack "Diffuse"
}
