Shader "Common/GrayUI" {
	Properties {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _GrayOff("Gray Off", float) = 0.0
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
            fixed4 _Color;
            float _fillCount;
            float _GrayOff;

            v2f vert(a2v v){
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }


            fixed4 frag(v2f o) : SV_Target
            {
                fixed4 c =  tex2D (_MainTex, o.uv) * _Color;
                if (_GrayOff <= 0)
                {
                    float average = 0.2126 * c.r + 0.7152 * c.g + 0.0722 * c.b;
                    c.rgb = fixed3(average, average, average);
                }
                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }

    }

   FallBack "VertexLit"
}
