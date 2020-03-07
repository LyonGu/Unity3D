Shader "Shaders/Common/OutLine" {
	Properties {
		 [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        _OutlineColor("OutlineColor",Color) = (1,1,1,1)
        _LineWidth("LineWidth",Range(0,10)) = 0
        _CheckAccuracy("CheckAccuracy",Range(0.1,0.99)) = 0.9

	}
    SubShader {
		Tags
        {
            "Queue"="Transparent"
            "RenderType"="Opaque"
        }


		Pass {
			NAME "OUTLINE2D"

			CGPROGRAM

			    #pragma vertex vert
	            #pragma fragment frag
	            #pragma target 2.0
	            #pragma multi_compile _ PIXELSNAP_ON
	            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
	            #include "UnityCG.cginc"
	            sampler2D _MainTex;
	            float4 _MainTex_TexelSize;
	            sampler2D _AlphaTex;
	            fixed4 _Color;
	            fixed4 _OutlineColor;
	            float _CheckRange;
	            float _LineWidth;
	            float _CheckAccuracy;

			 	struct appdata_t
	            {
	                float4 vertex   : POSITION;
	                float4 color    : COLOR;
	                float2 texcoord : TEXCOORD0;
	            };

	            struct v2f
	            {
	                float4 pos   : SV_POSITION;
	                fixed4 color    : COLOR;       //顶点颜色
	                float2 uv[5]  : TEXCOORD0;
	            };


				v2f vert(appdata_t v)
	            {
	                v2f o;
	                o.pos = UnityObjectToClipPos(v.vertex);
	                float2 uv = v.texcoord;
	                o.uv[0] = uv;
	                o.color = v.color * _Color;
	                #ifdef PIXELSNAP_ON
	                	o.pos = UnityPixelSnap (o.pos);
	                #endif

	                //提前把领边UV坐标给拿到
	                o.uv[1] = uv + _MainTex_TexelSize.xy * half2(0, _LineWidth);
					o.uv[2] = uv + _MainTex_TexelSize.xy * half2(0, -_LineWidth);
					o.uv[3] = uv + _MainTex_TexelSize.xy * half2(_LineWidth, 0);
					o.uv[4] = uv + _MainTex_TexelSize.xy * half2(-_LineWidth, 0);
	                return o;
	            }


			 	fixed4 frag(v2f o) : SV_Target
	            {
	            	fixed4 tex = tex2D(_MainTex, o.uv[0]);
	            	fixed a = tex.a;
					clip(a-0.1);
	                fixed4 c = tex * o.color;

              		fixed4 pixelUp    = tex2D(_MainTex, o.uv[1]);
                    fixed4 pixelDown  = tex2D(_MainTex, o.uv[2]);
                    fixed4 pixelRight = tex2D(_MainTex, o.uv[3]);
                    fixed4 pixelLeft  = tex2D(_MainTex, o.uv[4]);

                    //step(parm1,parm2)  parm2>parm1 返回1 否则返回0
                    float bOut = step((1-_CheckAccuracy),pixelUp.a*pixelDown.a*pixelRight.a*pixelLeft.a);
                    c = lerp(_OutlineColor,c,bOut);
                    return c;
	            }

			ENDCG
		}


	}
	FallBack "Diffuse"
}
