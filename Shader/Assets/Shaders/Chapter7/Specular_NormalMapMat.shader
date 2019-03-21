Shader "Shaders/Chapter7/Specular_NormalMapMat"
{
	Properties
	{
		_Color			("Color Tint", Color) 		= (1.0,1.0,1.0,1.0)
		_MainTex 		("Main Tex", 2D) 			= "white" {}
		_BumpMap 		("Normal Map", 2D) 			= "white" {}
		_BumpScale 		("Bump Scale", float) 		= 1.0
		_SpecularMap 	("Normal Map", 2D) 			= "white" {}
		_SpecularScale 	("Bump Scale", float) 		= 1.0
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
			sampler2D   _SpecularMap;
			float4      _SpecularMap_ST;
			float 		_BumpScale;
			float 		_SpecularScale;
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

			v2f vert(a2v i){
				v2f o;
				o.pos = UnityObjectToClipPos(i.vertex);

				//实际上_MainTex 和 _BumoMap使用同一组纹理坐标
				o.uv.xy = i.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;

				
				//法线贴图中的法线向量在切线空间中，法线永远指着正z方向
				//直接使用TBN矩阵：这个矩阵可以把切线坐标空间的向量转换到世界坐标空间
				//TBN矩阵的逆矩阵：把世界坐标空间的向量转换到切线坐标空间

				//计算副切线
				float3 binormal = cross(normalize(i.normal),normalize(i.tangent.xyz)) * i.tangent.w;

				//构建TBN矩阵：从模型空间转到切线空间
				//i.normal信息是模型空间的法线信息，所以只能够能从模型空间转向切线空间的矩阵

				float3x3 rotation = float3x3(i.tangent.xyz, binormal, i.normal);
				
				//把光的方向转到切线空间
				o.lightDir = mul(rotation, ObjSpaceLightDir(i.vertex)).xyz;

				//把视觉方向转到切线空间
				o.viewDir = mul(rotation, ObjSpaceViewDir(i.vertex)).xyz;

				return o;
			}

			fixed4 frag(v2f i):SV_Target{

				fixed3 tangentLightDir = normalize(i.lightDir);
				fixed3 tangentViewDir =  normalize(i.viewDir);

				//从normalMap获取像素
				fixed4 packedNormal = tex2D(_BumpMap,i.uv.xy);
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

				//高光  _LightColor0.rgb * _Specular.rgb可以理解为光分量中的高光分量
				fixed3 halfDir = normalize(tangentLightDir + tangentViewDir);

				//rgb都存的是同一个值
				fixed  specularMask = tex2D(_SpecularMap, i.uv).r * _SpecularScale;
				fixed3 specular = _LightColor0.rgb * _Specular.rgb * pow(max(0,dot(tangentNormal,halfDir)),_Gloss) * specularMask;

				return fixed4(ambient + diffuse + specular,1.0);

			};


			ENDCG
		}
	}
	FallBack "Specular"
}
