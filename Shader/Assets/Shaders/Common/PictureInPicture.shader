Shader "Common/PictureInPicture" {

    
	Properties {
		_MainTex("Sprite Texture", 2D) = "white" {}
		_MaskTex("Mask Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
    	Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        
        Cull Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass {
            CGPROGRAM
            
            #pragma vertex  vert
            #pragma fragment frag

            #pragma target 2.0
            
            #include "UnityCG.cginc"

            struct a2v{
                float4 vertex: POSITION;
                float2 texcoord : TEXCOORD0;

            };

            struct v2f
            {
                float4 pos   : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
			sampler2D _MaskTex;
            fixed4 _Color;

            v2f vert(a2v v){
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }


			fixed4 frag(v2f o) : SV_Target
			{
                fixed4 c =  tex2D (_MainTex, o.uv) * _Color * tex2D(_MaskTex, o.uv);
                c.rgb *= c.a;
                return c;
            }
            ENDCG     
        }
       
    }
    
   FallBack "VertexLit"
}
