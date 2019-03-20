Shader "Shaders/Chapter7/NormalMapTangentSpaceMat"
{
	Properties
	{
		_Color			("Color Tint", Color) 		= (1.0,1.0,1.0,1.0)
		_MainTex 		("Main Tex", 2D) 			= "white" {}
		_BumpMap 		("Normal Map", 2D) 			= "white" {}
		_BumpScale 		("Bump Scale", float) 		= 1.0
		_Specular		("Specular", Color)			= (1.0,1.0,1.0,1.0)
		_Gloss			("Gloss", Range(8.0,256))	= 20

	}
	SubShader
	{
		Pass
		{
			Tags {"LightMode" = "ForwardBase"}
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag

			#include "Lighting.cginc"

			//属性定义
			fixed4 		_Color;
			sampler2D 	_MainTex;
			float4 		_MainTex_ST;
			sampler     _BumpMap;
			float4      _BumpMap_ST;
			float 		_BumpScale;
			fixed4 		_Specular;
			float 		_Gloss;

			struct a2v{
				float4 vertex 	: POSITION;
				float3 normal 	: NORMAL;
				float4 tangent 	: TANGENT;
				float4 texcoord	: TEXCOORD0;
			};

			struct v2f{
				float4 pos 		: SV_POSITION;
				float4 uv  		: TEXCOORD0;   //同时存储MainTex和normalMap的纹理坐标
				float3 lightDir : TEXCOORD1;
				float3 viewDir 	: TEXCOORD2;
			};

			v2f vert(a2v i){
				v2f o;
				o.pos = UnityObjectToClipPos(i.vertex);
				o.uv.xy = i.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				o.uv.zw = i.texcoord.xy * _BumpMap_ST.xy + _BumpMap_ST.zw;

				
				//法线贴图中的法线向量在切线空间中，法线永远指着正z方向
				//直接使用TBN矩阵：这个矩阵可以把切线坐标空间的向量转换到世界坐标空间
				//TBN矩阵的逆矩阵：把世界坐标空间的向量转换到切线坐标空间

				//计算副切线

			}


			ENDCG
		}
	}
}
