Shader "Shaders/Chapter5/SimpleShader_5"  //shader的名称，这里的写法会在选择shader时生效
{
    //声明属性
    Properties{
        //声明一个Color类型的属性
        _Color1 ("Color1 Tint", Color) = (1.0,1.0,1.0,1.0)
        _Color2 ("Color2 Tint", Color) = (1.0,1.0,1.0,1.0)
        _TransVal ("Transparency Value", Range(0,1)) = 0.5
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }

	SubShader {
        //Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}

        //Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass {
            CGPROGRAM
            #include "UnityCG.cginc"

            #pragma vertex vert
            #pragma fragment frag

            //必须定义一个与属性名称类型一致的变量
            fixed4 _Color1;
            fixed4 _Color2;
            fixed _TransVal;
            sampler2D _MainTex;
    

            //使用一个结构体来定义顶点着色器的输入
            struct a2v{
                float4 vertex : POSITION; //POSITION告诉Unity,用模型空间的坐标填充vertext
                float3 normal : NORMAL;     //NORMAL告诉Unity,用模型空间的法线方向填充normal
                float4 texcoord : TEXCOORD0;  //TEXCOORD告诉Unity,用0级纹理填充texcoord


            };

            //使用过一个结构体来定义顶点着色器的输出
            struct v2f{
                float4 pos: SV_POSITION;  //SV_POSITION告诉Unity,pos里包含了顶点在裁剪空间的位置信息
                fixed2 uv:TEXCOORD0;
            };
            
            v2f vert(a2v v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            //SV_Target：告诉渲染器 把用户的输出颜色存储到一个渲染目标中，这里输出到默认的帧缓冲中
        	fixed4 frag(v2f i):SV_Target
        	{  
                fixed4 color = tex2D (_MainTex, i.uv);
                //fixed4 color1 = _Color1 * _Color2; //（r1*r2,g1*g2,b1*b2,a1*a2） ==> 但是alpha通道只有开启混合才有效
                color.a = _TransVal; //alpha通道其实主要是用来起混合作用的，不开启的话其实没什么作用

        		return color;

        	}
            ENDCG
        }
    }
}
