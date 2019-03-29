Shader "CookBookCustom/Cloth" {
	Properties {
		_MainTint ("Global Tint", Color) = (1,1,1,1)

		//标准变化贴图:张贴图将会模拟缝纫的变化，防止所有表面看起来都是一样的，而更像是有岁月磨损的样子
		_BumpMap ("Normal Map", 2D) = "Bump" {}

		//细节法线贴图:这张贴图将会平铺在表面上来模拟细小的缝纫痕迹。
		_DetailBump ("Detail Normal Map", 2D) = "Bump" {}

		//细节漫反射贴图
		//使用这张贴图去乘以基本颜色来模拟布料的整体颜色，以此来为整体增加更多的深度细节和真实感，并且还能强调布料的缝纫痕迹
		_MainTex ("Fabric Weave", 2D) = "white" {}

		//菲涅尔
		_FresnelColor ("Fresnel Color", Color) = (1,1,1,1)
		_FresnelPower ("Fresnel Power", Range(0, 12)) = 3
		_RimPower ("Rim FallOff", Range(0, 12)) = 3

		//高光系数
		_SpecIntesity ("Specular Intensiity", Range(0, 1)) = 0.2 //强度系数
		_SpecWidth ("Specular Width", Range(0, 1)) = 0.2	 //指数部分系数

		// float spec = pow(nh,s.Specular * 128.0) * s.Gloss;

	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		
		#pragma surface surf Velvet

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _BumpMap;
		sampler2D _DetailBump;
		sampler2D _MainTex;
		fixed4 _MainTint;
		fixed4 _FresnelColor;
		fixed _FresnelPower;
		fixed _RimPower;
		fixed _SpecIntesity;
		fixed _SpecWidth;


		struct Input {
			float2 uv_MainTex;
			float2 uv_DetailBump;
			float2 uv_BumpMap;
		};

		//因为跟视角有关 所以需要使用这个格式的光照函数
		inline fixed4 LightingVelvet(SurfaceOutput s, fixed3 lightDir, fixed3 viewDir, fixed atten)
		{
			viewDir = normalize(viewDir);
			lightDir = normalize(lightDir);
			half3 halfVec = normalize(lightDir + viewDir); //半程向量
			fixed NdotL = max(0,dot(s.Normal,lightDir)); //漫反射光照

			fixed NdotH = max(0,dot(s.Normal, halfVec));
			float spec = pow (NdotH, s.Specular*128.0) * s.Gloss;

			//布料渲染很大程度上依赖你从什么角度观察这个平面。观察角度越倾斜，就有越多的纤维捕捉到灯光后面的光照，并增强了高光反射。（菲涅耳效应）


			//菲涅尔
			float HdotV = pow(1-max(0, dot(halfVec, viewDir)), _FresnelPower);
			float NdotE = pow(1-max(0, dot(s.Normal, viewDir)), _RimPower);
			float finalSpecMask = NdotE * HdotV;

			//Output the final color
			fixed4 c;
			c.rgb = (s.Albedo * NdotL * _LightColor0.rgb)
					 + (spec * (finalSpecMask * _FresnelColor)) * (atten * 2);
			c.a = 1.0;
			return c;

		}
		

		void surf (Input IN, inout SurfaceOutput  o) {

			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _MainTint;

			//***********使用不同的平铺率整合两个法线贴图

			/*

			基本的线性代数表明，我们可以将两个向量相加得到一个新的位置。
			因此，我们可以这样操作我们的法线贴图。我们使用UnpackNormal()函数得到标准变化贴图（Normal Variation map）的法线向量，再将其和细节法线贴图（Detail Normal map）的法线向量相加。这样得到了一个新的法线贴图
			然后，我们标准化最后的向量，来让它的范围在0到1之间。如果没有这样做，我们的法线贴图就会看起来就是错的。

			*/

			fixed3 normals = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap)).rgb;
			fixed3 detailNormals = UnpackNormal(tex2D(_DetailBump, IN.uv_DetailBump)).rgb;
			fixed3 finalNormals = normals + detailNormals;
			o.Normal = normalize(finalNormals);

			// float spec = pow(nh,s.Specular * 128.0) * s.Gloss;
			o.Specular = _SpecWidth;
			o.Gloss = _SpecIntesity;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
