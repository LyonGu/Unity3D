Shader "Shaders/Chapter7/SingleTexture"
{
	//Blinn-Phong模型：用半法量方向来代替反射方向计算高光 （phong光照模型的进阶版）
	Properties
	{
		_Color 		("Color Tint", Color) 		= (1.0,1.0,1.0,1.0)
		_MainTex 	("Main Tex", 2D) 			= "white" {}
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
			fixed4 _Color;
			sampler2D _MainTex;
			fixed4 _Specular;
			float  _Gloss;

			//并不是只能声明自己定义的，还可以添加@@@@@@@@@
			//_MainTex_ST.xy 存储的是缩放值 _MainTex_ST.zw是偏移值
			float4 _MainTex_ST; //纹理名_ST表明纹理的属性

			struct a2v{
				float4 vertex: POSITION;
				float3 normal: NORMAL;
				float4 texcoord : TEXCOORD0;
			};

			struct v2f{
				float4 pos: SV_POSITION;
				fixed3 worldNormal: TEXCOORD0;
				float3 worldPos : TEXCOORD1; //纹理有4个通道可以用来存储数据
				float2 uv : TEXCOORD2;  	//纹理坐标
			};

			
			v2f vert(a2v i){
				v2f o;
				o.pos = UnityObjectToClipPos(i.vertex);

				//法线方向转到世界空间
				//fixed3 worldNormal = normalize(mul((float3x3)unity_ObjectToWorld,i.normal));

				//Unity内置函数
				fixed3 worldNormal = UnityObjectToWorldNormal(i.normal);

				o.worldNormal = worldNormal;
				o.worldPos = mul(unity_ObjectToWorld, i.vertex);

				o.uv = i.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;

				//调用内置函数
				//o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);

				return o;

			}

			//逐像素高光
			fixed4 frag(v2f o): SV_Target 
			{
				fixed3 color;

				fixed3 worldNormal = normalize(o.worldNormal);

				//光的方向
				fixed3 worldLightDir = normalize(UnityWorldSpaceLightDir(o.worldPos));

				// 采样颜色
				fixed3 albedo = tex2D(_MainTex, o.uv).rgb * _Color.rgb;

				//环境光照
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * albedo;

				//计算漫反射
				fixed3 diffuse = _LightColor0.rgb * albedo * saturate(dot(worldNormal, worldLightDir));

				//获取反射方向向量
				//fixed3 reflectDir = normalize(reflect(-worldLightDir,worldNormal));

				//获取视觉方向: 摄像机的位置-世界空间的顶点位置
				float3 worldPos = o.worldPos;

				fixed3 viewDir = normalize(UnityWorldSpaceViewDir(worldPos));

				//计算半法线方向
				fixed3 halfDir = normalize(worldLightDir + viewDir);

				//计算高光:用dot(半法线，法线)
				fixed3 specular = _LightColor0.rgb * _Specular.rgb * pow(saturate(dot(halfDir, worldNormal)),_Gloss);


				//最终光照结果
				color = ambient + diffuse + specular;

				return fixed4(color,1.0);
			}
	
			ENDCG
		}
	}
	FallBack "Specular"
}
