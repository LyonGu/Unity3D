#ifndef MYRP_UNLIT_INCLUDED
#define MYRP_UNLIT_INCLUDED

    /*
        Unity没有直接提供MVP矩阵二是拆开成提供两个矩阵M和VP是因为VP矩阵在一帧中不会改变，可以重复利用。
        Unity将M矩阵和VP矩阵存入Constant Buffer中以提高运算效率，M矩阵存入的buffer为UnityPerDraw buffer,
        也就是针对每个物体的绘制不会改变。VP矩阵则存入的是UnityPerFrame buffer，即每一帧VP矩阵并不会改变。
        Constant Buffer并不是所有平台都支持，目前OpenGL就不支持 --> fuck 那移动平台？

        因为constant buffer并不是支持所有平台，所以我们使用宏来代替直接cbuffer keyword，
        使用CBUFFER_START 和CBUFFER_END 这两个宏需要使用Core Library，通过package manager可以安装。 
        安装成功后，在hlsl代码中引入common.hlsl就可以使用这两个宏了。


    */

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #define UNITY_MATRIX_M unity_ObjectToWorld
    /*
        The include file is UnityInstancing.hlsl,
        and because it might redefine UNITY_MATRIX_M we have to include it after defining that macro ourselves.
    */
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

    CBUFFER_START(UnityPerFrame)
        float4x4 unity_MatrixVP;
    CBUFFER_END

    CBUFFER_START(UnityPerDraw)
        float4x4 unity_ObjectToWorld;
    CBUFFER_END

    // CBUFFER_START(UnityPerMaterial)
	//     float4 _Color;
    // CBUFFER_END

    //手动创建一个常量缓存来存储颜色数据
    UNITY_INSTANCING_BUFFER_START(PerInstance)
	    UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
    UNITY_INSTANCING_BUFFER_END(PerInstance)

    /*
        We must now either use unity_ObjectToWorld when not instancing,
        or a matrix array when we are instancing.
        To keep the code in UnlitPassVertex the same for both cases,
        we'll define a macro for the matrix, specifically UNITY_MATRIX_M.
        We use that macro name, because the core library has an include file that defines macros to support instancing for us,
        and it also redefines UNITY_MATRIX_M to use the matrix array when needed.
    */




    struct VertexInput {
        float4 pos : POSITION;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct VertexOutput {
        float4 clipPos : SV_POSITION;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    VertexOutput UnlitPassVertex (VertexInput input) {
        VertexOutput output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_TRANSFER_INSTANCE_ID(input, output);
        float4 worldPos = mul(UNITY_MATRIX_M, float4(input.pos.xyz, 1.0));
	    output.clipPos = mul(unity_MatrixVP, worldPos);
        return output;
    }

    float4 UnlitPassFragment (VertexOutput input) : SV_TARGET {
        UNITY_SETUP_INSTANCE_ID(input);
	    return UNITY_ACCESS_INSTANCED_PROP(PerInstance, _Color);
    }

#endif // MYRP_UNLIT_INCLUDED
