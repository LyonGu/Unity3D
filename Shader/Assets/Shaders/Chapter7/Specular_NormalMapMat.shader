Shader "Shaders/Chapter7/Specular_NormalMapMat"
{
	Properties
	{
		_Color			("Color Tint", Color) 		= (1.0,1.0,1.0,1.0)
		_MainTex 		("Main Tex", 2D) 			= "white" {}
		_BumpMap 		("Normal Map", 2D) 			= "white" {}
		_BumpScale 		("Bump Scale", float) 		= 1.0
		_SpecularMap 	("Specular Map", 2D) 			= "white" {}
		_SpecularScale 	("Specular Scale", float) 		= 1.0
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

			v2f vert(a2v v){
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);

				//实际上_MainTex 和 _BumoMap使用同一组纹理坐标
				o.uv.xy = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;

				//从模型空间转到切线空间 有问题 ，未考虑非线性缩放，参考NormalMapTangentSpaceMat代码
				TANGENT_SPACE_ROTATION;
				o.lightDir = mul(rotation, ObjSpaceLightDir(v.vertex)).xyz;
				o.viewDir = mul(rotation, ObjSpaceViewDir(v.vertex)).xyz;

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
