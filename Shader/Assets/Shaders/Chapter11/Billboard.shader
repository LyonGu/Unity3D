Shader "Shaders/Chapter11/Billboard"
{
	Properties
	{
		_MainTex ("Main Tex", 2D) = "white" {}
		_Color ("Color Tint", Color) = (1, 1, 1, 1)

		//固定法线还是固定向上方向： 1固定法线 0固定向上方向
		_VerticalBillboarding ("Vertical Restraints", Range(0, 1)) = 1 
	}
	SubShader
	{
		//设置透明效果， DisableBatching为关闭批处理
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "DisableBatching"="True"}

		Pass
		{

			Tags { "LightMode"="ForwardBase" }
			ZWrite Off   //关闭深度写入
			Blend SrcAlpha OneMinusSrcAlpha  //开启混合 设置混合模式
			Cull Off  //关闭剔除背面，让两面都渲染

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag


			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;
			fixed _VerticalBillboarding;
			
			struct a2v{
				float4 vertex:POSITION;
				float4 texcoord : TEXCOORD0;
			};

			struct v2f{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			//所有的计算都在模型空间里
			v2f vert(a2v i)
			{
				v2f o;

				//选择模型空间的原点作为广告牌的锚点
				float3 center = float3(0, 0, 0);

				//获取模型空间下摄像机的位置
				float3 viewer = mul(unity_WorldToObject,float4(_WorldSpaceCameraPos, 1));

				//固定法线为视角方向
				float3 normalDir = viewer - center;

				normalDir.y =normalDir.y * _VerticalBillboarding;
				normalDir = normalize(normalDir);

				//构建3个正交基，跟opengl里摄像机view矩阵一样
				float3 upDir = abs(normalDir.y) > 0.999 ? float3(0, 0, 1) : float3(0, 1, 0);
				float3 rightDir = normalize(cross(upDir, normalDir));
				upDir = normalize(cross(normalDir, rightDir));

				// Use the three vectors to rotate the quad
				float3 centerOffs = i.vertex.xyz - center;
				float3 localPos = center + rightDir * centerOffs.x + upDir * centerOffs.y + normalDir * centerOffs.z;
              
				o.pos = UnityObjectToClipPos(float4(localPos, 1));
				o.uv = TRANSFORM_TEX(i.texcoord,_MainTex);

				return o;

			}

			fixed4 frag (v2f i) : SV_Target {
				fixed4 c = tex2D (_MainTex, i.uv);
				c.rgb *= _Color.rgb;
				
				return c;
			}
			
			ENDCG
		}
	}
}
