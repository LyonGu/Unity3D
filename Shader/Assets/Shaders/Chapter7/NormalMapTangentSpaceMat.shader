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
			sampler2D   _BumpMap;
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
				float3 lightDir : TEXCOORD1;
				float3 viewDir 	: TEXCOORD2;
			};

			float4x4 inverse(float4x4 input) {
				#define minor(a,b,c) determinant(float3x3(input.a, input.b, input.c))
				
				float4x4 cofactors = float4x4(
				     minor(_22_23_24, _32_33_34, _42_43_44), 
				    -minor(_21_23_24, _31_33_34, _41_43_44),
				     minor(_21_22_24, _31_32_34, _41_42_44),
				    -minor(_21_22_23, _31_32_33, _41_42_43),
				    
				    -minor(_12_13_14, _32_33_34, _42_43_44),
				     minor(_11_13_14, _31_33_34, _41_43_44),
				    -minor(_11_12_14, _31_32_34, _41_42_44),
				     minor(_11_12_13, _31_32_33, _41_42_43),
				    
				     minor(_12_13_14, _22_23_24, _42_43_44),
				    -minor(_11_13_14, _21_23_24, _41_43_44),
				     minor(_11_12_14, _21_22_24, _41_42_44),
				    -minor(_11_12_13, _21_22_23, _41_42_43),
				    
				    -minor(_12_13_14, _22_23_24, _32_33_34),
				     minor(_11_13_14, _21_23_24, _31_33_34),
				    -minor(_11_12_14, _21_22_24, _31_32_34),
				     minor(_11_12_13, _21_22_23, _31_32_33)
				);
				#undef minor
				return transpose(cofactors) / determinant(input);
			}


			v2f vert(a2v v){
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);

				//实际上_MainTex 和 _BumoMap使用同一组纹理坐标
				o.uv.xy = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				o.uv.zw = v.texcoord.xy * _BumpMap_ST.xy + _BumpMap_ST.zw;

				
				//我们要得出从世界空间到切线空间的转换矩阵TBN，然后把世界空间下的光方向和视觉方向
				//都转到切线空间，所以我们可以先求出TBN的逆矩阵，即从切线空间到世界空间的转换矩阵TBN_I

				///
				/// Note that the code below can handle both uniform and non-uniform scales
				///

				//考虑了非线性缩放和线性缩放

				//构建一个从切线空间到世界空间的矩阵，因为用的都是世界空间的信息，所以只能转到世界空间
				fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);  
				fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);  
				//计算副切线
				fixed3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w; 

				/*
				float4x4 tangentToWorld = float4x4(worldTangent.x, worldBinormal.x, worldNormal.x, 0.0,
												   worldTangent.y, worldBinormal.y, worldNormal.y, 0.0,
												   worldTangent.z, worldBinormal.z, worldNormal.z, 0.0,
												   0.0, 0.0, 0.0, 1.0);
				// The matrix that transforms from world space to tangent space is inverse of tangentToWorld

				//取反操作，得到TBN
				float3x3 worldToTangent = inverse(tangentToWorld);
				*/
				
				//wToT = the inverse of tToW = the transpose of tToW as long as tToW is an orthogonal matrix.
				float3x3 worldToTangent = float3x3(worldTangent, worldBinormal, worldNormal);

				// Transform the light and view dir from world space to tangent space
				o.lightDir = mul(worldToTangent, WorldSpaceLightDir(v.vertex));
				o.viewDir = mul(worldToTangent, WorldSpaceViewDir(v.vertex));

				///
				/// Note that the code below can only handle uniform scales, not including non-uniform scales
				/// 
				// 下面的代码未考虑非线性缩放
				// Compute the binormal
//				float3 binormal = cross( normalize(v.normal), normalize(v.tangent.xyz) ) * v.tangent.w;
//				// Construct a matrix which transform vectors from object space to tangent space
//				float3x3 rotation = float3x3(v.tangent.xyz, binormal, v.normal);
				// Or just use the built-in macro
//				TANGENT_SPACE_ROTATION;
//				
//				// Transform the light direction from object space to tangent space
//				o.lightDir = mul(rotation, normalize(ObjSpaceLightDir(v.vertex))).xyz;
//				// Transform the view direction from object space to tangent space
//				o.viewDir = mul(rotation, normalize(ObjSpaceViewDir(v.vertex))).xyz;
				
				return o;

			}

			fixed4 frag(v2f i):SV_Target{

				fixed3 tangentLightDir = normalize(i.lightDir);
				fixed3 tangentViewDir =  normalize(i.viewDir);

				//从normalMap获取像素
				fixed4 packedNormal = tex2D(_BumpMap,i.uv.zw);
				fixed3 tangentNormal;

				//法线贴图被标记了 "Normal Map"需要先解压缩
				tangentNormal = UnpackNormal(packedNormal);
				tangentNormal.xy *= _BumpScale;
				tangentNormal.z = sqrt(1.0 - saturate(dot(tangentNormal.xy, tangentNormal.xy)));

				//计算光照
				fixed3 albedo = tex2D(_MainTex, i.uv).rgb * _Color.rgb;

				//环境光照
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * albedo;

				//用切线空间的法线和光源方向计算漫反射
				fixed3 diffuse = _LightColor0.rgb * albedo * max(0, dot(tangentNormal,tangentLightDir));

				//高光
				fixed3 halfDir = normalize(tangentLightDir + tangentViewDir);
				fixed3 specular = _LightColor0.rgb * _Specular.rgb * pow(max(0,dot(tangentNormal,halfDir)),_Gloss);

				return fixed4(ambient + diffuse + specular,1.0);

			};


			ENDCG
		}
	}
}
