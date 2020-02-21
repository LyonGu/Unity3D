Shader "Shaders/Chapter5/SimpleShader_1"  //shader的名称，这里的写法会在选择shader时生效
{

	SubShader {
        Pass {
            CGPROGRAM
            #include "UnityCG.cginc"

            #pragma vertex vert
            #pragma fragment frag

            //使用一个结构体来定义顶点着色器的输入
            struct a2v{
                float4 vertex : POSITION; //POSITION告诉Unity,用模型空间的坐标填充vertext
                float3 normal : NORMAL;     //NORMAL告诉Unity,用模型空间的法线方向填充normal
                float4 texcoord : TEXCOORD;  //TEXCOORD告诉Unity,用0级纹理填充texcoord
            };

            float4 vert(a2v v): SV_POSITION
            {
                //使用v.vertext 来访问模型空间的顶点坐标
                return UnityObjectToClipPos(v.vertex);
            }

            //SV_Target：告诉渲染器 把用户的输出颜色存储到一个渲染目标中，这里输出到默认的帧缓冲中
        	float4 frag():SV_Target
        	{
        		return fixed4(1.0,0.0,1.0,1.0);

        	}
            ENDCG
        }
    }
}
