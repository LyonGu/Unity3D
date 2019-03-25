Shader "Shaders/Surface/NormalExtrusion" {
	Properties {
		_ColorTint ("Color Tint", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_BumpMap ("Normalmap", 2D) = "bump" {}
		_Amount ("Extrusion Amount", Range(-0.5, 0.5)) = 0.1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM

		/*
			表面函数sruf
			光照函数CustomLambert
			顶点修改函数 myvert
			最后颜色修改函数 mycolor
			addshadow 为shader生成一个阴影投射的pass  ("LightMode" = "ShadowCaster")
			exclude_path:deferred 不要生成延迟渲染的代码
			exclude_path:prepass  不要生成遗留延迟渲染的代码
			nometa    不要生成提取元数据的pass
		*/
		
		#pragma surface surf CustomLambert vertex:myvert finalcolor:mycolor addshadow exclude_path:deferred exclude_path:prepass nometa

		#pragma target 3.0

		fixed4 _ColorTint;
		sampler2D _MainTex;
		sampler2D _BumpMap;
		half _Amount;

		struct Input{
			//定义纹理坐标
			float2 uv_MainTex;
			float2 uv_BumpMap;
		};

		//自定义顶点修改函数
		void myvert (inout appdata_full v) {
			v.vertex.xyz += v.normal * _Amount;
		}

		//表面函数
		void surf(Input IN, inout SurfaceOutput o)
		{
			fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = tex.rgb;
			o.Alpha = tex.a;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
		}

		//光照函数
		half4 LightingCustomLambert(SurfaceOutput s, half3 lightDir, half atten)
		{
			half NdotL = dot(s.Normal, lightDir);
			half4 c;
			c.rgb = s.Albedo * _LightColor0.rgb * (NdotL * atten);
			c.a = s.Alpha;
			return c;

		}

		//最后颜色修改函数
		void mycolor (Input IN, SurfaceOutput o, inout fixed4 color) {
			color *= _ColorTint;
		}

		ENDCG
	}
	FallBack "Legacy Shaders/Diffuse"
}
