Shader "Shaders/Common/OutLine_origin" {
	Properties {
		 [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        _OutlineColor("OutlineColor",Color) = (1,1,1,1)
        _CheckRange("CheckRange",Range(0,10)) = 0
        _LineWidth("LineWidth",Float) = 0.39
        _CheckAccuracy("CheckAccuracy",Range(0.1,0.99)) = 0.9

	}
    SubShader {
		Tags
        { 
            "Queue"="Transparent" 
            "RenderType"="Opaque" 
        }
		

		Pass {
			NAME "OUTLINE"
			
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
	                float2 texcoord  : TEXCOORD0;
	            };

			
				v2f vert(appdata_t v)
	            {
	                v2f o;
	                o.pos = UnityObjectToClipPos(v.vertex);
	                o.texcoord = v.texcoord;
	                o.color = v.color * _Color;
	                #ifdef PIXELSNAP_ON
	                	o.pos = UnityPixelSnap (o.pos);
	                #endif
	                return o;
	            }
			

			 	fixed4 frag(v2f o) : SV_Target
	            {
	            	fixed4 tex = tex2D(_MainTex, o.texcoord);
	            	fixed a = tex.a;
					clip(a-0.1);
	                fixed4 c = tex * o.color;
	                
	                float isOut = step(abs(1/_LineWidth),c.a);
	                if(isOut != 0)
	                {
	                    fixed4 pixelUp = tex2D(_MainTex, o.texcoord + fixed2(0, _MainTex_TexelSize.y*_CheckRange));  
	                    fixed4 pixelDown = tex2D(_MainTex, o.texcoord - fixed2(0, _MainTex_TexelSize.y*_CheckRange));  
	                    fixed4 pixelRight = tex2D(_MainTex, o.texcoord + fixed2(_MainTex_TexelSize.x*_CheckRange, 0));  
	                    fixed4 pixelLeft = tex2D(_MainTex, o.texcoord - fixed2(_MainTex_TexelSize.x*_CheckRange, 0)); 

	                    //step(parm1,parm2)  parm2>parm1 返回1 否则返回0
	                    float bOut = step((1-_CheckAccuracy),pixelUp.a*pixelDown.a*pixelRight.a*pixelLeft.a);
	                    c = lerp(_OutlineColor,c,bOut);
	                    return c;
	                }
	                return c; 
	            }
			
			ENDCG
		}
		
	
	}
	FallBack "Diffuse"
}
