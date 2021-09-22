#ifndef CUSTOM_UNITY_INPUT_INCLUDED
#define CUSTOM_UNITY_INPUT_INCLUDED

CBUFFER_START(UnityPerDraw)
	float4x4 unity_ObjectToWorld;
	float4x4 unity_WorldToObject;
	float4 unity_LODFade;
	real4 unity_WorldTransformParams;
    
    //光照贴图使用
	float4 unity_LightmapST;
	float4 unity_DynamicLightmapST;
    
    //光照探针使用
    /*
        光探针是场景中的一个点，通过用三阶多项式（特别是L2球谐函数）近似的将所有入射光进行烘焙
        数据由七个float4向量组成，分别代表红色，绿色和蓝色光的多项式的分量。
        它们的名称为unity_SH ，为A，B或C。前两个具有三个版本，后缀为r，g和b。
        
        适用于比较小的动态对象
    */
	float4 unity_SHAr;
	float4 unity_SHAg;
	float4 unity_SHAb;
	float4 unity_SHBr;
	float4 unity_SHBg;
	float4 unity_SHBb;
	float4 unity_SHC;
    
    
    //光照探针代理集 简称LPPV 适用于比较大的动态对象
	float4 unity_ProbeVolumeParams;
	float4x4 unity_ProbeVolumeWorldToObject;
	float4 unity_ProbeVolumeSizeInv;
	float4 unity_ProbeVolumeMin;
CBUFFER_END

float4x4 unity_MatrixVP;
float4x4 unity_MatrixV;
float4x4 glstate_matrix_projection;

float3 _WorldSpaceCameraPos;

#endif