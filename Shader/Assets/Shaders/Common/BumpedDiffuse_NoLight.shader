Shader "Shaders/Common/BumpedDiffuse_NoLight" {
	//只有单纯的漫反射贴图
	Properties {
		_Color ("Color Tint", Color) = (1, 1, 1, 1)
		_MainTex ("Main Tex", 2D) = "white" {}
	}
	SubShader 
	{
		Tags { "RenderType"="Opaque" "Queue"="Geometry"}

		Pass { 

			CGPROGRAM
			
				#pragma vertex vert
				#pragma fragment frag
				
				fixed4 _Color;
				sampler2D _MainTex;
				float4 _MainTex_ST;
			
				
				struct a2v {
					float4 vertex : POSITION;
					float4 texcoord : TEXCOORD0;
				};
				
				struct v2f {
					float4 pos : SV_POSITION;
					float4 uv : TEXCOORD0;
				};
				
				v2f vert(a2v v) {
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					o.uv.xy = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
					return o;
				}
				
				fixed4 frag(v2f i) : SV_Target {
					fixed3 albedo = tex2D(_MainTex, i.uv.xy).rgb * _Color.rgb;
					return fixed4(albedo, 1.0);
				}
			
			ENDCG
		}
		
		
	}
} 

