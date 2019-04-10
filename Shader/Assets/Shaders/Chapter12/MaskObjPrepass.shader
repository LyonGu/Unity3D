Shader "Shaders/Chapter12/MaskObjPrepass"
{
	//Mask图生成shader
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			Cull Off

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				struct a2v
				{
					float4 vertex : POSITION;

				};
			
				struct v2f
				{
					float4 pos : SV_POSITION;
				};
				
				v2f vert(a2v v)
				{
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					return o;
				}
				
				fixed4 frag(v2f i) : SV_Target
				{
					//这个Pass直接输出颜色
					return fixed4(1,1,1,1);
				}
			ENDCG
		}
	}
}
