Shader "Hidden/CC_Frost"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Scale ("Scale", Float) = 1.2
		_Sharpness ("Sharpness", Range(0, 1.0)) = 0.4
		_Darkness ("Darkness", Range(0, 2.0)) = 0.35
	}

	CGINCLUDE

		#include "UnityCG.cginc"
		#include "Colorful.cginc"

		sampler2D _MainTex;
		half _Scale;
		half _Sharpness;
		half _Darkness;

		fixed4 frag(v2f_img i):COLOR
		{
			half2 uv = i.uv;
			half4 color = tex2D(_MainTex, uv);

			half n = simpleNoise(uv.x, uv.y, 1234, 1.0);

			half dx = -0.005 + fmod(n, 0.008);
			half dy = -0.006 + fmod(n, 0.01);

			half4 frosted = tex2D(_MainTex, uv + half2(dx, dy) * _Scale);
			return frosted;
		}

		fixed4 frag_vignette(v2f_img i):COLOR
		{
			half2 uv = i.uv;
			half4 color = tex2D(_MainTex, uv);

			half n = simpleNoise(uv.x, uv.y, 1234, 1.0);

			half dx = -0.005 + fmod(n, 0.008);
			half dy = -0.006 + fmod(n, 0.01);

			half4 frosted = tex2D(_MainTex, uv + half2(dx, dy) * _Scale);

			half4 vignette = half4(1.0, 1.0, 1.0, 1.0);
			half d = distance(i.uv, half2(0.5, 0.5));
			vignette.rgb *= smoothstep(0.8, _Sharpness * 0.799, d * (_Darkness + _Sharpness));

			return lerp(frosted, color, vignette);
		}

	ENDCG

	SubShader
	{
		ZTest Always Cull Off ZWrite Off
		Fog { Mode off }

		Pass
		{
			CGPROGRAM

				#pragma vertex vert_img
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma glsl

			ENDCG
		}

		Pass
		{
			CGPROGRAM

				#pragma vertex vert_img
				#pragma fragment frag_vignette
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma glsl

			ENDCG
		}
	}

	FallBack off
}
