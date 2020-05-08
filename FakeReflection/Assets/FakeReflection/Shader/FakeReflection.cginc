#ifndef __FAKE_REFLECTION__
#define __FAKE_REFLECTION__

#include "UnityCG.cginc"

inline half3 FakeReflection_BoxProjectedCubemapDirection(half3 worldRefl, float3 worldPos, float3 cubemapCenter, float3 boxMin, float3 boxMax)
{
	half3 nrdir = normalize(worldRefl);

	half3 rbmax = (boxMax.xyz - worldPos) / nrdir;
	half3 rbmin = (boxMin.xyz - worldPos) / nrdir;

	half3 rbminmax = (nrdir > 0.0f) ? rbmax : rbmin;

	half fa = min(min(rbminmax.x, rbminmax.y), rbminmax.z);

	worldPos -= cubemapCenter.xyz;
	worldRefl = worldPos + nrdir * fa;

	return worldRefl;
}

#define DECLARE_FAKE_REFLECTION UNITY_DECLARE_TEXCUBE(_FakeReflectionCube); \
								float3 _FakeReflectionCenter; \
								float3 _FakeReflectionSize; \
								half _FakeReflectionRoughness; \
								half3 _FakeReflectionForwardAxis; \
								half3 _FakeReflectionPos;

#define V2F_FAKE_REFLECTION(idx1, idx2) float3 wPos_fakeReflection : TEXCOORD##idx1; \
										float3 wNormal_fakeReflection : TEXCOORD##idx2;

#define V2F_FAKE_REFLECTION2(idx1) float3 wPos_fakeReflection : TEXCOORD##idx1;

#define FAKE_REFLECTION_TRANSFORM(o, v) o.wPos_fakeReflection = mul(unity_ObjectToWorld, v).xyz; \
										o.wNormal_fakeReflection = mul((float3x3)unity_WorldToObject, _FakeReflectionForwardAxis);

#define FAKE_REFLECTION_TRANSFORM2(o, v) o.wPos_fakeReflection = mul(unity_ObjectToWorld, v).xyz;

#if defined(_FAKE_REFLECTION_LOCAL)
	#define FAKE_REFLECTION_LOCAL(wRefl, wPos) wRefl = FakeReflection_BoxProjectedCubemapDirection(wRefl, wPos, _FakeReflectionPos, _FakeReflectionCenter - _FakeReflectionSize, _FakeReflectionCenter + _FakeReflectionSize); 
#else
	#define FAKE_REFLECTION_LOCAL(wRefl, wPos) 0;
#endif

#if defined(_FAKE_REFLECTION_INNER)
	#define FAKE_REFLECTION_INNER(wRefl, wViewDir, distortion) wRefl = wViewDir;
#else
	#define FAKE_REFLECTION_INNER(wRefl, wViewDir, distortion) wRefl = normalize(reflect(wViewDir_fakeReflection, normalize(wNormal_fakeReflection + distortion)));;
#endif

#define FAKE_REFLECTION_APPLY_INNER(i, c, distortion) float3 wViewDir_fakeReflection = normalize(i.wPos_fakeReflection - _WorldSpaceCameraPos.xyz); \
													  float3 wReflection_fakeReflection = 0; \
													  FAKE_REFLECTION_INNER(wReflection_fakeReflection, wViewDir_fakeReflection, distortion); \
													  FAKE_REFLECTION_LOCAL(wReflection_fakeReflection, i.wPos_fakeReflection); \
													  c.rgb = UNITY_SAMPLE_TEXCUBE_LOD(_FakeReflectionCube, wReflection_fakeReflection, _FakeReflectionRoughness).rgb;


#define FAKE_REFLECTION_APPLY(i, c, distortion) float3 wNormal_fakeReflection = normalize(i.wNormal_fakeReflection); \
												FAKE_REFLECTION_APPLY_INNER(i, c, distortion);
											    
#define FAKE_REFLECTION_APPLY2(i, c, distortion) float3 wNormal_fakeReflection = mul((float3x3)unity_WorldToObject, _FakeReflectionForwardAxis); \
												 FAKE_REFLECTION_APPLY_INNER(i, c, distortion);

#endif