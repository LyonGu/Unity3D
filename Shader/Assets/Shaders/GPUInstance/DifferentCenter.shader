Shader "GPUInstance/DifferentCenter"
{
    /*
        使用GPUInstance技术
    */
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_CenterX("CenterX",float) = 0.5
		_CenterY("CenterY",float) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
        LOD 100
		Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            //第一步先在预处理指令那里添加上下面的代码,会根据是否开启GPU Instance生成不同的shader变体
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                //第二步分别在输入输出结构体内都添加下面的宏,用于在结构体中定义SV_InstanceID的元素
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                //第二步分别在输入输出结构体内都添加下面的宏,用于在结构体中定义SV_InstanceID的元素
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            //自定义属性，其实都是数组 "Props"--> 这个可以使任意的，但是要保证下方取的时候一致
            UNITY_INSTANCING_BUFFER_START(Props)
				UNITY_DEFINE_INSTANCED_PROP(float, _CenterX)
				UNITY_DEFINE_INSTANCED_PROP(float, _CenterY)
			UNITY_INSTANCING_BUFFER_END(Props)

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                /*
                    第三步在顶点着色器开头声明输出结构体后添加如下代码,从输入结构体v中读取Instance ID. 然后给输出结构体o中的相关元素赋值
                */
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v,o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                /*
                    第四步,在片元着色器开头添加如下代码,从输入结构体i中读取instance的元素,让片元着色器能够访问并使用Instance的属性.
                    如果不需要使用对应属性，就不用写
                */
                UNITY_SETUP_INSTANCE_ID(i);
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

                //使用自定义属性
                float cx = UNITY_ACCESS_INSTANCED_PROP(Props, _CenterX); //Props为UNITY_INSTANCING_BUFFER_START设置
				float cy = UNITY_ACCESS_INSTANCED_PROP(Props, _CenterY);
                float2 offset = abs(i.uv - float2(cx, cy));
                return fixed4(col.rgb,offset.x + offset.y);
            }
            ENDCG
        }
    }
}
