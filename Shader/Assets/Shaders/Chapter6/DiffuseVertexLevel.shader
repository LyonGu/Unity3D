Shader "Shaders/Chapter6/DiffuseVertexLevel"
{
	Properties
	{
		_Diffuse ("Diffuse", Color)	 = (1.0,1.0,1.0,1.0) 
	}
	SubShader
	{
		Pass
		{
			//指定该Pass的光照模式
			Tags {"LightMode" = "ForwardBase"}
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "Lighting.cginc"

			fixed4 _Diffuse;

			struct a2v{
				float4 vertex : POSITION; //模型空间的点给vertex
				float3 normal : NORMAL;   //模型空间的法线给normal
			};

			struct v2f{
				float4 pos : SV_POSITION; //pos包含裁剪空间的顶点坐标
				fixed3 color : COLOR;   //颜色存储
			};

			//逐顶点光照
			v2f vert(a2v v){
				v2f o;
				//坐标转到裁剪空间
				o.pos = UnityObjectToClipPos(v.vertex);

				//获取环境光分量
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;

				//把法线从模型空间转到世界空间
				//fixed3 worldNormal = normalize(mul(v.normal, (float3x3)unity_WorldToObject));
				fixed3 worldNormal = normalize(mul((float3x3)unity_ObjectToWorld,v.normal));

				//光源的方向（假设场景中只有一个光源切实平行光）
				fixed3 worldLight = normalize(_WorldSpaceLightPos0.xyz);

				//计算漫反射 saturate类似于 math.max ==>避免计算值为负值
				//dot(法线方向，光源方向) 
				//_Diffuse理解为材质的颜色
				fixed3 diffuse = _LightColor0.rgb * _Diffuse.rgb * saturate(dot(worldNormal, worldLight));

				o.color = ambient + diffuse;
				return o;
			}

			fixed4 frag(v2f i):SV_Target{
				return fixed4(i.color, 1.0);
			}
			
			ENDCG
		}
	}
	FallBack "Diffuse"
}
