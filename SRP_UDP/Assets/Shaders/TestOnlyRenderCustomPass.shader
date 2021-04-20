Shader "Unlit/TestOnlyRenderCustomPass"
{
    Properties
    {

        // _MainTex ("Texture", 2D) = "white" {}
        // _Color("Main Color", Color) = (1,1,1,1)

        //URP 不再推荐写_MainTex 和 _Color这样的属性名，内置的URP都改成 _BaseMap 和 _BaseColor
        _BaseMap ("Texture", 2D) = "white" {}
        _BaseColor("Main Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
			Tags {"LightMode" = "TestOnlyRenderCustomPass"}
            //URP shader 使用 HLSLPROGRAM/ENDHLSL 代替 CGPROGRAM/ENDCG
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            //内置管线用的是cginc，URP用的是hlsl
            //#include "UnityCG.cginc"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            // #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"


            //结构体大致一样，只是名字换了下，也可以不换
            // struct appdata
            // {
            //     float4 vertex : POSITION;
            //     float2 uv : TEXCOORD0;
            // };

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            // struct v2f
            // {
            //     float2 uv : TEXCOORD0;
            //     UNITY_FOG_COORDS(1)
            //     float4 vertex : SV_POSITION;
            // };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionHCS : SV_POSITION;
            };


            // 内置管线声明贴图 sampler2D
            // sampler2D _BaseMap;

            // URP声明贴图
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            // 内置管线声明其他属性
            // float4 _BaseMap_ST;
            // fixed4 _BaseColor;

            //URP需要将属性包裹在CBUFFER中，这样才能进行SPRBatcher
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor; //无法使用fixed4
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings o;

                //UnityObjectToClipPos 变成 TransformObjectToHClip
                o.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                o.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                // tex2D 变成 SAMPLE_TEXTURE2D
                // fixed4 col = tex2D(_BaseMap, i.uv) * _BaseColor;
                half4 col = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _BaseColor;

                return col;
            }
            ENDHLSL
        }
    }
}
