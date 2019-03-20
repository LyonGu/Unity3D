Shader "Shaders/Chapter8/AlphaBlendMat"
{
	//单纯进行alpha测试
	Properties
	{
		_Color ("Color Tint", Color) 			= (1,1,1,1)
		_MainTex ("Main Tex", 2D) 	 			= "white" {}
		_AlphaScale ("Alpha Scale", Range(0,1))	= 1
	}
	SubShader
	{
		//在subShader里定义tags,对所有的pass都生效
		Tags {"Queue" = "Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		Pass
		{
			Tags { "LightMode"="ForwardBase" }

			ZWrite Off //关闭深度写入

			//设置混合因子
			//源颜色用SrcAlpha, 已经存在缓冲区里的颜色用OneMinusSrcAlpha
			Blend SrcAlpha OneMinusSrcAlpha 

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			
			#include "Lighting.cginc"

			fixed4 		_Color;
			sampler2D 	_MainTex;
			float4 		_MainTex_ST;
			fixed 		_AlphaScale;

			struct a2v{
				float4 vertex:  POSITION; 	//模型顶点坐标==》得到世界坐标
				float3 normal:  NORMAL;   	//模型法线信息==》计算漫反射 法线贴图 高光用
				float4 texcoord: TEXCOORD0; //纹理的坐标 ==》计算得到贴图的UV坐标
			};

			struct v2f{
				float4 pos 		    : SV_POSITION;   //这个是一定要返回的
				float3 worldNormal  : TEXCOORD0;
				float3 worldPos 	: TEXCOORD1;
				float2 uv 			: TEXCOORD2;
			};

			v2f vert(a2v i){
				v2f o;
				o.pos = UnityObjectToClipPos(i.vertex);

				//世界空间法线
				float3 worldNormal = UnityObjectToWorldNormal(i.normal);
				o.worldNormal = worldNormal;

				float3 worldPos = mul(unity_ObjectToWorld, i.vertex);
				o.worldPos = worldPos;

				//使用内置函数得到UV坐标
				o.uv = TRANSFORM_TEX(i.texcoord, _MainTex);

				return o;
			}

			fixed4 frag(v2f v): SV_Target{

				fixed3 worldNormal = normalize(v.worldNormal);
				fixed3 worldLightDir = normalize(UnityWorldSpaceLightDir(v.worldPos));

				fixed4 texColor = tex2D(_MainTex, v.uv);
	
				fixed3 albedo = texColor.rgb * _Color.rgb;

				fixed3 ambient =  UNITY_LIGHTMODEL_AMBIENT.xyz * albedo;

				fixed3 diffuse = _LightColor0 * albedo * max(dot(worldNormal, worldLightDir),0);

				return fixed4(ambient + diffuse, texColor.a * _AlphaScale);
			}

			
			ENDCG
		}
	}
	FallBack "Transparent/Cutout/VertexLit"
}
