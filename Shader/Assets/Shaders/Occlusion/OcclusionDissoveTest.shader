Shader "Occlusion/OcclusionDissoveTest"
{
	//像素坐标转换成屏幕坐标
	Properties
	{
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
	
			
			#include "UnityCG.cginc"

			struct a2v
			{
				float4 vertex : POSITION;
				fixed2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 screenPos:TEXCOORD0;
			};


			v2f vert (a2v v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.screenPos = ComputeGrabScreenPos(o.pos);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				
				/*
					//直接输出屏幕坐标
					float screenSpacePos = i.screenPos.y / i.screenPos.w;
					return fixed4(screenSpacePos, screenSpacePos, screenSpacePos, 1);
				*/
				

				//计算出这个顶点距离中心点的距离
				float2 screenPos = i.screenPos.xy / i.screenPos.w;
				float2 dir = float2(0.5, 0.5) - screenPos;
				float distance = sqrt(dir.x * dir.x + dir.y * dir.y);
				return fixed4(distance, distance, distance, 1);

			}
			ENDCG
		}
	}
}
