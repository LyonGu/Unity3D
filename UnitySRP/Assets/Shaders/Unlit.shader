Shader "MyPipine/Unlit"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma target 3.5

            //GUP Instance --》 需要同样的mesh和material
            /*
                When instancing is enabled, the GPU is told to draw the same mesh multiple times with the same constant data
            */
            #pragma multi_compile_instancing
            #pragma instancing_options assumeuniformscaling
			#pragma vertex UnlitPassVertex
			#pragma fragment UnlitPassFragment

			#include "ShaderLibrary/Unlit.hlsl"
			ENDHLSL
        }
    }
}
