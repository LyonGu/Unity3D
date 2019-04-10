Shader "Shaders/Chapter12/DistortPostEffectScreen"
{
	//全屏幕扭曲Shader,不利用遮罩图
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_NoiseTex("Base (RGB)", 2D) = "black" {}//默认给黑色，也就是不会偏移
		
	}
	SubShader
	{
		Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
		
		//定义抓取屏幕纹理
		
		ZTest Always Cull Off ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct a2v
			{
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			sampler2D _MainTex;
			sampler2D _NoiseTex;
			sampler2D _MaskTex;
			float _DistortTimeFactor;
			float _DistortStrength;

		
			
			v2f vert (a2v v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv =  v.texcoord;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				//根据时间改变采样噪声图获得随机的输出
				float4 noise = tex2D(_NoiseTex, i.uv - _Time.xy * _DistortTimeFactor);
				//以随机的输出*控制系数得到偏移值
				float2 offset = noise.xy * _DistortStrength;

				//像素采样时偏移offset
				float2 uv = offset + i.uv;
				return tex2D(_MainTex, uv);
			}
			ENDCG
		}
	}
	Fallback off
}
