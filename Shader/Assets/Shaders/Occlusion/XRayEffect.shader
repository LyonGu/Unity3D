
Shader "Occlusion/XRayEffect"
{
	//所谓X光，就是在被遮挡的部分呈现一个其他的颜色
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_XRayColor("XRay Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags{ "Queue" = "Geometry" "RenderType" = "Opaque" }
		LOD 100

		//渲染X光效果的Pass
		Pass
		{
			//ZTest 可取值为：Greater , GEqual , Less , LEqual , Equal , NotEqual , Always , Never , Off，默认是 LEqual，ZTest Off 等同于 ZTest Always。

			Blend SrcAlpha One
			ZWrite Off    //一定要关闭深度写入，否则正常渲染的pass会把遮挡部分也画出来  ZTest默认值为LEqual
			ZTest Greater //被遮挡的部分设置深度大于通过

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "Lighting.cginc"
			fixed4 _XRayColor;
			struct v2f
			{	
				float4 pos : SV_POSITION;
				fixed3 normal : NORMAL;
				fixed3 viewDir : TEXCOORD0;

			};
 
			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.normal = v.normal;
				o.viewDir = ObjSpaceViewDir(v.vertex);
				return o;
			}
 
			fixed4 frag(v2f i) : SV_Target
			{
				float3 normal = normalize(i.normal);
				float3 viewDir = normalize(i.viewDir);
				float rim = 1 - dot(normal, viewDir);
				return _XRayColor * rim;

			}

			ENDCG
		}

		//正常渲染的Pass
		Pass
		{
			
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "Lighting.cginc"

			sampler2D _MainTex;
			fixed4 _MainTex_ST;

			struct v2f
			{
				float4 pos : SV_POSITION;
				fixed2 uv : TEXCOORD1;
			};
 
			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}
 
			fixed4 frag(v2f i) : SV_Target
			{
				return tex2D(_MainTex, i.uv);
			}

			ENDCG
		}
	}
}
