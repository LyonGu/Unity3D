Shader "Shaders/Chapter5/SimpleShader_4"  //shader的名称，这里的写法会在选择shader时生效
{
    //声明属性
    Properties{
        //声明一个Color类型的属性
        _Color ("Color Tint", Color) = (1.0,1.0,1.0,1.0)
        _MainTex ("Main Tex",2D) = "white" {}
    }

	SubShader {
        Pass {
            CGPROGRAM
            #include "UnityCG.cginc"

            #pragma vertex vert
            #pragma fragment frag

            //必须定义一个与属性名称类型一致的变量
            fixed4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            //使用一个结构体来定义顶点着色器的输入
            struct a2v{
                float4 vertex : POSITION; //POSITION告诉Unity,用模型空间的坐标填充vertext
                float3 normal : NORMAL;     //NORMAL告诉Unity,用模型空间的法线方向填充normal
                float4 texcoord : TEXCOORD0;  //TEXCOORD告诉Unity,用0级纹理填充texcoord


            };

            //使用过一个结构体来定义顶点着色器的输出
            struct v2f{
                float4 pos: SV_POSITION;  //SV_POSITION告诉Unity,pos里包含了顶点在裁剪空间的位置信息
                fixed3 color: COLOR0;      //COLOR0用于存储颜色信息

                float2 uv: TEXCOORD0;
            };
            
            v2f vert(a2v v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.normal*0.5 + fixed3(0.5,0.5,0.5);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);  
                return o;
            }

            //SV_Target：告诉渲染器 把用户的输出颜色存储到一个渲染目标中，这里输出到默认的帧缓冲中
        	fixed4 frag(v2f i):SV_Target
        	{
                fixed3 c = i.color;
                //使用_Color属性来控制输出颜色
                c = _Color.rgb * tex2D(_MainTex, i.uv);
        		return fixed4(c, 1.0);

        	}
            ENDCG
        }
    }
}
