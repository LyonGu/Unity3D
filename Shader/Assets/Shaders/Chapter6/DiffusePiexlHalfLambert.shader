Shader "Shaders/Chapter6/DiffusePiexlHalfLambert"
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
				float3 worldNormal : TEXCOORD0;
			};


			v2f vert(a2v v){
				v2f o;
				//坐标转到裁剪空间
				o.pos = UnityObjectToClipPos(v.vertex);

				//把法线从模型空间转到世界空间 使用逆转置矩阵
				fixed3 worldNormal = normalize(mul(v.normal, (float3x3)unity_WorldToObject));

				//传递给片段着色器
				o.worldNormal = worldNormal;
				return o;
			}

			//逐像素光照
			fixed4 frag(v2f i):SV_Target{

				//获取环境光分量
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;

				fixed3 worldNormal = normalize(i.worldNormal);

				//光的方向
				fixed3 worldLight = normalize(_WorldSpaceLightPos0.xyz);

				//半兰伯特光照模型 不用max操作防止为负值
				fixed halfLambert = dot(worldNormal, worldLight) * 0.5 + 0.5;

				//_Diffuse理解为材质的颜色
				fixed3 diffuse = _LightColor0.rgb * _Diffuse.rgb * halfLambert;

				fixed3 color = ambient + diffuse;
				return fixed4(color, 1.0);
			}

			ENDCG
		}
	}
	FallBack "Diffuse"
}
