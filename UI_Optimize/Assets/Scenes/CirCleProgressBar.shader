Shader "UI/CirCleProgressBar"
{
    Properties
    {
        [hideinInspector]
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        _Angle("Angle", range(0,361)) = 360
        _Center("Center", vector) = (.5,.5,0,0)
        _Width("Width", float) = 1
        [hideinInspector]
        _UVRect ("UvRect", Vector) = (0, 0, 1, 1)

        [hideinInspector]
        _UVScale("UVScale", Vector) = (0, 0, 0, 0)
    }

        SubShader
            {
                Tags
                {
                    "Queue" = "Transparent"
                    "IgnoreProjector" = "True"
                    "RenderType" = "Transparent"
                    "PreviewType" = "Plane"
                    "CanUseSpriteAtlas" = "True"
                }


                Cull Off
                Lighting Off
                ZWrite Off
                ZTest[unity_GUIZTestMode]
                Blend SrcAlpha OneMinusSrcAlpha

                Pass
                {
                    CGPROGRAM
                        #pragma vertex vert
                        #pragma fragment frag
                        #include "UnityCG.cginc"

                        float _Angle;
                        float4 _Center;
                        half _Width;

                        struct appdata_t
                        {
                            float4 vertex   : POSITION;
                            float4 color    : COLOR;
                            float2 texcoord : TEXCOORD0;
                            float2 texcoord1 : TEXCOORD1;
                        };

                        struct v2f
                        {
                            float4 vertex   : SV_POSITION;
                            fixed4 color : COLOR;
                            half2 texcoord  : TEXCOORD0;
                            float2 texcoord1 : TEXCOORD1;
                        };

                        fixed4 _Color;
                        sampler2D _MainTex;
                        float4 _MainTex_ST;
                        float4 _UVRect;
                        float4 _UVScale;

                        v2f vert(appdata_t IN)
                        {
                            v2f OUT;
                            OUT.vertex = UnityObjectToClipPos(IN.vertex);

                            //非运行时，_MainTex 是sprite对应的texture, texcoord对应的就是原始sprite的uv坐标
                            //运行时，MainTex 是sprite对应图集的texture,texcoord对应的就是图集的uv坐标
                            float2 texcoord = TRANSFORM_TEX(IN.texcoord,_MainTex);
                            OUT.texcoord = texcoord;
                            float2 tc = IN.texcoord1;
                            OUT.texcoord1 = IN.texcoord1;
                            OUT.color = IN.color * _Color;
                            return OUT;
                        }


                        fixed4 frag(v2f IN) : SV_Target
                        {
                            // float2 tc = IN.texcoord1;
                            // tc.x = _UvRect.x + (_UvRect.z - _UvRect.x) * tc.x;
				            // tc.y = _UvRect.y + (_UvRect.w - _UvRect.y) * tc.y;

                            // float2 texcoord = IN.texcoord;
                            // texcoord.x =  (_UVRect.z - _UVRect.x)*texcoord.x + _UVRect.x;
                            // texcoord.y =  (_UVRect.w - _UVRect.y)*texcoord.y + _UVRect.y;
                            // _Center.x = _Center.x + (IN.texcoord.x - texcoord.x);
                            // _Center.y = _Center.y + (IN.texcoord.y - texcoord.y);

                            //IN.texcoord1 = tc;

                            // float2 center = (_UVRect.zw + _UVRect.xy) / 2;
                            // float2 texcoord = IN.texcoord;
                            // texcoord = texcoord - _UVRect.xy - center;
                            // texcoord *= _UVScale;
                            // texcoord += center;
                            // texcoord = texcoord / (_UVRect.zw - _UVRect.xy);

                            // float2 texcoord = IN.texcoord;
                            // texcoord.x =  (_UVRect.z - _UVRect.x)*texcoord.x + _UVRect.x;
                            // texcoord.y =  (_UVRect.w - _UVRect.y)*texcoord.y + _UVRect.y;

                            // float2 tc = IN.texcoord1;
			                // tc.x = _UVRect.x + (_UVRect.z - _UVRect.x) * tc.x;
				            // tc.y = _UVRect.y + (_UVRect.w - _UVRect.y) * tc.y;

                            half4 color = tex2D(_MainTex, IN.texcoord)* IN.color;

                            // float uvCenterX = (_UVRect.x + _UVRect.z) * 0.5f;
                            // float uvCenterY = (_UVRect.y + _UVRect.w) * 0.5f;
                            float2 pos =  IN.texcoord1 -_Center;
                            // float2 pos =  texcoord-_Center;

                            float ang = degrees(atan2(pos.x, -pos.y)) + 180;

                            _Angle = 360 - _Angle;
                            color.a = color.a * saturate((ang - _Angle) / _Width);
                            return color;
                        }
                    ENDCG
                 }
            }
}
