Shader "FakeReflection/FakeReflection"
{
	Properties
	{
		[HideInInspector] _Cull ("Cull", Int) = 0.0
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100
		Cull [_Cull]

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#pragma multi_compile _ _FAKE_REFLECTION_LOCAL
			#pragma multi_compile _ _FAKE_REFLECTION_INNER

			#include "FakeReflection.cginc"

			DECLARE_FAKE_REFLECTION;

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				V2F_FAKE_REFLECTION(2,3)
			};
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				FAKE_REFLECTION_TRANSFORM(o, v.vertex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				half3 c = half3(0,0,0);
				FAKE_REFLECTION_APPLY(i, c, 0);

				UNITY_APPLY_FOG(i.fogCoord, c);
				return fixed4(c.rgb, 1);
			}
			ENDCG
		}
	}
}
