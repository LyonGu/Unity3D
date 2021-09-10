

1 ***************矩阵
{
	1 位移T 旋转R 缩放S： 多个矩阵一起时有先后顺序 先缩放在旋转最后位移 ==> 从右开始 TRS
	2 齐次坐标：第四个值为1表示一个点，第四个值为0表示一个向量
	{
		[x]   [x]
		[y]   [y]
		[z]   [z]
		[1]   [0]

		第四个值为0表示一个向量==> 可以用来变换位置、法线、切线
	}

	3 旋转矩阵
	{
		绕Z轴
		{
			[cosZ -sinZ][x]
			[sinZ  cosZ][y] 

			==> 扩展到三维

			[cosZ -sinZ 0] [x]
			[sinZ  cosZ 0] [y]
			[0 		 0  1] [z]

		}

		绕X轴
		{

			[1   0		0]  [x]
			[0  cosX -sinX] [y]
			[0 	sinX  cosX] [z]
		}

		绕Y轴
		{

			[cosY   0  sinY	]   [x]
			[0  	1 	0	] 	[y]
			[-sinY 	0  cosY	] 	[z]
		}
	}

	4 正交矩阵
	5 透视矩阵

}

2 ***************着色器

3 ***************组合纹理
{
	texture 勾选rRGB，会对颜色进行一次伽马矫正

	多个纹理融合{
		一张主纹理
		其他纹理融合比例加起来为1

		float4 splat = tex2D(_MainTex, i.uvSplat);
				return
					tex2D(_Texture1, i.uv) * splat.r +
					tex2D(_Texture2, i.uv) * splat.g +
					tex2D(_Texture3, i.uv) * splat.b +
					tex2D(_Texture4, i.uv) * (1 - splat.r - splat.g - splat.b);
	}

	细节纹理

	线性空间
	{
		Unity假定纹理和颜色存储为sRGB。在伽玛空间中渲染时，着色器直接访问原始颜色和纹理数据。这就是我们到目前为止所假设的。
		但在线性空间中渲染时，这不再成立，GPU将纹理样本转换为线性空间
		同样，Unity还将材质颜色属性转换为线性空间。然后，着色器将使用这些线性颜色进行操作。之后，片段程序的输出会被转换回伽玛空间。

		UnityCG定义了一个统一变量，该变量将包含要乘以的正确数字。它是一个float4，其rgb分量视情况而定为2或大约4.59。由于伽马校正未应用于Alpha通道，因此始终为2。
		float4 MyFragmentProgram (Interpolators i) : SV_TARGET {
			float4 color = tex2D(_MainTex, i.uv) * _Tint;
			color *= tex2D(_DetailTex, i.uvDetail) * unity_ColorSpaceDouble;
			return color;
		}

		伽玛空间==》 访问的是原始颜色和纹理数据
		线性空间==》 访问的是转换为的线性数据，但是片段最后输出会被转换回伽玛空间

	}

}


4 ***************光照
{
	1 法线从物体空间转化为世界空间 ==》 逆转置矩阵 是为了解决因为当曲面沿一个纬度拉伸时，其法线不会以相同的方式拉伸。需要使用逆转置矩阵 i.normal = UnityObjectToWorldNormal(v.normal);
	2 点积
	{
		两个向量之间的点积在几何上定义为A⋅B= || A || || B || cosθ。这意味着它是矢量之间的角度的余弦乘以它们的长度。因此，在两个单位矢量的情况下，A⋅B=cosθ。

		这意味着你可以通过将所有组件对相乘，并用求和来计算它。
		float dotProduct = v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;

		在视觉上，此操作将一个向量直接投影到另一个向量上。仿佛在其上投下阴影。这样，你最终得到一个直角三角形，其底边的长度是点积的结果。而且，如果两个向量都是单位长度，那就是它们角度的余弦。
	}
	2 漫反射
	{
		表面法线和光方向的点积确定的是反射率

		_WorldSpaceLightPos0 ==> 仅仅有方向光的时候就表示方向光的方向，有多个光源的时候就代表方向光的位置


		材质的漫反射率的颜色称为反照率，可以使用材质的纹理和色调来定义它
		Albedo ==> 反照率 
		float3 albedo = tex2D(_MainTex, i.uv).rgb * _Tint.rgb;

		float4 MyFragmentProgram (Interpolators i) : SV_TARGET {
				i.normal = normalize(i.normal);
				float3 lightDir = _WorldSpaceLightPos0.xyz;
				float3 lightColor = _LightColor0.rgb;
				float3 albedo = tex2D(_MainTex, i.uv).rgb * _Tint.rgb;
				float3 diffuse =
					albedo * lightColor * DotClamped(lightDir, i.normal);
				return float4(diffuse, 1);
	}

	3 镜面反射
	{
		当光线撞击表面后没有发生扩散时，就会发生这种情况。取而代之的是，光线以等于其撞击表面的角度的角度从表面反弹。比如你在镜子中看到的各种反射。

		观察者的位置很重要   _WorldSpaceCameraPos 摄像机的位置
		观察方向：float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);

		要了解反射光的去向，我们可以使用标准反射功能。它接受入射光线的方向，并根据表面法线对其进行反射。因此，我们必须取反光的方向。
		float3 reflectionDir = reflect(-lightDir, i.normal);
		return float4(reflectionDir * 0.5 + 0.5, 1);

		你可以通过D-2N(N·D)的公式，用法线N计算出方向D

		高光反射其实就是一个亮点：如果有一面完美光滑的镜子，我们只会看到表面角度恰好合适的反射光。在所有其他地方，反射光都会错开我们，并且表面对我们而言将显示为黑色

		光滑度
			通过这种效果产生的高光的大小取决于材质的粗糙度。光滑的材质可以更好地聚焦光线，因此高光较小


		模型是Blinn-Phong
			用半视角方向代替反射方向 
			float3 halfVector = normalize(lightDir + viewDir);
			return pow(
						DotClamped(halfVector, i.normal),
						_Smoothness * 100
					);

		高光颜色
			当然，镜面反射的颜色需要与光源的颜色匹配。因此，把这个也考虑在内
			float3 halfVector = normalize(lightDir + viewDir);
			float3 specular = lightColor * pow(
				DotClamped(halfVector, i.normal),
				_Smoothness * 100
			);

			return float4(specular, 1);

		高光反射的颜色也取决于材质
			添加纹理和色调以定义镜面反射颜色 _SpecularTint为纯色
			float3 halfVector = normalize(lightDir + viewDir);
				float3 specular = _SpecularTint.rgb * lightColor * pow(
					DotClamped(halfVector, i.normal),
					_Smoothness * 100
				);

				return float4(specular, 1);


		整合漫反射和高光反射
		Diffuse and Specular
			return float4(diffuse + specular, 1);

	}

	4 能量守恒
	{
		仅将漫反射和镜面反射加在一起会存在一个问题。结果可能比光源还亮。当使用全白镜面反射和低平滑度时，这一点非常明显。
		当光线撞击表面时，其中一部分会反射为镜面反射光。它的其余部分穿透表面，或者以散射光的形式返回，或者被吸收。
		但是我们目前没有考虑到这一点。取而代之的是，我们的光会全反射和扩散。因此，最终可能将光的能量加倍了。

		必须确保材质的漫反射和镜面反射部分的总和不超过1。这保证了我们不会在任何地方产生光

		当使用恒定的镜面反射色时，我们可以简单地通过将反照率乘以1减去镜面反射来调整反照率色度
		float3 albedo = tex2D(_MainTex, i.uv).rgb * _Tint.rgb;
		albedo *= 1 - _SpecularTint.rgb;
	}

	5 PBR 金属度工作流
	{
		其实我们主要关注两种材质就好。金属和非金属。后者也称为介电材料。目前，我们可以通过使用强镜面反射色来创建金属。使用弱的单色镜面反射来创建介电材质。这是镜面反射工作流程

		金属工作流更简单
			如果我们可以仅在金属和非金属之间切换，那将更加简单。由于金属没有反照率（albedo），因此我们可以使用该颜色数据作为镜面反射色。
			而非金属也没有彩色的镜面反射，因此我们根本不需要单独的镜面反射色调。这称为金属工作流程。

		金属化的工作流程更为简单，因为你只有一个颜色来源和一个滑块。这足以创建逼真的材质。
		镜面反射工作流程可以产生相同的结果，但是由于你拥有更多的控制权，因此也可能出现不切实际的材质


		我们可以使用另一个滑块属性作为金属切换，以替换镜面反射色调。通常，应将其设置为0或1，因为某物如果不是金属。就用介于两者之间的值表示混合金属和非金属成分的材质
				float3 specularTint = albedo * _Metallic; //从反照率和金属特性中得出镜面反射色
				float oneMinusReflectivity = 1 - _Metallic;
				albedo *= oneMinusReflectivity; //为了能量守恒 
				
				float3 diffuse =
					albedo * lightColor * DotClamped(lightDir, i.normal);

				float3 halfVector = normalize(lightDir + viewDir);
				float3 specular = specularTint * lightColor * pow(
					DotClamped(halfVector, i.normal),
					_Smoothness * 100
				);

		但是，这过于简单了。即使是纯介电材质，也仍然具有镜面反射。
		因此，镜面强度和反射值与金属滑块的值不完全匹配。而且这也受到色彩空间的影响。幸运的是，UnityStandardUtils还具有DiffuseAndSpecularFromMetallic函数，该函数为我们解决了这一问题。
				float3 specularTint; // = albedo * _Metallic;
				float oneMinusReflectivity; // = 1 - _Metallic;
//				albedo *= oneMinusReflectivity;
				albedo = DiffuseAndSpecularFromMetallic(
					albedo, _Metallic, specularTint, oneMinusReflectivity
				);


		一个细节是金属滑块本身应该位于伽马空间中。但是，在线性空间中渲染时，单个值不会被Unity自动伽玛校正。我们可以使用Gamma属性来告诉Unity，它也应该将gamma校正应用于金属滑块。
		[Gamma] _Metallic ("Metallic", Range(0, 1)) = 0

	}

	6 基于物理的着色（PBS）
	{
		长期以来，Blinn-Phong一直是游戏行业的主力军，但如今，基于物理的阴影（称为PBS）风靡一时。
		这是有充分理由的，因为它更加现实和可预测。理想情况下，游戏引擎和建模工具都使用相同的着色算法。这使内容创建更加容易。业界正在慢慢地趋向于标准PBS实施。

		Unity的标准着色器也使用PBS方法。Unity实际上有多种实现。
		它根据目标平台，硬件和API级别决定使用哪个。可通过UnityPBSLighting中定义的UNITY_BRDF_PBS宏访问该算法。BRDF代表双向反射率分布函数。

		为了确保Unity选择最佳的BRDF功能，我们必须至少定位着色器级别3.0。我们用语用表述来做到这一点
		#pragma target 3.0

		Unity的BRDF函数返回RGBA颜色，且alpha分量始终设置为1。因此，我们可以直接让我们的片段程序返回其结果。
		return UNITY_BRDF_PBS();

		当然，我们必须使用参数来调用它。每个功能都有八个参数。前两个是 材质的漫反射和镜面反射颜色
		return UNITY_BRDF_PBS(
					albedo, specularTint
				);

		接下来的两个参数必须是反射率和粗糙度。这些参数必须为一减形式，这是一种优化。
		我们已经从DiffuseAndSpecularFromMetallic中获得了一减反射率。平滑度与粗糙度相反，因此我们可以直接使用它。
		return UNITY_BRDF_PBS(
					albedo, specularTint,
					oneMinusReflectivity, _Smoothness
				);

		还需要表面法线和视角方向。这些成为第五和第六个参数
		return UNITY_BRDF_PBS(
					albedo, specularTint,
					oneMinusReflectivity, _Smoothness,
					i.normal, viewDir
				);

		最后两个参数是直接光和间接光
			UnityLight light;
			light.color = lightColor;
			light.dir = lightDir;
			light.ndotl = DotClamped(i.normal, lightDir);

			UnityIndirect indirectLight;
			indirectLight.diffuse = 0;
			indirectLight.specular = 0;
			
			return UNITY_BRDF_PBS(
					albedo, specularTint,
					oneMinusReflectivity, _Smoothness,
					i.normal, viewDir,
					light, indirectLight
				);


		完整的代码
		{
			float4 MyFragmentProgram (Interpolators i) : SV_TARGET {
				i.normal = normalize(i.normal);
				float3 lightDir = _WorldSpaceLightPos0.xyz;
				float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);

				float3 lightColor = _LightColor0.rgb;
				float3 albedo = tex2D(_MainTex, i.uv).rgb * _Tint.rgb;

				float3 specularTint;
				float oneMinusReflectivity;
				albedo = DiffuseAndSpecularFromMetallic(
					albedo, _Metallic, specularTint, oneMinusReflectivity
				);
				
				UnityLight light;
				light.color = lightColor;
				light.dir = lightDir;
				light.ndotl = DotClamped(i.normal, lightDir);
				UnityIndirect indirectLight;
				indirectLight.diffuse = 0;
				indirectLight.specular = 0;

				return UNITY_BRDF_PBS(
					albedo, specularTint,
					oneMinusReflectivity, _Smoothness,
					i.normal, viewDir,
					light, indirectLight
				);
			}
		}

	}



}
