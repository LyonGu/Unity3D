Shader "Shaders/Common/RimLight" {

    //边缘发光效果  https://blog.csdn.net/puppet_master/article/details/53548134
    /*
    所谓RimLight边缘发光，也就是说对应我们当前视角方向，物体上位于边缘的地方额外加一个光的效果。那么，怎么判断一个点是否在物体的边缘呢？就是通过法线方向和视线方向的夹角来判断。当视线方向V与法线方向N垂直时，这个法线对应的面就与视线方向平行，说明当前这个点对于当前视角来说，就处在边缘；而视线方向与法线方向一致时，这个法线对应的面就垂直于视线方向，说明当前是直视这个面。所以，我们就可以根据dot（N,V）来获得视线方向与法线方向的余弦值，通过这个值来区分该像素是否处在边缘，进而判断是否需要增加以及增加边缘光的强弱。

    */

    //
    Properties {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        //边缘发光颜色
        _RimColor("RimColor", Color) = (1,1,1,1)

        //边缘发光强度
        _RimPower("RimPower", Range(0.000001, 3.0)) = 0.1

        //mask图，控制自发光部分
        _RimMask("RimMask", 2D) = "white"{}

        _SpecularMap ("Specular Texture", 2D) = "white" {}
        _Specular ("Specular Color", Color) = (1, 1, 1, 1)
        _Gloss ("Gloss", Range(8.0, 256)) = 20

    }
    SubShader
    {
        
        Tags { "RenderType"="Opaque" "Queue"="Geometry"}
        
        Pass {
            Tags { "LightMode"="ForwardBase" }

            CGPROGRAM
            
            #pragma vertex  vert
            #pragma fragment frag

            #pragma target 2.0
            
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct a2v{
                float4 vertex: POSITION;
                float2 texcoord : TEXCOORD0;
                float3 normal:NORMAL;

            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal:TEXCOORD1;
                float3 worldViewDir:TEXCOORD2;
                float3 worldPos : TEXCOORD3;
            };

            sampler2D _MainTex;
            sampler2D _SpecularMap;
            sampler2D _RimMask;

            fixed4 _Color;
            fixed4 _RimColor;
            half  _RimPower;
            fixed4 _Specular;
            float _Gloss;

            v2f vert(a2v v){
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);

                float3 worldPos = mul((float3x3)unity_ObjectToWorld, v.vertex);
                o.worldPos = worldPos;
                o.worldViewDir = UnityWorldSpaceViewDir(worldPos);
                return o;
            }


            fixed4 frag(v2f o) : SV_Target
            {

                fixed3 color;
               
                fixed3 worldLightDir = normalize(UnityWorldSpaceLightDir(o.worldPos));
                fixed3 worldNormal = normalize(o.worldNormal);
                fixed3 worldViewDir = normalize(o.worldViewDir);


                fixed3 albedo = tex2D(_MainTex, o.uv.xy).rgb * _Color.rgb;

                //环境光照
                fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * albedo;

                fixed3 lambert = 0.5 * dot(worldNormal, worldLightDir) + 0.5;
                
                //漫反射
                fixed3 diffuse = _LightColor0.rgb * albedo * lambert;

                //高光
                fixed  specularMask = tex2D(_SpecularMap, o.uv).r;
                fixed3 halfDir = normalize(worldLightDir + worldViewDir);
                fixed3 specular = _LightColor0.rgb * _Specular.rgb * pow(max(0, dot(worldNormal, halfDir)), _Gloss) * specularMask;

                color = ambient + diffuse + specular;

                //计算视线方向与法线方向的夹角，夹角越大，dot值越接近0，说明视线方向越偏离该点，也就是平视，该点越接近边缘
                float rim = 1 - max(0, dot(worldViewDir, worldNormal));

                //计算rimLight
                fixed3 rimColor = _RimColor * pow(rim, 1 / _RimPower);

                //通过RimMask控制是否有边缘光,Rim目前存在一张Alpha8类型的图片中
                fixed rimMask = tex2D(_RimMask, o.uv).r;
                rimColor = rimColor * (1-rimMask);

                //加上边缘颜色
                color += rimColor;
                return fixed4 (color,1.0);
               
            }
            ENDCG     
        }
       
    }
    
   FallBack "Specular"
}
