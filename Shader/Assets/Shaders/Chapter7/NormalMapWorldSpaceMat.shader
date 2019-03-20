Shader "Shaders/Chapter7/NormalMapWorldSpaceMat"
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
				float4 vertex 	: POSITION;   //模型空间中顶点的位置
				float3 normal 	: NORMAL;     //模型空间的法线信息 
				float4 tangent 	: TANGENT;    //模型空间的切线信息 
				float4 texcoord	: TEXCOORD0;
			};

			struct v2f{
				float4 pos 		: SV_POSITION;
				float4 uv  		: TEXCOORD0;   //同时存储MainTex和normalMap的纹理坐标
				float4 TtoW0 	: TEXCOORD1;  
				float4 TtoW1 	: TEXCOORD2;  
				float4 TtoW2 	: TEXCOORD3; 
			};


			//法线贴图中的法线向量在切线空间中，法线永远指着正z方向
			//直接使用TBN矩阵：这个矩阵可以把切线坐标空间的向量转换到世界坐标空间
			//TBN矩阵的逆矩阵：把世界坐标空间的向量转换到切线坐标空间

			v2f vert(a2v i){
				v2f o;
				o.pos = UnityObjectToClipPos(i.vertex);

				//实际上_MainTex 和 _BumoMap使用同一组纹理坐标
				o.uv.xy = i.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				o.uv.zw = i.texcoord.xy * _BumpMap_ST.xy + _BumpMap_ST.zw;


				float3 worldPos = mul(unity_ObjectToWorld, i.vertex).xyz; 

				//计算世界空间下的法线和切线方向
				fixed3 worldNormal = normalize(UnityObjectToWorldNormal(i.normal));  
				fixed3 worldTangent = normalize(UnityObjectToWorldDir(i.tangent.xyz)); 

				fixed3 worldBinormal = cross(worldNormal, worldTangent) * i.tangent.w; 

				//计算从切线空间转到世界空间的矩阵TBN，把世界坐标也一并存储了
				o.TtoW0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
				o.TtoW1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
				o.TtoW2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);

				return o;

			}

			fixed4 frag(v2f i):SV_Target{

				//获取世界坐标
				float3 worldPos = float3(i.TtoW0.w, i.TtoW1.w, i.TtoW2.w);

				//获取世界空间下的光源方向和视觉方向
				fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
				fixed3 viewDir = normalize(UnityWorldSpaceViewDir(worldPos));

				//从切线空间获取法线信息，法线贴图被标记了 "Normal Map"需要先解压缩
				fixed3 tangentNormal = UnpackNormal(tex2D(_BumpMap, i.uv.zw));
				tangentNormal.xy *= _BumpScale;
				tangentNormal.z = sqrt(1.0 - saturate(dot(tangentNormal.xy, tangentNormal.xy)));

			

				// 把法线信息从切线空间转到世界空间
				fixed3 worldNormal = normalize(half3(dot(i.TtoW0.xyz, tangentNormal), dot(i.TtoW1.xyz, tangentNormal), dot(i.TtoW2.xyz, tangentNormal)));


				//计算光照
				fixed3 albedo = tex2D(_MainTex, i.uv).rgb * _Color.rgb;

				//环境光照
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * albedo;

				//用世界空间的法线和光源方向计算漫反射
				fixed3 diffuse = _LightColor0.rgb * albedo * max(0, dot(worldNormal,lightDir));

				//高光
				fixed3 halfDir = normalize(lightDir + viewDir);
				fixed3 specular = _LightColor0.rgb * _Specular.rgb * pow(max(0,dot(worldNormal,halfDir)),_Gloss);

				return fixed4(ambient + diffuse + specular,1.0);

			};


			ENDCG
		}
	}
}
