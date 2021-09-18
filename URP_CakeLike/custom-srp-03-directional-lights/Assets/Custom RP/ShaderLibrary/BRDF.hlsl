#ifndef CUSTOM_BRDF_INCLUDED
#define CUSTOM_BRDF_INCLUDED

struct BRDF {
	float3 diffuse;
	float3 specular;
	float roughness;
};

#define MIN_REFLECTIVITY 0.04

/*
不同的表面，反射的方式不同，但通常金属会通过镜面反射反射所有光，并且漫反射为零。因此，我们将声明反射率等于金属表面属性
    float oneMinusReflectivity = 1.0 - surface.metallic;
	brdf.diffuse = surface.color * oneMinusReflectivity;
	
	非金属的反射率有所不同，但平均约为0.04。让我们将其定义为最小反射率，并添加一个OneMinusReflectivity函数，
	该函数将范围从0~1调整为0~0.96。此范围调整与Universal RP的方法匹配
*/
float OneMinusReflectivity (float metallic) {
	float range = 1.0 - MIN_REFLECTIVITY;
	return range - metallic * range;
}

BRDF GetBRDF (Surface surface, bool applyAlphaToDiffuse = false) {
	BRDF brdf;
	float oneMinusReflectivity = OneMinusReflectivity(surface.metallic);
    //散射的光 surface.color其实就是Albedo
    /*
        材质的漫反射率的颜色称为反照率，可以使用材质的纹理和色调来定义它
		Albedo ==> 反照率 
		float3 albedo = tex2D(_MainTex, i.uv).rgb * _Tint.rgb;
    */
	brdf.diffuse = surface.color * oneMinusReflectivity;
	if (applyAlphaToDiffuse) {
		brdf.diffuse *= surface.alpha;
	}
	//反射的光 surface.color其实就是Albedo
	brdf.specular = lerp(MIN_REFLECTIVITY, surface.color, surface.metallic);

	float perceptualRoughness =
		PerceptualSmoothnessToPerceptualRoughness(surface.smoothness);
	brdf.roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
	return brdf;
}

float SpecularStrength (Surface surface, BRDF brdf, Light light) {
	float3 h = SafeNormalize(light.direction + surface.viewDirection);
	float nh2 = Square(saturate(dot(surface.normal, h)));
	float lh2 = Square(saturate(dot(light.direction, h)));
	float r2 = Square(brdf.roughness);
	float d2 = Square(nh2 * (r2 - 1.0) + 1.00001);
	float normalization = brdf.roughness * 4.0 + 2.0;
	return r2 / (d2 * max(0.1, lh2) * normalization);
}

float3 DirectBRDF (Surface surface, BRDF brdf, Light light) {
	return SpecularStrength(surface, brdf, light) * brdf.specular + brdf.diffuse;
}

#endif