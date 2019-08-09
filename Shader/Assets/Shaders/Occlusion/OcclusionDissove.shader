Shader "Occlusion/OcclusionDissove"
{
	Properties
	{
		_Diffuse("Diffuse", Color) = (1,1,1,1)
		_DissolveColorA("Dissolve Color A", Color) = (0,0,0,0)
		_DissolveColorB("Dissolve Color B", Color) = (1,1,1,1)

		_MainTex("Base 2D", 2D) = "white"{}
		_DissolveMap("DissolveMap", 2D) = "white"{}

		_DissolveThreshold("DissolveThreshold", Range(0,2)) = 0

		_ColorFactorA("ColorFactorA", Range(0,1)) = 0.7
		_ColorFactorB("ColorFactorB", Range(0,1)) = 0.8
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
			#include "Lighting.cginc"

			fixed4 _Diffuse;
			fixed4 _DissolveColorA;
			fixed4 _DissolveColorB;

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _DissolveMap;
			float4 _DissolveMap_ST;

			half _DissolveThreshold;
			half _ColorFactorA;
			half _ColorFactorB;


			struct a2v
			{
				float4 vertex : POSITION;
				fixed2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				fixed2 uv : TEXCOORD0;
				float3 worldNormal:TEXCOORD1;
				float4 screenPos : TEXCOORD2;
				float3 worldPos :TEXCOORD3;
			};

			
			v2f vert (a2v v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.screenPos = ComputeGrabScreenPos(o.pos);  //根据clip坐标计算屏幕坐标
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float2 screenPos = i.screenPos.xy / i.screenPos.w;
				//计算距离中心点距离
				float2 dir = float2(0.5, 0.5) - screenPos;
				float distance = sqrt(dir.x * dir.x + dir.y * dir.y);
				//距离中心点近的才进行溶解处理
				float disolveFactor = (0.5 - distance) * _DissolveThreshold;
				//采样Dissolve Map
				fixed4 dissolveValue = tex2D(_DissolveMap, i.uv);
				//小于阈值的部分直接discard
				if (dissolveValue.r < disolveFactor)
				{
					discard;
				}

				//Diffuse + Ambient光照计算
				fixed3 worldNormal = normalize(i.worldNormal);
				fixed3 worldLightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));
				float3 lambert = dot(worldNormal, worldLightDir);
				lambert = lambert * 0.5 + 0.5;
				fixed3 albedo = lambert * _Diffuse.xyz * _LightColor0.xyz + UNITY_LIGHTMODEL_AMBIENT.xyz;
				fixed3 color = tex2D(_MainTex, i.uv).rgb * albedo;

				//这里为了比较方便，直接用color和最终的边缘lerp了
				float lerpValue = disolveFactor / dissolveValue.r;
				if (lerpValue > _ColorFactorA)
				{
					if (lerpValue > _ColorFactorB)
						return _DissolveColorB;
					return _DissolveColorA;
				}
				return fixed4(color, 1);

			}
			ENDCG
		}
	}
}
