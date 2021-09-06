#if !defined(MY_SHADOWS_INCLUDED_TransparentShadow)
#define MY_SHADOWS_INCLUDED_TransparentShadow

#include "UnityCG.cginc"

#if defined(_RENDERING_FADE) || defined(_RENDERING_TRANSPARENT)
	//fade模式或者transparent模式
	#if defined(_SEMITRANSPARENT_SHADOWS)
		//阴影半透
		#define SHADOWS_SEMITRANSPARENT 1
	#else
		//强制使用CUTOUT模式
		#define _RENDERING_CUTOUT
	#endif
#endif

#if SHADOWS_SEMITRANSPARENT || defined(_RENDERING_CUTOUT)
	//coutout模式
	#if !defined(_SMOOTHNESS_ALBEDO)
		// 不使用反照率的Alpha值确定平滑度时
		#define SHADOWS_NEED_UV 1
	#endif
#endif

float4 _Tint;
sampler2D _MainTex;
float4 _MainTex_ST;
float _AlphaCutoff;

sampler3D _DitherMaskLOD; //Unity的抖动模式纹理

struct VertexData {
	float4 position : POSITION;
	float3 normal : NORMAL;
	float2 uv : TEXCOORD0;
};

struct InterpolatorsVertex {
	float4 position : SV_POSITION;
	#if SHADOWS_NEED_UV
		float2 uv : TEXCOORD0; //cutout 模式需要uv坐标
	#endif
	#if defined(SHADOWS_CUBE)
		//点光阴影
		float3 lightVec : TEXCOORD1;
	#endif
};

struct Interpolators {
	#if SHADOWS_SEMITRANSPARENT
		UNITY_VPOS_TYPE vpos : VPOS; //通过在片段程序中添加带有VPOS语义的参数，可以访问片段的屏幕空间位置
	#else
		float4 positions : SV_POSITION;
	#endif

	#if SHADOWS_NEED_UV
		float2 uv : TEXCOORD0;
	#endif
	#if defined(SHADOWS_CUBE)
		float3 lightVec : TEXCOORD1;
	#endif
};

float GetAlpha (Interpolators i) {
	float alpha = _Tint.a;
	#if SHADOWS_NEED_UV
		//cutout 模式从albedo图中获取a通道
		alpha *= tex2D(_MainTex, i.uv.xy).a;
	#endif
	return alpha;
}

InterpolatorsVertex MyShadowVertexProgram (VertexData v) {
	InterpolatorsVertex i;
	#if defined(SHADOWS_CUBE)
		i.position = UnityObjectToClipPos(v.position);
		i.lightVec =
			mul(unity_ObjectToWorld, v.position).xyz - _LightPositionRange.xyz;
	#else
		i.position = UnityClipSpaceShadowCasterPos(v.position.xyz, v.normal);
		i.position = UnityApplyLinearShadowBias(i.position);
	#endif

	#if SHADOWS_NEED_UV
		//cutout模式
		i.uv = TRANSFORM_TEX(v.uv, _MainTex);
	#endif
	return i;
}

float4 MyShadowFragmentProgram (Interpolators i) : SV_TARGET {
	float alpha = GetAlpha(i);
	#if defined(_RENDERING_CUTOUT)
		//cutout模式
		clip(alpha - _AlphaCutoff);
	#endif

	#if SHADOWS_SEMITRANSPARENT
		//半透明模式
		//采样抖动模式纹理
		float dither =
			tex3D(_DitherMaskLOD, float3(i.vpos.xy * 0.25, alpha * 0.9375)).a;
		clip(dither - 0.01);
	#endif
	
	#if defined(SHADOWS_CUBE)
		float depth = length(i.lightVec) + unity_LightShadowBias.x;
		depth *= _LightPositionRange.w;
		return UnityEncodeCubeShadowDepth(depth);
	#else
		return 0;
	#endif
}

#if defined(SHADOWS_CUBE)

#endif

#endif