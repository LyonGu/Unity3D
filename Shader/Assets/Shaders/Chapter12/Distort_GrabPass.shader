Shader "Shaders/Chapter12/Distort_GrabPass"
{
	//使用GrabGrass来操作屏幕纹理
	Properties
	{
		//_MainTex ("Texture", 2D) = "white" {}
		_DistortStrength("DistortStrength", Range(0,1)) = 0.2
		_DistortTimeFactor("DistortTimeFactor", Range(0,1)) = 1
		_NoiseTex("NoiseTexture", 2D) = "white" {}
		
	}
	SubShader
	{
		Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
		
		//定义抓取屏幕纹理
		GrabPass { "_GrabTempTex" }
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
				//屏幕的采样纹理坐标
				float4 grabPos : TEXCOORD1;
				
			};

			//sampler2D _MainTex;
			//float4 _MainTex_ST;
			sampler2D _GrabTempTex;
			float4 _GrabTempTex_ST;

			float _DistortStrength;
			float _DistortTimeFactor;
			sampler2D _NoiseTex;
			float4 _NoiseTex_ST;
		
			
			v2f vert (a2v v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);

				//计算抓屏的位置，其中主要是将坐标从(-1,1)转化到（0,1）空间并处理DX和GL纹理反向的问题
				o.grabPos = ComputeGrabScreenPos(o.pos);
				o.uv = TRANSFORM_TEX(v.texcoord, _NoiseTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				//首先采样噪声图，采样的uv值随着时间连续变换，而输出一个噪声图中的随机值，乘以一个扭曲快慢系数
				float4 offset = tex2D(_NoiseTex, i.uv - _Time.xy * _DistortTimeFactor);


				//让屏幕动起来，改变uv的采样坐标
				i.grabPos.xy -= offset.xy * _DistortStrength;

				//根据抓屏位置采样Grab贴图,tex2Dproj等同于tex2D(grabPos.xy / grabPos.w)
				//uv偏移后去采样贴图即可得到扭曲的效果
				fixed4 color = tex2Dproj(_GrabTempTex, i.grabPos);
				return color;
			}
			ENDCG
		}
	}
}
