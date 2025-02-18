Shader "Shaders/Chapter6/SpecularVertexLevel"
{
	//逐顶点高光会导致高光部分不平滑，不推荐使用

	//高光部分的计算是非线性的
	//顶点着色器中计算光照在进行插值的过程是线性的，破坏了原有的非线性关系

	Properties
	{
		_Diffuse 	("Diffuse", Color) 			= (1.0,1.0,1.0,1.0)
		_Specular 	("Specular", Color) 		= (1.0,1.0,1.0,1.0)
		_Gloss 		("Gloss", Range(8.0,256)) 	= 20
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

			//属性定义需要分号了
			fixed4 _Diffuse;
			fixed4 _Specular;
			float  _Gloss;

			struct a2v{
				float4 vertex: POSITION;
				float3 normal: NORMAL;
			};

			struct v2f{
				float4 pos: SV_POSITION;
				fixed3 color: COLOR;
			};

			//逐顶点高光
			v2f vert(a2v i){
				v2f o;
				o.pos = UnityObjectToClipPos(i.vertex);

				//环境光照
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;

				//法线方向转到世界空间
				fixed3 worldNormal = normalize(mul(i.normal, (float3x3)unity_WorldToObject));

				//光的方向
				fixed3 worldLightDir = normalize(_WorldSpaceLightPos0.xyz);

				//计算漫反射
				fixed3 diffuse = _LightColor0.rgb * _Diffuse.rgb * saturate(dot(worldNormal, worldLightDir));

				//获取反射方向向量
				fixed3 reflectDir = normalize(reflect(-worldLightDir,worldNormal));

				//获取视觉方向: 摄像机的位置-世界空间的顶点位置
				float3 worldPos = mul(unity_ObjectToWorld, i.vertex).xyz;
				fixed3 viewDir = normalize(_WorldSpaceCameraPos.xyz-worldPos);

				//计算高光
				fixed3 specular = _LightColor0.rgb * _Specular.rgb * pow(saturate(dot(reflectDir, viewDir)),_Gloss);


				//最终光照结果
				o.color = ambient + diffuse + specular;
				return o;

			}

			fixed4 frag(v2f o): SV_Target
			{
				return fixed4(o.color,1.0);
			}

			ENDCG
		}
	}
	FallBack "Specular"
}
