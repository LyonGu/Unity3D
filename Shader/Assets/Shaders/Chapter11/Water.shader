Shader "Shaders/Chapter11/Water"
{
	//使用正弦函数来模拟水流的波动效果
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color Tint", Color) = (1,1,1,1)

		//水流波动的幅度
		_Magnitude ("Distortion Magnitude", Float) = 1

		//波动频率
		_Frequency ("Distortion Frequency", Float) = 1

		//波长的倒数，_InvWaveLength越大，波长越小
		_InvWaveLength ("Distortion Inverse Wave Length", Float) = 10
 		
		//水流速度
 		_Speed ("Speed", Float) = 0.5

	}
	SubShader
	{
		//设置透明效果， DisableBatching为关闭批处理
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "DisableBatching"="True"}
		Pass
		{
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off //关闭剔除，让水流每个面都可以显示

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;
			float _Magnitude;
			float _Frequency;
			float _InvWaveLength;
			float _Speed;

			struct a2v {
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
			};
			
			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(a2v i){
				v2f o;

				float4 offset;
				offset.yzw = float3(0.0, 0.0, 0.0);

				float dis = i.vertex.x * _InvWaveLength + i.vertex.y * _InvWaveLength + i.vertex.z * _InvWaveLength;
				offset.x = sin(_Frequency * _Time.y + dis) * _Magnitude;

				o.pos = UnityObjectToClipPos(i.vertex + offset);

				//得到_MainTex的UV坐标
				o.uv = TRANSFORM_TEX(i.texcoord, _MainTex);
				o.uv +=  float2(0.0, _Time.y * _Speed);
				return o;

			}

			fixed4 frag(v2f i) : SV_Target {
				fixed4 c = tex2D(_MainTex, i.uv);
				c.rgb *= _Color.rgb;
				
				return c;
			} 

			ENDCG
		}
	}
	FallBack "Transparent/VertexLit"
}
