Shader "SampleUV/DissolveEffectX"
{
	//按照方向消失或重现效果: 使用模型空间坐标采样
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_DissolveVector("DissolveVector", Vector) = (0,0,0,0)
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

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _DissolveVector;

			struct a2v
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 worldNormal : NORMAL;
				float2 uv : TEXCOORD0;
				float3 worldLight : TEXCOORD1;
				float4 objPos : TEXCOORD2;
			};

			
			
			v2f vert (a2v v)
			{
				v2f o;
				o.objPos = v.vertex;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.worldNormal = UnityObjectToWorldNormal(v.vertex);
				o.worldLight = WorldSpaceLightDir(v.vertex);
				  

				//居然使用场景中只满足一个平行光的光源会比 先计算世界pos然后算出光的方向 表现不一致，前者更亮
				//o.worldLight = UnityObjectToWorldDir(_WorldSpaceLightPos0.xyz);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				half3 normal = normalize(i.worldNormal);
				half3 light = normalize(i.worldLight);
				fixed diff = dot(normal, light);
				diff = diff * 0.5 + 0.5;

				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * col;
				//不满足条件的discard

				clip(i.objPos.xyz - _DissolveVector.xyz);
				fixed4 c;
				c.rgb = diff * col + ambient;
				c.a = 1;
				return c;
			}
			ENDCG
		}
	}
}
