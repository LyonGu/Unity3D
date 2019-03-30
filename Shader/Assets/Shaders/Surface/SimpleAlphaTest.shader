Shader "CookBookCustom/SimpleAlphaTest" {

	//使用透明度测试
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}

		//必须要用_Cutoff属性 因为默认shader用了这个
		_Cutoff ("Cutoff Value", Range(0,1.5)) = 0.5
	}
	SubShader {
		Tags {"Queue" = "AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}

		
		LOD 200

		CGPROGRAM
		
		//开启透明通道 alpha是代表alpha混合意思  alphatest才是代表alpha测试 效率低
		#pragma surface surf Lambert alphatest
		
		#pragma target 3.0

		sampler2D _MainTex;
		fixed 		_Cutoff;
		

		struct Input {
			float2 uv_MainTex;
		};

	
		void surf (Input IN, inout SurfaceOutput o) {
			
			fixed2 uv = IN.uv_MainTex;
			fixed4 c = tex2D (_MainTex, uv);

			clip (c.a - _Cutoff);
			o.Albedo = c.rgb;
			o.Alpha = c.a;  //这个是用来控制alpha测试是否通过的，o.Alpha=0你就看不见了
		}
		ENDCG
	}
	FallBack "Transparent/Cutout/VertexLit"
}
