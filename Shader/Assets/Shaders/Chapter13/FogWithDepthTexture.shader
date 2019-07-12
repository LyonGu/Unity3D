Shader "Shaders/Chapter13/FogWithDepthTexture" {

	//利用深度纹理来重建每个像素（UV坐标）对应的世界坐标

	//在视觉空间下从摄像机出发发射一条射线进行插值，
	//这条射线存储了该像素在世界空间下到摄像机的方向信息，
	//然后把该射线和线性化后的视觉空间下的深度值相乘，
	//再加上摄像机的世界坐标，就能得到该像素的世界坐标
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_FogDensity ("Fog Density", Float) = 1.0
		_FogColor ("Fog Color", Color) = (1, 1, 1, 1)
		_FogStart ("Fog Start", Float) = 0.0
		_FogEnd ("Fog End", Float) = 1.0
	}
	SubShader {
		CGINCLUDE
		
		#include "UnityCG.cginc"
		
		float4x4 _FrustumCornersRay;
		
		sampler2D _MainTex;
		half4 _MainTex_TexelSize;
		sampler2D _CameraDepthTexture;  //Unity会把深度纹理传给这个值
		half _FogDensity;
		fixed4 _FogColor;
		float _FogStart;
		float _FogEnd;
		
		struct v2f {
			float4 pos : SV_POSITION;
			half2 uv : TEXCOORD0;
			half2 uv_depth : TEXCOORD1;
			float4 interpolatedRay : TEXCOORD2;
		};
		
		v2f vert(appdata_img v) {
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			
			o.uv = v.texcoord;
			o.uv_depth = v.texcoord;
			
			#if UNITY_UV_STARTS_AT_TOP
			if (_MainTex_TexelSize.y < 0)
				o.uv_depth.y = 1 - o.uv_depth.y;
			#endif
			
			//这里跟矩阵的构建有关
			int index = 0;
			if (v.texcoord.x < 0.5 && v.texcoord.y < 0.5) {
				index = 0;
			} else if (v.texcoord.x > 0.5 && v.texcoord.y < 0.5) {
				index = 1;
			} else if (v.texcoord.x > 0.5 && v.texcoord.y > 0.5) {
				index = 2;
			} else {
				index = 3;
			}

			#if UNITY_UV_STARTS_AT_TOP
			if (_MainTex_TexelSize.y < 0)
				index = 3 - index;
			#endif
			
			//射线信息
			//求得视锥体对应四个边界射线的值，这个操作在vertex阶段进行，由于我们的后处理实际上就是渲染了一个Quad，上下左右四个顶点，
			//把这个射线传递给pixel阶段时，就会自动进行插值计算，也就是说在顶点阶段的方向值到pixel阶段就变成了逐像素的射线方向。
			o.interpolatedRay = _FrustumCornersRay[index];
				 	 
			return o;
		}
		
		fixed4 frag(v2f i) : SV_Target {
			//SAMPLE_DEPTH_TEXTURE采样得到的是非线性深度值
			//LinearEyeDepth函数转化为视觉空间下[0,1]的线性深度值
			//Linear01Depth函数返回一个[0,1]的线性深度值，应该是在世界空间下？
			float linearDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv_depth));
			float3 worldPos = _WorldSpaceCameraPos + linearDepth * i.interpolatedRay.xyz;
			
			//基于高度线性计算雾效因子，这里可以改成其他类型
			float fogDensity = (_FogEnd - worldPos.y) / (_FogEnd - _FogStart); 

			//基于距离 z  调整下_FogEnd和_FogStart即可
			//float fogDensity = (_FogEnd - worldPos.z) / (_FogEnd - _FogStart); 
			
			fogDensity = saturate(fogDensity * _FogDensity);
			
			fixed4 finalColor = tex2D(_MainTex, i.uv);
			finalColor.rgb = lerp(finalColor.rgb, _FogColor.rgb, fogDensity);
			
			return finalColor;
		}
		
		ENDCG
		
		Pass {
			ZTest Always Cull Off ZWrite Off
			     	
			CGPROGRAM  
			
			#pragma vertex vert  
			#pragma fragment frag  
			  
			ENDCG  
		}
	} 
	FallBack Off
}
