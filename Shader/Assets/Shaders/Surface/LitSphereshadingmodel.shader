Shader "CookBookCustom/LitSphereshadingmodel" {

	//这种技术的缺点就是它不能根据真实的灯光实时更新。它的光源看起来就像是被固定在摄像机朝向的某个位置上，就像贴图在视图上被投影到对象上一样。
	Properties {
		_MainTint  ("Diffuse", Color) = (1,1,1,1)
		_MainTex ("Base  (RGB)", 2D) = "white" {}
		_NormalMap("Normal Map", 2D) = "bump"{}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM

		//定义自己的光照模型以及顶点函数
		#pragma surface surf Unlit vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		fixed4 _MainTint;
		sampler2D _MainTex;
		sampler2D _NormalMap;

		struct Input {
			float2 uv_MainTex;
			float2 uv_NormalMap;

			//自定义属性
			float3 tan1;
			float3 tan2;

		};


		//自定义顶点函数, 返回Input类型，这样就可以加自定义属性了
		void vert (inout appdata_full v, out Input o) 
		{
			UNITY_INITIALIZE_OUTPUT(Input,o);

			//把模型空间转到切线空间
			TANGENT_SPACE_ROTATION;
            o.tan1 = mul(rotation, UNITY_MATRIX_IT_MV[0].xyz);
            o.tan2 = mul(rotation, UNITY_MATRIX_IT_MV[1].xyz);
		}

		void surf (Input IN, inout SurfaceOutput  o) {

			float3 normals = UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap));
			o.Normal = normals;

			float2 litSphereUV;

			//转到切线空间，dot后得到的时切线空间下的xy值
			litSphereUV.x = dot(IN.tan1, o.Normal);
			litSphereUV.y = dot(IN.tan2, o.Normal);
			
			//从-1到1 转到 0到1
			half4 c = tex2D (_MainTex, litSphereUV*0.5+0.5);
			o.Albedo = c.rgb * _MainTint;
			o.Alpha = c.a;

		}

		//自定义光照函数
		inline fixed4 LightingUnlit (SurfaceOutput s, fixed3 lightDir, fixed atten)
		{
			fixed4 c = fixed4(1,1,1,1);
			c.rgb = s.Albedo;
			c.a = s.Alpha;
			return c;

		}


		ENDCG
	}
	FallBack "Diffuse"
}
