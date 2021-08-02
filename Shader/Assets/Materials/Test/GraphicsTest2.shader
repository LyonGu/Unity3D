Shader "Common/GraphicTest2" {

    //参照UIDefault改写
	Properties {
       [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _ShowTex 		("Show Tex", 2D) 	= "white" {}
        _UVX("uv x", Float) = 0.5
        _UVY("uv y", Float) = 0.5
        _UVRange("uv Range",Vector)= (0,0,0,0)  // xmin,xmax,ymin,ymax
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _MaskTex("Mask Tex", 2D) 	= "white" {}

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }
    SubShader
    {
    	Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]


        Pass {
            CGPROGRAM

            #pragma vertex  vert
            #pragma fragment frag

            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _Color;
            fixed _UVX;
            fixed _UVY;
            sampler2D _ShowTex;
            sampler2D _MaskTex;
            vector _UVRange;


            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = v.texcoord;

                OUT.color = v.color * _Color;
                return OUT;
            }

            sampler2D _MainTex;

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color;
                half4 Maskcolor = tex2D(_MaskTex, IN.texcoord);
                if (Maskcolor.r == 0)
                {
                    color = tex2D(_ShowTex, IN.texcoord) * IN.color;
                }
                else
                {
                    color = tex2D(_MainTex, IN.texcoord) * IN.color;
                }
                // if(IN.texcoord.x >= _UVRange.x && IN.texcoord.x < _UVRange.y && IN.texcoord.y >= _UVRange.z && IN.texcoord.y < _UVRange.w)
                // {
                //     float ux = (IN.texcoord.x - _UVRange.x) * 1/(_UVRange.y - _UVRange.x);
                //     float uy = (IN.texcoord.y - _UVRange.z) * 1/(_UVRange.w - _UVRange.z);
                //     color = tex2D(_ShowTex, float2(ux, uy)) * IN.color;
                // }
                // else
                // {
                //     color = tex2D(_MainTex, IN.texcoord) * IN.color;
                // }
                return color;
            }
            ENDCG
        }

    }

   FallBack "VertexLit"
}
