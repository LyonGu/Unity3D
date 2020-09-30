Shader "CustomShaderGUI/Two"
{
	Properties
	{
		[KeywordEnum(Simple, Reflective)] _ShadeMode("ShadingMode", Float) = 0
		[Toggle(_TEST_BLENDMODE)] _BlendMode_TEST("Test Blend Mode", Float) = 1
		_BaseTex("BaseTex", 2D) = "white" {}
		[NoScaleOffset]_BaseTex_NoScaleOffset("BaseTex", 2D) = "white" {}
		_Strenth("Strenth", Range(0, 1)) = 1
		_MainColor("MainColor", Color) = (0, 0, 0, 1)
		_Offset("Offset", Vector) = (0, 0, 0, 1)
		_TestFloat("Float", Float) = 1
		[HideInInspector]_TestFloat_Hide("Float", Float) = 1

	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 100

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				// make fog work
				#pragma multi_compile_fog

				#pragma	multi_compile _SHADEMODE_SIMPLE _SHADEMODE_REFLECTIVE
				#pragma multi_compile __ _TEST_BLENDMODE

				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					float4 vertex : SV_POSITION;
				};

				sampler2D _BaseTex;
				float4 _BaseTex_ST;
				half _Strenth;
				half4 _MainColor;
				half4 _Offset;
				half _TestFloat;

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _BaseTex);
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					// sample the texture
					half4 Output = tex2D(_BaseTex, i.uv);
				#if _SHADEMODE_SIMPLE
					Output.rgb *= half3(1, 0, 0);
				#endif
				#if _SHADEMODE_REFLECTIVE
					Output.rgb *= half3(0, 1, 0);
				#endif

					Output.rgb *= _Strenth;

				#if _TEST_BLENDMODE
					Output.rgb *= 3;
				#endif

					Output.rgb += (_MainColor.rgb * _MainColor.a);

					// apply fog
					UNITY_APPLY_FOG(i.fogCoord, Output);
					return Output;
				}
				ENDCG
			}
		}
}