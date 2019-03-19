Shader "Shaders/Chapter5/SimpleShader"  //shader的名称，这里的写法会在选择shader时生效
{
	
	SubShader {
        Pass {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            
            //POSITION 输入 ：把模型的顶点坐标填充到输入参数v中
            //SV_POSITION 输出: 顶点着色器输出的是裁剪空间的顶点坐标
        	float4 vert(float4 v : POSITION) : SV_POSITION{
        		return UnityObjectToClipPos(v);
        	}

            //SV_Target：告诉渲染器 把用户的输出颜色存储到一个渲染目标中，这里输出到默认的帧缓冲中
        	float4 frag():SV_Target
        	{
        		return fixed4(1.0,1.0,0.0,1.0);

        	}
            ENDCG
        }
    }
}
