#if !defined(FLAT_WIREFRAME_INCLUDED)
#define FLAT_WIREFRAME_INCLUDED

#define CUSTOM_GEOMETRY_INTERPOLATORS \
	float2 barycentricCoordinates : TEXCOORD9; //使用第十个插值器语义为它提供一个float3 barycentricCoordinators向量。

#include "My Lighting Input_Wireframe.cginc"

float3 _WireframeColor;
float _WireframeSmoothing;
float _WireframeThickness;

float3 GetAlbedoWithWireframe (Interpolators i) {
	float3 albedo = GetAlbedo(i);
	float3 barys;
	barys.xy = i.barycentricCoordinates;
	barys.z = 1 - barys.x - barys.y;
	float3 deltas = fwidth(barys);
	float3 smoothing = deltas * _WireframeSmoothing;
	float3 thickness = deltas * _WireframeThickness;
	barys = smoothstep(thickness, thickness + smoothing, barys);
	float minBary = min(barys.x, min(barys.y, barys.z));
	return lerp(_WireframeColor, albedo, minBary);
}

#define ALBEDO_FUNCTION GetAlbedoWithWireframe

#include "My Lighting_Wireframe.cginc"

struct InterpolatorsGeometry {
	InterpolatorsVertex data;
	CUSTOM_GEOMETRY_INTERPOLATORS
};

//必须声明它将输出多少个顶点。此数字可能有所不同，因此我们需要提供一个最大值。
//因为我们正在处理三角形，所以每次调用总是输出三个顶点。通过将maxvertexcount属性添加到我们的函数中（以3作为参数）来指定。

//由于几何着色器可以输出的顶点数量各不相同，因此我们没有统一的返回类型。
//相反，几何着色器将写入图元流。在我们的例子中，它是一个TriangleStream，必须将其指定为inout参数。
//TriangleStream的工作方式类似于C＃中的泛型类型。它需要知道我们要提供的顶点数据的类型，它仍然是InterpolatorsVertex。
//调整MyGeometryProgram的流数据类型，使其使用新结构，是为了计算三角形的重心
//在函数内部定义此类型的变量，将输入数据分配给它们，然后将其附加到流中，而不是直接将输入传递给它们。
[maxvertexcount(3)]
void MyGeometryProgram (
	triangle InterpolatorsVertex i[3],
	inout TriangleStream<InterpolatorsGeometry> stream
) {

	//要找到三角形的法线向量，请先提取其三个顶点的世界位置
	float3 p0 = i[0].worldPos.xyz;
	float3 p1 = i[1].worldPos.xyz;
	float3 p2 = i[2].worldPos.xyz;

	//执行标准化的叉积，每个三角形一次。
	float3 triangleNormal = normalize(cross(p1 - p0, p2 - p0));

	//用该三角形法线替换顶点法线。
	i[0].normal = triangleNormal;
	i[1].normal = triangleNormal;
	i[2].normal = triangleNormal;

	InterpolatorsGeometry g0, g1, g2;
	g0.data = i[0];
	g1.data = i[1];
	g2.data = i[2];

	//给每个顶点一个重心坐标。哪个顶点获得什么坐标都没有关系，只要它们是有效的即可。
	g0.barycentricCoordinates = float2(1, 0);
	g1.barycentricCoordinates = float2(0, 1);
	g2.barycentricCoordinates = float2(0, 0);

	//将顶点数据放入流中
	stream.Append(g0);
	stream.Append(g1);
	stream.Append(g2);
}

#endif