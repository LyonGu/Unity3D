Shader "CookBookCustom/BRDFOpengl" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}

		_SpecularColor ("SpecColor", Color) = (1,1,1,1)
		_Roughness("Roughness", Float) = 0.0
		_Metallic("Metallic", Float) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf BRDF 

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		struct Input {
			float2 uv_MainTex;
		};


		fixed4 _Color;
		float4 _SpecularColor;
        float _Roughness;
        float _Metallic;

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}

		#define PI 3.141592653


		//正态分布函数：估算在受到表面粗糙度的影响下，取向方向与中间向量一致的微平面的数量 ==》微平面的取向方向与中间向量的方向越是一致，镜面反射的效果就越是强烈越是锐利
		float DistributionGGX(fixed3 N, fixed3 H, float roughness)
		{
		    float a = roughness*roughness;
		    float a2 = a*a;
		    float NdotH = max(dot(N, H), 0.0);
		    float NdotH2 = NdotH*NdotH;

		    float nom   = a2;
		    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
		    denom = PI * denom * denom;

		    return nom / max(denom, 0.001); // prevent divide by zero for roughness=0.0 and NdotH=1.0
		}

		float GeometrySchlickGGX(fixed NdotV, float roughness)
		{
		    float r = (roughness + 1.0);
		    float k = (r*r) / 8.0;

		    float nom   = NdotV;
		    float denom = NdotV * (1.0 - k) + k;

		    return nom / denom;
		}


		//几何函数：描述了微平面自成阴影的属性。当一个平面相对比较粗糙的时候，平面表面上的微平面有可能挡住其他的微平面从而减少表面所反射的光线。
		float GeometrySmith(fixed3 N, fixed3 V, fixed3 L, float roughness)
		{
		    fixed NdotV = max(dot(N, V), 0.0);
		    fixed NdotL = max(dot(N, L), 0.0);
		    fixed ggx2 = GeometrySchlickGGX(NdotV, roughness);
		    fixed ggx1 = GeometrySchlickGGX(NdotL, roughness);

		    return ggx1 * ggx2;
		}


		//菲涅尔方程：描述的是在不同的表面角下表面所反射的光线所占的比率
		fixed3 fresnelSchlick(float cosTheta, fixed3 F0)
		{
		    return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
		}


		inline fixed4 LightingBRDF(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten)
		{
			// 归一化
			lightDir = normalize(lightDir);
			viewDir = normalize(viewDir);
			fixed3 normal = normalize(s.Normal);

			fixed3 h = normalize(lightDir + viewDir);

			fixed NDotH = max(dot(normal,h),0);
			fixed HDotL = max(dot(lightDir,h),0);
			fixed NDotL = max(dot(normal,lightDir),0);
			fixed NDotV = max(dot(normal,viewDir),0);

			//计算垂直入射的反射率 
			//如果是金属,使用反照率颜色作为F0   如果是非金属，使用插值计算
		    fixed3 F0 = fixed3(0.04,0.04,0.04); 
		    F0 = lerp(F0, s.Albedo, _Metallic);
		    


			float NDF = DistributionGGX(normal, h, _Roughness);               		//正态分布函数
	        float G   = GeometrySmith(normal, viewDir, lightDir, _Roughness);       //几何函数
	        fixed3 F  = fresnelSchlick(clamp(dot(h, lightDir), 0.0, 1.0), F0);     	//菲涅尔方程

	        fixed3 nominator    = NDF * G * F; 
	        float denominator = 4 * NDotV * NDotL;

	        //高光系数
	        fixed3 specular =  nominator / max(denominator, 0.001); // prevent divide by zero for NdotV=0.0 or NdotL=0.0


	        //******************计算每个光源在反射率方程中的贡献值了
			//kS等于菲涅耳：kS表示光能中被反射的能量的比例
			//计算漫反射系数
			fixed3 kS = F;
			fixed3 kD = 1.0 - kS;	//漫反射光和镜面光不能超过1.0  折射光量比例(kD)应该等于1.0 - kS。
			kD *= 1.0 - _Metallic;	//因为金属不会折射光线，因此不会有漫反射。所以如果表面是金属的，我们会把系数kD变为0


			fixed3 Lo = fixed3(0.0,0.0,0.0);
			//Lo += (kD * s.Albedo / PI + specular) * atten * NDotL;
			Lo += (kD * s.Albedo / PI + specular)  * NDotL; //暂时不考虑衰减

			//Lo包含了漫反射和高光反射部分
			//加一个环境光照项给Lo，然后我们就拥有了片段的最后颜色：
    		//fixed3 ambient = vec3(0.03) * albedo * ao;  //ao的AO贴图的值

			fixed4 c;
            c.rgb = Lo *_LightColor0.rgb;
            c.a = s.Alpha;

            return c;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
