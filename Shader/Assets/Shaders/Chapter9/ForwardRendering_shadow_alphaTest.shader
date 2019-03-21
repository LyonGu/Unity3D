Shader "Shaders/Chapter9/ForwardRenderingShadowAlphaTest"
{
	//两个Pass,第一个pass为ForwardBase,第二个pass为ForwardAdd
	Properties
	{
		_Color ("Color Tint", Color) = (1, 1, 1, 1)
		_MainTex ("Main Tex", 2D) = "white" {}
		_Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5
	}
	SubShader
	{
		Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
		Pass
		{
			//平行光处理
			Tags { "LightMode"="ForwardBase" }
			Cull Off

			CGPROGRAM

			//开启这个编译命令，是为了让Unity自动赋予shader需要的光照计算变量
			#pragma multi_compile_fwdbase

			#pragma vertex vert
			#pragma fragment frag

			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			fixed4 _Color;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed _Cutoff;

			struct a2v{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 texcoord : TEXCOORD0;
			};

			struct v2f{
				float4 pos : SV_POSITION;
				float3 worldNormal: TEXCOORD0;
				float3 worldPos: TEXCOORD1;
				float2 uv : TEXCOORD2;
				SHADOW_COORDS(3)  //声明一个用于阴影纹理采样的坐标
			};

			v2f vert(a2v v){
				v2f o;
			 	o.pos = UnityObjectToClipPos(v.vertex);
			 	
			 	o.worldNormal = UnityObjectToWorldNormal(v.normal);
			 	
			 	o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

			 	o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);

				//计算声明的阴影纹理坐标
			 	TRANSFER_SHADOW(o);

				return o;
			}

			fixed4 frag(v2f v): SV_Target{
				fixed3 worldNormal = normalize(v.worldNormal);

				//因为这个pass 只处理平行光 一般会用内置方法UnityWorldSpaceLightDir(o.worldPos)
				fixed3 worldLightDir = normalize(UnityWorldSpaceLightDir(v.worldPos));

				fixed4 texColor = tex2D(_MainTex, v.uv);

				clip (texColor.a - _Cutoff);
				
				fixed3 albedo = texColor.rgb * _Color.rgb;
				
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * albedo;
				
				fixed3 diffuse = _LightColor0.rgb * albedo * max(0, dot(worldNormal, worldLightDir));
							 	
			 	//同时计算衰减值和阴影值 atten就包含了阴影值 
				UNITY_LIGHT_ATTENUATION(atten, v, v.worldPos);
			 	
				return fixed4(ambient + diffuse * atten, 1.0);
			}


			ENDCG
		}

	}
	FallBack "Transparent/Cutout/VertexLit"
}
