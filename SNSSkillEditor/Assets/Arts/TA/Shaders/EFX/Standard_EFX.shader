Shader "KingsGroup/EFX/Standard"
{
	Properties
	{
		[HDR]_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}

		[HideInInspector] _BlendMode("Blend Mode", Int) = 0.0
		[HideInInspector] _SrcBlend("Src Blend", Int) = 1.0
		[HideInInspector] _DstBlend("Dst Blend", Int) = 0.0
		[HideInInspector] _ZWrite("ZWrite", Int) = 1.0
		[HideInInspector] _CullMode("Cull Mode", Int) = 2
	}

	Category
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }

		SubShader
		{
			Pass
			{
				Blend[_SrcBlend][_DstBlend]
				ZWrite[_ZWrite]
				Cull[_CullMode]
				Lighting Off
				Fog { Mode Off }

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest

				#include "UnityCG.cginc"

				sampler2D _MainTex;
				fixed4 _Color;
		
				struct appdata
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f
				{
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
				};
		
				float4 _MainTex_ST;

				v2f vert (appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.color = v.color;
					o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
					return o;
				}
		
				fixed4 frag (v2f i) : SV_Target
				{
					fixed4 result = i.color * _Color * tex2D(_MainTex, i.texcoord);
					return result;
				}
				ENDCG 
			}
		}
	}
}
