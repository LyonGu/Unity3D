Shader "CookBookCustom/TextureBlending" {

	//为了减少textures的数量，我们可以看看Shader中使用的那些图片可以合并成一张，以此来优化性能。
	Properties {
		_MainTint("Diffuse Tint", Color) = (1,1,1,1)

		_ColorA("Terrain Color A", Color)= (1,1,1,1)
		_ColorB("Terrain Color B", Color)= (1,1,1,1)
		_RTexture ("Red Channel Texture", 2D) = ""{}
		_GTexture ("Green Channel Texture", 2D) = ""{}
		_BTexture ("Blue Channel Texture", 2D) = ""{}
		_ATexture ("Alpha Channel Texture", 2D) = ""{}
		_BlendTex ("Blend Texture", 2D) = ""{}

	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		
		#pragma surface surf Lambert 

		#pragma target 3.0

		sampler2D _RTexture;
		sampler2D _GTexture;
		sampler2D _BTexture;
		sampler2D _ATexture;
		sampler2D _BlendTex;

		fixed4 _MainTint;
		fixed4 _ColorA;
		fixed4 _ColorB;

		//在结构体Input中定义了大于等于3个的额外的uv变量信息。UV坐标变量不能大于3个
		struct Input {
			float2 uv_BlendTex;
		};



		void surf (Input IN, inout SurfaceOutput o) {

			//得到每一张贴图的颜色
			float4 blendData = tex2D(_BlendTex, IN.uv_BlendTex);
			
			//Get the data from the textures we want to blend
			float4 rTexData = tex2D(_RTexture, IN.uv_BlendTex);
			float4 gTexData = tex2D(_GTexture, IN.uv_BlendTex);
			float4 bTexData = tex2D(_BTexture, IN.uv_BlendTex);
			float4 aTexData = tex2D(_ATexture, IN.uv_BlendTex);


			//lerp进行插值
			float4 finalColor;
			finalColor = lerp(rTexData, gTexData, blendData.g);
			finalColor = lerp(finalColor, bTexData, blendData.b);
			finalColor = lerp(finalColor, aTexData, blendData.a);
			finalColor.a = 1.0;

			//blending texture的R通道值混合两个颜色色调值，并将结果与之前的混合值相乘
			float4 terrainLayers = lerp(_ColorA, _ColorB, blendData.r);
			finalColor *= terrainLayers;
			finalColor = saturate(finalColor);
				
			o.Albedo = finalColor.rgb * _MainTint.rgb;
			o.Alpha = finalColor.a;


		}
		ENDCG
	}
	FallBack "Diffuse"
}
