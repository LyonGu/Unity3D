Shader "Shaders/Chapter9/ForwardRenderingTexture"
{
	//两个Pass,第一个pass为ForwardBase,第二个pass为ForwardAdd
	Properties
	{
		//漫反射贴图
		_Diffuse ("Diffuse", Color) = (1,1,1,1)
		_DiffuseMap ("Diffuse Map", 2D) = "white"{}

		//法线贴图
		_BumpMap ("Normal Map", 2D) = "white" {}
		_BumpScale ("Bump Scale", float) = 1.0

		//高光贴图
		_SpecularMap ("Specular Map", 2D) 	= "white" {}
		_SpecularScale ("Specular Scale", float) = 1.0
		_Specular ("Specular", Color)	= (1.0,1.0,1.0,1.0)
		_Gloss ("Gloss", Range(8.0, 256)) = 20
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		Pass
		{
			//平行光处理
			Tags { "LightMode"="ForwardBase" }

			CGPROGRAM

			//开启这个编译命令，是为了让Unity自动赋予shader需要的光照计算变量
			#pragma multi_compile_fwdbase

			#pragma vertex vert
			#pragma fragment frag

			#include "Lighting.cginc"

			fixed4 _Diffuse;
			sampler2D _DiffuseMap;
			float4 _DiffuseMap_ST;

			sampler2D _BumpMap;
			float4 _BumpMap_ST;
			float  _BumpScale;

			sampler2D _SpecularMap;
			float4 _SpecularMap_ST;
			fixed4 _Specular;
			float _SpecularScale;
			float  _Gloss;

			struct a2v{
				float4 vertex 	: POSITION;   //模型空间中顶点的位置
				float3 normal 	: NORMAL;     //模型空间的法线信息 
				float4 tangent 	: TANGENT;    //模型空间的切线信息 
				float4 texcoord	: TEXCOORD0;  //计算UV坐标用
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

				//实际上_DiffuseMap 和 _BumoMap使用同一组纹理坐标
				o.uv.xy = i.texcoord.xy * _DiffuseMap_ST.xy + _DiffuseMap_ST.zw;

				
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

			fixed4 frag(v2f v): SV_Target{

				fixed3 tangentLightDir = normalize(v.lightDir);
				fixed3 tangentViewDir = normalize(v.viewDir);

				//从normalMap获取像素
				fixed4 packedNormal = tex2D(_BumpMap,v.uv.xy);
				fixed3 tangentNormal;

				//法线贴图被标记了 "Normal Map"需要先解压缩
				tangentNormal = UnpackNormal(packedNormal);
				tangentNormal.xy *= _BumpScale;
				tangentNormal.z = sqrt(1.0 - saturate(dot(tangentNormal.xy, tangentNormal.xy)));

				//计算光照
				fixed3 albedo = tex2D(_DiffuseMap, v.uv).rgb * _Diffuse.rgb;

				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * albedo;

				//用切线空间的法线和光源方向计算漫反射
				fixed3 diffuse = _LightColor0.rgb * albedo * max(0, dot(tangentNormal,tangentLightDir));

				//高光  _LightColor0.rgb * _Specular.rgb可以理解为光分量中的高光分量
				fixed3 halfDir = normalize(tangentLightDir + tangentViewDir);

				//rgb都存的是同一个值
				fixed  specularMask = tex2D(_SpecularMap, v.uv).r * _SpecularScale;
				fixed3 specular = _LightColor0.rgb * _Specular.rgb * pow(max(0,dot(tangentNormal,halfDir)),_Gloss) * specularMask;


				//平行光没有衰减，所以这里设置成1.0
				fixed atten = 1.0;
				fixed3 color = ambient + (diffuse + specular) * atten;

				return fixed4(color, 1.0);
			}


			ENDCG
		}

		Pass
		{
			//其他像素光源：点光源（渲染模型设置为重要的）+平行光（最强的）
			Tags { "LightMode"="ForwardAdd" }

			//开启混合，为了跟之前的计算结果进行混合
			Blend One One
			CGPROGRAM

			//开启这个编译命令，是为了让Unity自动赋予shader需要的光照计算变量
			#pragma multi_compile_fwdadd

			#pragma vertex vert
			#pragma fragment frag

			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			fixed4 _Diffuse;
			sampler2D _DiffuseMap;
			float4 _DiffuseMap_ST;

			sampler2D _BumpMap;
			float4 _BumpMap_ST;
			float  _BumpScale;

			sampler2D _SpecularMap;
			float4 _SpecularMap_ST;
			fixed4 _Specular;
			float _SpecularScale;
			float  _Gloss;

			struct a2v{
				float4 vertex 	: POSITION;   //模型空间中顶点的位置
				float3 normal 	: NORMAL;     //模型空间的法线信息 
				float4 tangent 	: TANGENT;    //模型空间的切线信息 
				float4 texcoord	: TEXCOORD0;  //计算UV坐标用
			};

			struct v2f{
				float4 pos 		: SV_POSITION;
				float4 uv  		: TEXCOORD0;   //同时存储MainTex和normalMap的纹理坐标
				float3 lightDir : TEXCOORD1;
				float3 viewDir 	: TEXCOORD2;
				float3 worldPos : TEXCOORD3;
			};

			v2f vert(a2v i){
				v2f o;
				o.pos = UnityObjectToClipPos(i.vertex);

				//实际上_MainTex 和 _BumoMap使用同一组纹理坐标
				o.uv.xy = i.texcoord.xy * _DiffuseMap_ST.xy + _DiffuseMap_ST.zw;

				o.worldPos = mul(unity_ObjectToWorld, i.vertex).xyz;
				
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

			fixed4 frag(v2f v): SV_Target{
				fixed3 tangentLightDir = normalize(v.lightDir);
				fixed3 tangentViewDir = normalize(v.viewDir);

				//从normalMap获取像素
				fixed4 packedNormal = tex2D(_BumpMap,v.uv.xy);
				fixed3 tangentNormal;

				//法线贴图被标记了 "Normal Map"需要先解压缩
				tangentNormal = UnpackNormal(packedNormal);
				tangentNormal.xy *= _BumpScale;
				tangentNormal.z = sqrt(1.0 - saturate(dot(tangentNormal.xy, tangentNormal.xy)));

				//计算光照
				fixed3 albedo = tex2D(_DiffuseMap, v.uv).rgb * _Diffuse.rgb;


				//用切线空间的法线和光源方向计算漫反射
				fixed3 diffuse = _LightColor0.rgb * albedo * max(0, dot(tangentNormal,tangentLightDir));

				//高光  _LightColor0.rgb * _Specular.rgb可以理解为光分量中的高光分量
				fixed3 halfDir = normalize(tangentLightDir + tangentViewDir);

				//rgb都存的是同一个值
				fixed  specularMask = tex2D(_SpecularMap, v.uv).r * _SpecularScale;
				fixed3 specular = _LightColor0.rgb * _Specular.rgb * pow(max(0,dot(tangentNormal,halfDir)),_Gloss) * specularMask;


				//根据光源类型计算衰减值: 平行光没有衰减 点光源和聚光有衰减
				#ifdef USING_DIRECTIONAL_LIGHT
					fixed atten = 1.0; //平行光没有衰减，所以这里设置成1.0
				#else
					#if defined (POINT)
						//点光源
						float3 lightCoord = mul(unity_WorldToLight, float4(v.worldPos, 1)).xyz;
				        fixed atten = tex2D(_LightTexture0, dot(lightCoord, lightCoord).rr).UNITY_ATTEN_CHANNEL;
					#elif defined (SPOT)
						//聚光
						float4 lightCoord = mul(unity_WorldToLight, float4(v.worldPos, 1));
				        fixed atten = (lightCoord.z > 0) * tex2D(_LightTexture0, lightCoord.xy / lightCoord.w + 0.5).w * tex2D(_LightTextureB0, dot(lightCoord, lightCoord).rr).UNITY_ATTEN_CHANNEL;
					#else
						//其他光源
					   fixed atten = 1.0;
					#endif
				#endif

				//只计算漫反射和高光，不计算环境光
				fixed3 color = (diffuse + specular) * atten;

				return fixed4(color, 1.0);
			}

			ENDCG
		}
	}
	FallBack "Specular"
}
