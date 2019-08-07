// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Occlusion/XRay"
{
	//所谓X光，就是在被遮挡的部分呈现一个其他的颜色
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags{ "Queue" = "Geometry" "RenderType" = "Opaque" }
		LOD 100

		//渲染X光效果的Pass
		Pass
		{
			
			Blend SrcAlpha One
			ZWrite Off    //一定要关闭深度写入，否则正常渲染的pass会把遮挡部分也画出来
			ZTest Greater //被遮挡的部分设置深度大于通过

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "Lighting.cginc"

			struct v2f
			{
				float4 pos : SV_POSITION;
			};
 
			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}
 
			fixed4 frag(v2f i) : SV_Target
			{
				return fixed4(1,1,1,0.5);
			}

			ENDCG
		}

		//正常渲染的Pass
		Pass
		{
			
			ZWrite On
	
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "Lighting.cginc"

			sampler2D _MainTex;
			fixed4 _MainTex_ST;

			struct v2f
			{
				float4 pos : SV_POSITION;
				fixed2 uv : TEXCOORD1;
			};
 
			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}
 
			fixed4 frag(v2f i) : SV_Target
			{
				return tex2D(_MainTex, i.uv);
			}

			ENDCG
		}
	}
}
