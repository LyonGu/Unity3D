
Shader "Shaders/Chapter12/OutlinePrePass"
{	
	Properties {
		_OutlinePrePassOutline ("Outline", Range(0, 1)) = 0.1
		_OutlinePrePassLineColor ("Outline Color", Color) = (1, 0, 0, 1)
		
	}
	//描边Shader（输出纯色）
	SubShader
	{
		//描边使用两个Pass，第一个pass沿法线挤出一点，只输出描边的颜色
		Pass
		{	
			CGPROGRAM
				#include "UnityCG.cginc"

				float _OutlinePrePassOutline;
				fixed4 _OutlinePrePassLineColor;
				
				struct v2f
				{
					float4 pos : SV_POSITION;
				};
				
				v2f vert(appdata_full v)
				{	
					v2f o;
					//把顶点和法线都转到世界空间下
					float4 pos = float4(UnityObjectToViewPos(v.vertex), 1.0);
					float3 normal = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);  

					normal.z = -0.5;
					pos = pos + float4(normalize(normal), 0) * _OutlinePrePassOutline;
					o.pos = mul(UNITY_MATRIX_P, pos);

					return o;
				}
				
				fixed4 frag(v2f i) : SV_Target
				{
					//这个Pass直接输出描边颜色
					return fixed4(_OutlinePrePassLineColor.rgb,1);
				}
			
				//使用vert函数和frag函数
				#pragma vertex vert
				#pragma fragment frag
			ENDCG
		}
	}
}
