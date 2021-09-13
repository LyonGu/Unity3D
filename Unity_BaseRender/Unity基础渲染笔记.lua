

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
				//albedo *= oneMinusReflectivity;
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


5 ***************多光源光照
{
	支持不同光源，副光源需要额外的pass

	前向渲染
	{
		主光源：ForwardBase
		副光源：ForwardAdd
	}

	需要修改副光源的混合模式，否则会覆盖主光源的渲染结果
	{
		默认模式是不混合，等效于One Zero。这样通过的结果将替换帧缓冲区中以前的任何内容。要添加到帧缓冲区，我们必须指示它使用“ One One”混合模式。这称为additive blending。
		Blend One One
	}

	同一对象最终记录了完全相同的深度值，不需要两次写入深度缓冲区，因此可以禁用副光源的深度写入。这是通过ZWrite Off着色器语句完成的
	{
				Tags {
						"LightMode" = "ForwardAdd"
				}

				Blend One One
				ZWrite Off
	}

	
	多光源没法进去动态合批

	定向光没有衰减，点光和聚光有==》可以设置不同的宏来控制变体逻辑  
	{
		#pragma multi_compile DIRECTIONAL POINT SPOT

		multi_compile 关键字会编译所有的变体 不管是否使用到
		shader_feature 关键字只会编译使用到的，
	}


	Cookie
	{
		所有光都能设置cookie，

		默认的聚光灯蒙版纹理是模糊的圆圈。但是，你可以使用任何正方形纹理，只要它的边缘降至零即可。
		这些纹理称为聚光Cookies。此名称源自cucoloris，cucoloris是指将阴影添加到灯光中的电影，剧院或摄影道具。
		Cookie的Alpha通道用于遮挡光线。其他通道无关紧要。这是一个示例纹理，其中所有四个通道均设置为相同的值
		导入纹理时，可以选择Cookie作为其类型


		点光源也可以有Cookies。在这种情况下，光线会全方位传播，因此cookie必须包裹在一个球体上。这是通过使用立方体贴图完成的。
		你可以使用各种纹理格式来创建点光源cookie，Unity会将其转换为立方体贴图。你必须指定Mapping，以便Unity知道如何解释你的图像。最好的方法是自己提供一个立方体贴图，可以使用自动映射模式。
	}
	

	逐顶点光照（效率高，效果不是很好），逐像素光照(一般都用这个)
	{
		每个顶点渲染一个光源意味着你可以在顶点程序中执行光照计算。然后对所得颜色进行插值，并将其传递到片段程序。这非常廉价，
		以至于Unity在base pass中都包含了这种灯光。发生这种情况时，Unity会使用VERTEXLIGHT_ON关键字寻找base pass着色器变体

		仅点光源支持顶点光照。因此，定向灯和聚光灯不能使用。
	}

	球谐函数
	{
		当我们用完所有像素光源和所有顶点光源时，可以使用另一种渲染光源的方法，球谐函数。所有三种光源类型均支持此功能。
		球谐函数背后的想法是，你可以使用单个函数描述某个点处的所有入射光。此功能定义在球体的表面上。

		如果使用的频段少于完美匹配所需要的频段，那么最终结果将是原始功能的近似值。使用的频段越少，近似值的准确性就越低。该技术用于压缩很多东西，例如声音和图像数据。在我们的案例中，我们将使用它来近似3D照明。

		Unity仅使用前三个频段 最终结果是将所有九个条目加在一起。通过调制这九项中的每一项，会产生不同的照明条件，并附加一个系数。


		ShadeSH9==》只能在BasePass中使用
			每个被球谐函数近似的光都必须分解为27个数。幸运的是，Unity可以非常快速地做到这一点。base pass可以通过在UnityShaderVariables中定义的七个float4变量的集合来访问它们。
			UnityCG包含ShadeSH9函数，该函数根据球谐数据和法线参数计算照明。它需要一个float4参数，其第四部分设置为1。

		现在激活这一堆灯。请确保硬件有足够的性能，以便所有像素和顶点光都能用完。其余灯的被添加到球谐函数中。同样，Unity将拆分灯光以混合过渡。
		{
			间接光中使用
			UnityIndirect CreateIndirectLight (Interpolators i) {
				UnityIndirect indirectLight;
				indirectLight.diffuse = 0;
				indirectLight.specular = 0;

				#if defined(VERTEXLIGHT_ON)
					indirectLight.diffuse = i.vertexLightColor;
				#endif

				#if defined(FORWARD_BASE_PASS) //自定义的宏
					indirectLight.diffuse += max(0, ShadeSH9(float4(i.normal, 1))); //使用球谐函数
				#endif
				
				return indirectLight;
			}

			Pass {
				Tags {
					"LightMode" = "ForwardBase"
				}

				CGPROGRAM
				#define FORWARD_BASE_PASS
				ENDCG
			}

		}


	}
	
}


6 ***************凹凸 (多数用法线贴图控制)
{
	高度贴图:在纹理中存储这个海拔数据
	法线贴图:将法线存储在纹理中 
	{
		我们必须通过计算2N-1之后将法线转换回其原始的-1~1的范围,从贴图中采样出来的数据范围为0~1 ：
		{
			DXT5nm格式仅存储法线的X和Y分量。其Z分量将被丢弃。如你所料，Y分量存储在G通道中。但是，X分量存储在A通道中。不使用R和B通道。
			i.normal.xy = tex2D(_NormalMap, i.uv).wy * 2 - 1;
		}

		凹凸缩放
		{
			void InitializeFragmentNormal(inout Interpolators i) {
				i.normal.xy = tex2D(_NormalMap, i.uv).wy * 2 - 1;
				i.normal.xy *= _BumpScale; //缩放值
				i.normal.z = sqrt(1 - saturate(dot(i.normal.xy, i.normal.xy)));
				i.normal = i.normal.xzy;
				i.normal = normalize(i.normal);

			}
		}

		UnityStandardUtils包含UnpackScaleNormal函数。它会自动对法线贴图使用正确的解码，并缩放法线。因此，让我们利用该便捷功能。
		{
			当定位不支持DXT5nm的平台时，Unity定义UNITY_NO_DXT5nm关键字。在这种情况下，该功能将切换为RGB格式，并且不支持正常缩放。
			由于指令限制，在定位Shader Model 2时，它也不支持缩放。因此，在定位移动设备时，请勿依赖凹凸缩放。

			void InitializeFragmentNormal(inout Interpolators i) {
				//	i.normal.xy = tex2D(_NormalMap, i.uv).wy * 2 - 1;
				//	i.normal.xy *= _BumpScale;
				//	i.normal.z = sqrt(1 - saturate(dot(i.normal.xy, i.normal.xy)));
					i.normal = UnpackScaleNormal(tex2D(_NormalMap, i.uv), _BumpScale);
					i.normal = i.normal.xzy;
					i.normal = normalize(i.normal);
				}

			//另一种写法
			//从normalMap获取像素
				fixed4 packedNormal = tex2D(_BumpMap,i.uv);
				fixed3 tangentNormal;

				//法线贴图被标记了 "Normal Map"需要先解压缩
				tangentNormal = UnpackNormal(packedNormal);
				tangentNormal.xy *= _BumpScale;
				tangentNormal.z = sqrt(1.0 - saturate(dot(tangentNormal.xy, tangentNormal.xy)));
		}

	}

	细节法线贴图：感觉手机上用的不多

	切线空间
	{
		struct VertexData {
			float4 position : POSITION;
			float3 normal : NORMAL;
			float4 tangent : TANGENT; //顶点的切线
			float2 uv : TEXCOORD0;
		};

		struct Interpolators {
			float4 position : SV_POSITION;
			float4 uv : TEXCOORD0;
			float3 normal : TEXCOORD1;

			#if defined(BINORMAL_PER_FRAGMENT)
				float4 tangent : TEXCOORD2; //传到像素着色器里的切线
			#else
				float3 tangent : TEXCOORD2;
				float3 binormal : TEXCOORD3;
			#endif

			float3 worldPos : TEXCOORD4;

			#if defined(VERTEXLIGHT_ON)
				float3 vertexLightColor : TEXCOORD5;
			#endif
		};

		Interpolators MyVertexProgram (VertexData v) {
			Interpolators i;
			i.position = UnityObjectToClipPos(v.position);
			i.worldPos = mul(unity_ObjectToWorld, v.position);
			i.normal = UnityObjectToWorldNormal(v.normal); // 世界空间法线

			#if defined(BINORMAL_PER_FRAGMENT)
				//逐像素 计算副法线
				//binormal在fs里需要叉积计算
				i.tangent = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);  //世界空间切线
			#else
				//逐顶点 计算副法线
				i.tangent = UnityObjectToWorldDir(v.tangent.xyz);
				i.binormal = CreateBinormal(i.normal, i.tangent, v.tangent.w); //binormal在fs里直接使用
			#endif
				
			i.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
			i.uv.zw = TRANSFORM_TEX(v.uv, _DetailTex);
			ComputeVertexLightColor(i);
			return i;
		}

		void InitializeFragmentNormal(inout Interpolators i) {
			float3 mainNormal =
				UnpackScaleNormal(tex2D(_NormalMap, i.uv.xy), _BumpScale); //UnpackScaleNormal得到的是切线空间的数据
			float3 detailNormal =
				UnpackScaleNormal(tex2D(_DetailNormalMap, i.uv.zw), _DetailBumpScale);
			float3 tangentSpaceNormal = BlendNormals(mainNormal, detailNormal);

			#if defined(BINORMAL_PER_FRAGMENT)
				float3 binormal = CreateBinormal(i.normal, i.tangent.xyz, i.tangent.w); //世界空间副法线
			#else
				float3 binormal = i.binormal;
			#endif
			
			//从切线空间转到世界空间==》得到世界空间的法线
			i.normal = normalize(
				tangentSpaceNormal.x * i.tangent +
				tangentSpaceNormal.y * binormal +
				tangentSpaceNormal.z * i.normal
			);
		}
	}
}


7 ***************阴影
{
	1 定向光创建阴影
	{
		阴影贴图：Unity以某种方式将阴影信息存储在纹理中

		禁用阴影时，将照常渲染所有对象。我们已经熟悉此过程。但是，启用阴影后，该过程将变得更加复杂。还有更多的渲染阶段，还有很多DrawCall。


		渲染到深度纹理：屏幕分辨率多大，对应的贴图就多大 _cameraDepthTexture（场景的深度纹理）
		{
			Unity开始进行渲染过程的深度 pass。将结果放入与屏幕分辨率匹配的纹理中。此过程渲染整个场景，但仅记录每个片段的深度信息
			此数据与片段空间中片段的Z坐标相对应。这是定义相机可以看到的区域的空间。深度信息最终存储为0-1范围内的值。查看纹理时，附近的纹素看起来很暗。纹素越远，它变得越轻
		}

		渲染到阴影贴图：也仅有深度信息（灯光的深度纹理）
		{

			从光源的角度渲染场景（每个光源都会渲染一张阴影贴图），仅将深度信息存储在纹理
			深度值告诉我们一束光线在撞击某物之前经过了多远。这可以用来确定是否有阴影

			事实证明Unity不只为每个光源渲染整个场景一次，而是每个灯光渲染场景四次！纹理被分为四个象限，每个象限是从不同的角度渲染的。发生这种情况是因为我们选择使用四个阴影级联


			从摄像机的角度来看，我们可以获得场景的深度信息。从每种光源的角度来看，我们也有此信息。
			当然，这些数据存储在不同的空间中（一个世界空间一个视觉空间），但是我们知道这些空间的相对位置和方向。这样我们就可以从一个空间转换为另一个空间。
			这使我们可以从两个角度比较深度测量值。从概念上讲，我们有两个向量在同一点结束。
			如果他们确实到在同一点结束了，则相机和灯光都可以看到该点，因此它是亮的。如果光的矢量在到达该点之前结束，则该光被遮挡，这意味着该点已被阴影化。

		}

		收集阴影：用场景的深度信息和光源的深度信息进行比较，最后把阴影信息输出到渲染到屏幕空间阴影贴图（_ShadowMapTexture  分辨率大小在playerSetting/Quality里设置 分高中低几挡）。
		{	
			Unity通过渲染一个覆盖整个视图的四边形来创建这些纹理。它为此过程使用Hidden / Internal-ScreenSpaceShadows着色器。
			每个片段都从场景和灯光的深度纹理中采样，进行比较，并将最终阴影值渲染到屏幕空间阴影贴图。光纹理像素设置为1，阴影纹理像素设置为0。这时，Unity还可以执行过滤以创建柔和阴影。
		}

		*******************（场景的深度纹理） + （灯光的深度纹理）==》 屏幕空间阴影贴图


		采样阴影贴图：指的是屏幕空间阴影贴图
		{
			最后，Unity完成渲染阴影。现在，场景已正常渲染，只进行了一次更改。颜色乘以存储在其阴影贴图中的值。这样可以消除应遮挡的光线
			在渲染到屏幕空间阴影贴图时，Unity会从正确的级联中进行采样。通过查找阴影纹素大小的突然变化，你可以找到一个级联结束而另一个级联开始的位置。
		}


		阴影尖刺（Shadow Acne）
		{
			当我们使用低质量的硬阴影时，我们会看到一些阴影出现在不应该出现的地方。而且，无论质量设置如何，都可能发生这种情况。
			阴影图中的每个纹理像素代表光线照射到表面的点。但是，纹素不是单点。它们最终会覆盖更大的区域。它们与光的方向对齐，而不是与表面对齐。结果，它们最终可能会像深色碎片一样粘在，穿过和伸出表面。
			由于部分纹理像素最终从投射阴影的表面戳出来，因此该表面似乎会产生自身阴影。这被称为阴影尖刺。

			阴影尖刺的另一个来源是数值精度限制。当涉及到非常小的距离时，这些限制可能导致错误的结果。

			防止此问题的一种方法是在渲染阴影贴图时添加深度偏移。此偏差会加到从光到阴影投射表面的距离，从而将阴影推入表面

			阴影偏移是针对每个光源配置的，默认情况下设置为0.05

			低的偏移会产生阴影尖刺，但较大的偏移会带来另一个问题。当阴影物体被推离灯光时，它们的阴影也被推开。
			结果，阴影将无法与对象完美对齐。使用较小的偏移时，效果还不错。但是太大的偏移会使阴影看起来与投射它们的对象断开连接。这种效果被称为peter panning。
		}

		抗锯齿：屏幕空间阴影贴图使用覆盖整个视图的单个四边形进行渲染。结果，没有三角形边缘，因此MSAA不会影响屏幕空间阴影贴图
		{

			你是否在质量设置中启用了抗锯齿功能？如果有，那么你可能已经发现了阴影贴图的另一个问题。它们没有与标准的抗锯齿方法混合使用

			在质量设置中启用抗锯齿功能后，Unity将使用多重采样抗锯齿功能MSAA。通过沿三角形边缘进行一些超级采样，可以消除这些边缘上的混叠。

			重要的是，当Unity渲染屏幕空间阴影贴图时，它使用覆盖整个视图的单个四边形进行渲染。结果，没有三角形边缘，因此MSAA不会影响屏幕空间阴影贴图。

			MSAA确实适用于最终图像，但是阴影值直接从屏幕空间阴影贴图中获取。
			当靠近较暗表面的较亮表面被阴影覆盖时，这变得非常明显。亮和暗几何之间的边缘被消除锯齿，而阴影边缘则没有。


			依靠图像后处理的抗锯齿方法（例如FXAA）不会出现此问题，因为它们是在渲染整个场景之后应用的。
			
			这是否意味着我无法将MSAA与定向阴影结合使用？
				可以，但是你会遇到上述问题。在某些情况下，它可能不会引起注意。例如，当所有表面颜色大致相同时，失真将很微小。当然你仍然会获得锯齿状的阴影边缘。
		}
	}

	2 投射阴影： ShadowCaster的pass
	{
		必须向它的着色器添加一个pass，其照明模式设置为ShadowCaster。因为我们只对深度值感兴趣，所以它将比其他pass操作简单得多。

		顶点程序像往常一样将位置从对象空间转换为剪切空间，并且不执行其他任何操作。片段程序实际上不需要执行任何操作，因此只需返回零即可。GPU会为我们记录深度值。
		#include "UnityCG.cginc"

		struct VertexData {
			float4 position : POSITION;
		};

		float4 MyShadowVertexProgram (VertexData v) : SV_POSITION {
			return mul(UNITY_MATRIX_MVP, v.position);
		}

		half4 MyShadowFragmentProgram () : SV_TARGET {
			return 0; //不返回颜色值，GPU会为我们记录深度值 
		}



		UnityClipSpaceShadowCasterPos(v.position.xyz, v.normal); //支持法线bias设置
		UnityApplyLinearShadowBias(position); //支持bias设置


	}

	3 接受阴影
	{
		当主定向光投射阴影时，Unity将查找启用了SHADOWS_SCREEN关键字的着色器变体。因此，我们必须创建基本pass的两个变体，一个带有此关键字，另一个不带有此关键字
			Tags {
				"LightMode" = "ForwardBase"
			}

			CGPROGRAM

			#pragma target 3.0

			#pragma multi_compile _ SHADOWS_SCREEN
			ENDCG



		采样阴影：对屏幕空间阴影贴图进行采样，需要知道屏幕空间纹理坐标
		{
			需要知道屏幕空间纹理坐标。像其他纹理坐标一样，我们会将它们从顶点着色器传递到片段着色器。因此，当支持阴影时，我们需要使用附加的插值器。我们将从传递齐次剪切空间位置开始，所以我们需要一个float4
			struct Interpolators {
				…

				#if defined(SHADOWS_SCREEN)
					float4 shadowCoordinates : TEXCOORD5;
				#endif

				#if defined(VERTEXLIGHT_ON)
					float3 vertexLightColor : TEXCOORD6;
				#endif
			};

			Interpolators MyVertexProgram (VertexData v) {
			…

			#if defined(SHADOWS_SCREEN)
				i.shadowCoordinates = i.position;
			#endif

			ComputeVertexLightColor(i);
			return i;
			}



			可以通过_ShadowMapTexture访问屏幕空间阴影。适当时在AutoLight中定义。简单的方法是仅使用片段的剪切空间XY坐标对该纹理进行采样。
			具有剪辑空间坐标而不是屏幕空间坐标。我们确实会得到阴影，但最终会压缩到屏幕中心的一个很小区域。必须拉伸它们以覆盖整个窗口
			UnityLight CreateLight (Interpolators i) {
			…

			#if defined(SHADOWS_SCREEN)
				float attenuation = tex2D(_ShadowMapTexture, i.shadowCoordinates.xy); //表现不对
			#else
				UNITY_LIGHT_ATTENUATION(attenuation, 0, i.worldPos);
			#endif

			…

			}

			在剪辑空间中，所有可见的XY坐标都在-1~1范围内，而屏幕空间的范围是0~1。解决这个问题的第一步是将XY减半。
			接下来，我们还必须偏移坐标，以使它们在屏幕的左下角为零。因为我们正在处理透视变换，所以必须偏移坐标，多少则取决于坐标距离。这时，在减半之前，偏移量等于第四齐次坐标。
			#if defined(SHADOWS_SCREEN)
				i.shadowCoordinates.xy = (i.position.xy + i.position.w) * 0.5;
				i.shadowCoordinates.zw = i.position.zw;
			#endif

			投影仍然不正确，因为我们使用的是齐次坐标。必须通过将X和Y除以W来转换为屏幕空间坐标
			i.shadowCoordinates.xy = (i.position.xy + i.position.w) * 0.5 / i.position.w; //必须放到片段着色器里除法

			结果会失真。阴影被拉伸和弯曲。这是因为我们在插值之前进行了除法。这是不正确的，应在除法之前分别对坐标进行插补。因此，我们必须将分割移动到片段着色器。

			Interpolators MyVertexProgram (VertexData v) {
				…

				#if defined(SHADOWS_SCREEN)
					i.shadowCoordinates.xy =
						(i.position.xy + i.position.w) * 0.5; // 不使用除法;
					i.shadowCoordinates.zw = i.position.zw;
				#endif
				
				…
			}

			UnityLight CreateLight (Interpolators i) {
				…

				#if defined(SHADOWS_SCREEN)
					float attenuation = tex2D(
						_ShadowMapTexture,
						i.shadowCoordinates.xy / i.shadowCoordinates.w // 使用除法
					);
				#else
					UNITY_LIGHT_ATTENUATION(attenuation, 0, i.worldPos);
				#endif

				…
			}
		}
	}

	4 使用Unity库函数
	{
		Unity的包含文件提供了功能和宏的集合，以帮助我们对阴影进行采样。他们兼顾API差异和平台限制。例如，我们可以使用UnityCG的ComputeScreenPos函数

		//从裁剪空间到屏幕空间
		#if defined(SHADOWS_SCREEN)
			i.shadowCoordinates = ComputeScreenPos(i.position);
		#endif


		AutoLight包含文件定义了三个有用的宏。它们是SHADOW_COORDS，TRANSFER_SHADOW和SHADOW_ATTENUATION。启用阴影后，这些宏将执行与刚才相同的工作。没有阴影时，它们什么也不做。

		1 SHADOW_COORDS在需要时定义阴影坐标的插值器
		struct Interpolators {
			float4 pos : SV_POSITION;
			float4 uv : TEXCOORD0;
			float3 normal : TEXCOORD1;

			#if defined(BINORMAL_PER_FRAGMENT)
				float4 tangent : TEXCOORD2;
			#else
				float3 tangent : TEXCOORD2;
				float3 binormal : TEXCOORD3;
			#endif

			float3 worldPos : TEXCOORD4;

			SHADOW_COORDS(5) //SHADOW_COORDS在需要时定义阴影坐标的插值器

			#if defined(VERTEXLIGHT_ON)
				float3 vertexLightColor : TEXCOORD6;
			#endif
		};

		2 TRANSFER_SHADOW将这些坐标填充到顶点程序中
		Interpolators MyVertexProgram (VertexData v) {
			Interpolators i;
			i.pos = UnityObjectToClipPos(v.vertex);
			i.worldPos = mul(unity_ObjectToWorld, v.vertex);
			i.normal = UnityObjectToWorldNormal(v.normal);

			#if defined(BINORMAL_PER_FRAGMENT)
				i.tangent = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
			#else
				i.tangent = UnityObjectToWorldDir(v.tangent.xyz);
				i.binormal = CreateBinormal(i.normal, i.tangent, v.tangent.w);
			#endif

			i.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
			i.uv.zw = TRANSFORM_TEX(v.uv, _DetailTex);

			TRANSFER_SHADOW(i);  //TRANSFER_SHADOW将这些坐标填充到顶点程序中, 片段着色器计算阴影使用

			ComputeVertexLightColor(i);
			return i;
		}

		3 SHADOW_ATTENUATION使用坐标在片段程序中对阴影贴图进行采样。

		UnityLight CreateLight (Interpolators i) {
			…

			#if defined(SHADOWS_SCREEN)
				float attenuation = SHADOW_ATTENUATION(i);
			#else
				UNITY_LIGHT_ATTENUATION(attenuation, 0, i.worldPos);
			#endif

			…
		}

		实际上，UNITY_LIGHT_ATTENUATION宏已经使用SHADOW_ATTENUATION。这就是我们之前遇到该编译器错误的原因。因此，仅使用该宏就足够了。唯一的变化是我们必须使用插值器作为第二个参数，而之前我们只是使用零。

		UnityLight CreateLight (Interpolators i) {
			UnityLight light;

			#if defined(POINT) || defined(POINT_COOKIE) || defined(SPOT)
				light.dir = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);
			#else
				light.dir = _WorldSpaceLightPos0.xyz;
			#endif

			//可以通过_ShadowMapTexture访问屏幕空间阴影  float attenuation = tex2D(_ShadowMapTexture, i.shadowCoordinates.xy);
			UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos); // 使用封装方法，处理各种不同光源

			light.color = _LightColor0.rgb * attenuation;
			light.ndotl = DotClamped(i.normal, light.dir);
			return light;
		}



		重写我们的代码以使用这些宏后，但得到了新的编译错误。发生这种情况是因为Unity的宏对顶点数据和插值器结构进行了假设。
		首先，假设顶点位置命名为vertex，而我们将其命名为position。其次，假定内插器位置命名为pos，但我们将其命名为position。
		我们老实一点，也采用这些名称。不管如何，它们仅在少数几个地方使用，因此我们不必进行太多更改。
		struct VertexData {
			float4 vertex : POSITION;
			…
		};

		struct Interpolators {
			float4 pos : SV_POSITION;
			…
		};

		…

		Interpolators MyVertexProgram (VertexData v) {
			Interpolators i;
			i.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			i.worldPos = mul(unity_ObjectToWorld, v.vertex);
			…
		}
	}

	5 聚光灯阴影：着色器代码跟定向光一样
	{
		查看帧调试器时，你会发现Unity对聚光灯阴影的工作较少。没有单独的深度pass，也没有屏幕空间阴影传递。仅渲染阴影贴图。

		阴影贴图与定向光的作用相同。它们是深度图，是从灯光的角度渲染的。但是，定向光和聚光灯之间存在很大差异。
		聚光灯具有实际位置，并且光线不平行。因此，聚光灯的摄像机具有透视图。结果，这些灯不能支持阴影级联
	}

	6 点光源阴影：使用深度立方体贴图
	{
		。显然，没有足够的平台支持它们。因此，我们不能依靠“My Shadows”中片段的深度值。取而代之的是，我们必须输出片段的距离作为片段程序的结果。

		投射阴影：
		{
			#if defined(SHADOWS_CUBE)
			//点光
			//渲染点光源阴影贴图时，Unity将使用定义的SHADOWS_CUBE关键字查找阴影投射器变体
			struct Interpolators {
				float4 position : SV_POSITION;
				float3 lightVec : TEXCOORD0;
			};

			Interpolators MyShadowVertexProgram (VertexData v) {
				Interpolators i;
				i.position = UnityObjectToClipPos(v.position);
				i.lightVec =
					mul(unity_ObjectToWorld, v.position).xyz - _LightPositionRange.xyz;
				return i;
			}

			float4 MyShadowFragmentProgram (Interpolators i) : SV_TARGET {
				float depth = length(i.lightVec) + unity_LightShadowBias.x;
				depth *= _LightPositionRange.w;
				return UnityEncodeCubeShadowDepth(depth); 
			}
		#else
			//定向光和聚光
			float4 MyShadowVertexProgram (VertexData v) : SV_POSITION {
				float4 position =
					UnityClipSpaceShadowCasterPos(v.position.xyz, v.normal); //支持法线bias设置
				return UnityApplyLinearShadowBias(position); //支持bias设置
			}

			half4 MyShadowFragmentProgram () : SV_TARGET {
				return 0;
			}
		#endif
		}


		与聚光灯阴影一样，阴影贴图对硬阴影采样一次，对软阴影采样四次。最大的区别是Unity不支持对阴影立方体贴图进行过滤。
		结果，阴影的边缘更加粗糙。因此，点光阴影既昂贵，锯齿又强。

	}
}


8 ***************反射
{
	1 环境贴图
	{
		当前，我们的着色器通过组合表面上的环境反射，漫反射和镜面反射为片段着色。至少在表面比较粗糙的情况下，会产生看似逼真的图像。
		但是，有光泽的表面看起来就不太正确。
		闪亮的表面就像镜子一样，尤其是金属的时候。完美的镜子可以反射所有光线。
		这意味着根本没有漫反射。只有镜面反射。
		因此，通过将“Metallic ”设置为1，将“Smoothness”设置为0.95，将我们的材质变成一面镜子。使其成为为纯白色。
		但结果表面几乎是全黑的，即使它自己的颜色设置是白色。我们只看到一个小的亮点，把光源直接反射给了我们。所有其他光都沿不同方向反射回去。如果将平滑度增加到1，则高光也会消失
		这看起来根本不像是真正的镜子。镜子不是黑色的，它们可以反射事物！在这种情况下，它应该反映出天空盒，显示蓝天和灰色地面才对。


		间接镜面光照
		{
			我们的球体变了黑色，因为我们只包含了方向光。为了反映环境，我们还必须包括间接光。具体而言，间接光用于镜面反射

			表面越光滑，菲涅耳反射越强。使用高平滑度值时，红色环变得非常明显。
			因为反射来自于间接光，所以它与直接光源无关。结果，反射也独立计算该光源的阴影。因此，菲涅耳反射在球的其他阴影边缘变得非常明显。
		}

		采样环境： 天空盒立方体贴图unity_SpecCube0  使用宏UNITY_SAMPLE_TEXCUBE进行采样
		{
			为了反映实际环境，我们必须对天空盒立方体贴图进行采样。它在UnityShaderVariables中定义为unity_SpecCube0。此变量的类型取决于目标平台，该目标平台在HSLSupport中确定。
			使用3D向量对立方体贴图进行采样，该向量指定了采样方向。我们可以为此使用UNITY_SAMPLE_TEXCUBE宏，它会为我们处理类型差异。
			#if defined(FORWARD_BASE_PASS)
				indirectLight.diffuse += max(0, ShadeSH9(float4(i.normal, 1)));
				float3 envSample = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, i.normal); //还需要从HDR转成RGB
				indirectLight.specular = envSample;
			#endif

			天空盒出现了，但是太亮了。这是因为立方体贴图包含HDR（高动态范围）颜色，这使其可以包含大于1的亮度值。我们必须将样本从HDR格式转换为RGB。
			#if defined(FORWARD_BASE_PASS)
				indirectLight.diffuse += max(0, ShadeSH9(float4(i.normal, 1)));
				float4 envSample = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, i.normal);
				indirectLight.specular = DecodeHDR(envSample, unity_SpecCube0_HDR); //间接光镜面光照
			#endif

			//我们得到了正确的颜色，但是还没有看到实际的反射。因为我们使用球体的法线来采样环境，所以投影不取决于视图方向。这就像在一个球体画了环境一样
			
		}

		追踪反射
		{
			我们得到了正确的颜色，但是还没有看到实际的反射。因为我们使用球体的法线来采样环境，所以投影不取决于视图方向。这就像在一个球体画了环境一样
			为了产生实际的反射，我们必须采取从照相机到表面的方向，并使用表面法线对其进行反射。可以为此使用反射功能
			UnityIndirect CreateIndirectLight (Interpolators i, float3 viewDir) {
				…
				
				#if defined(FORWARD_BASE_PASS)
					indirectLight.diffuse += max(0, ShadeSH9(float4(i.normal, 1)));
					float3 reflectionDir = reflect(-viewDir, i.normal); //反射方向
					float4 envSample = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, reflectionDir);
					indirectLight.specular = DecodeHDR(envSample, unity_SpecCube0_HDR);
				#endif

				return indirectLight;
			}

			…

			float4 MyFragmentProgram (Interpolators i) : SV_TARGET {
				…

				return UNITY_BRDF_PBS(
					albedo, specularTint,
					oneMinusReflectivity, _Smoothness,
					i.normal, viewDir,
					CreateLight(i), CreateIndirectLight(i, viewDir) //传入视觉方向（摄像机位置-片段位置）
				);
			}
		}


		使用反射探针：反射实际场景的几何形状，默认形式为烘焙，该模式只对静态物体生效
		{
			要查看建筑物的反射，必须首先捕获它。这是通过反射探针完成的，可以通过GameObject/ Light / Reflection Probe添加。创建一个并将其放置在与我们的球体相同的位置。

			反射探针通过渲染立方体贴图来捕获环境。这意味着它将渲染场景六次，每个立方体的面一次。
			默认情况下，其类型设置为烘焙。在这种模式下，立方体贴图由编辑器生成并包含在构建中。这些贴图仅包含静态几何体。因此，我们的建筑物在呈现到立方体贴图之前必须是静态的。

			实时反射探针比较昂贵
			尽管实时探针最灵活，但是如果频繁更新，它们也是最昂贵的。同样，实时探针不会在编辑模式下更新，而烘焙的探针或静态几何图形在编辑时会更新。这里，我们使用烘焙好的探针并使我们的建筑物保持静态。


			对象实际上不需要完全是静态的。你可以将它们标记为静态，以用于各种子系统。
			在这种情况下，相关设置为“Reflection Probe Static”。启用后，将对象渲染到烘焙的探针。你可以在运行时移动它们，但是它们的反射会保持冻结。
		}
	}


	2 不完美的反射：模糊的反射，使用不同等级的mipMap
	{
		只有完全光滑的表面才能产生完全清晰的反射。表面变得越粗糙，其反射越扩散。钝镜会产生模糊的反射

		纹理可以具有mipmap，它是原始图像的降采样版本。以全尺寸查看时，较高的Mipmap会产生模糊的图像。
		这些将是块状图像，但是Unity使用不同的算法来生成环境图的mipmap。这些代理体积的贴图代表了从清晰到模糊的良好发展。

		粗糙的镜子: 模糊的反射
		{
			使用UNITY_SAMPLE_TEXCUBE_LOD宏在特定的mipmap级别对立方体贴图进行采样，材质越粗糙，我们应该使用的mipmap级别越高

			当粗糙度从0变为1时，我们必须按使用的mipmap范围对其进行缩放。Unity使用UNITY_SPECCUBE_LOD_STEPS宏来确定此范围，

			float roughness = 1 - _Smoothness;
			float4 envSample = UNITY_SAMPLE_TEXCUBE_LOD(
				unity_SpecCube0, reflectionDir, roughness * UNITY_SPECCUBE_LOD_STEPS
			);


			实际上，粗糙度与mipmap级别之间的关系不是线性的。Unity使用转换公式
			1.7r−0.7r的平方，r为原始的粗糙度
				float roughness = 1 - _Smoothness;
				roughness *= 1.7 - 0.7 * roughness;
				float4 envSample = UNITY_SAMPLE_TEXCUBE_LOD(
					unity_SpecCube0, reflectionDir, roughness * UNITY_SPECCUBE_LOD_STEPS
				);


			使用Unity封装好的方法 ：天空盒立方体贴图==》unity_SpecCube0, 天空盒立方体贴图HDR数据==》unity_SpecCube0_HDR
			{
				UnityStandardBRDF包含文件包含Unity_GlossyEnvironment函数。它包含所有用于转换粗糙度，对立方体贴图采样以及从HDR转换的代码。因此，让我们使用该函数代替我们自己的代码。
					indirectLight.diffuse += max(0, ShadeSH9(float4(i.normal, 1)));
					float3 reflectionDir = reflect(-viewDir, i.normal);

			//		float roughness = 1 - _Smoothness;
			//		roughness *= 1.7 - 0.7 * roughness;
			//		float4 envSample = UNITY_SAMPLE_TEXCUBE_LOD(
			//			unity_SpecCube0, reflectionDir, roughness * UNITY_SPECCUBE_LOD_STEPS
			//		);
			//		indirectLight.specular = DecodeHDR(envSample, unity_SpecCube0_HDR);

					Unity_GlossyEnvironmentData envData;
					envData.roughness = 1 - _Smoothness;
					envData.reflUVW = reflectionDir;
					indirectLight.specular = Unity_GlossyEnvironment(
						UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, envData
					);
			}

		}

		金属与非金属
		{
			金属和非金属表面都可以产生清晰的反射，只是看起来有所不同。镜面反射在发亮的介电材质上看起来可能很好，但是它们并不能控制外观。仍然有大量的漫反射可见
			回想一下，金属会使其镜面反射着色，而非金属则不会。对于镜面高光和镜面环境反射都是如此
		}


		间接反射与表面的直接照明无关，阴影区域最为明显看的特比明显
		在非金属的情况下，这只会导致视觉上更亮的表面。你仍然可以看到直接光线投射的阴影。
		相同的规则适用于金属，但间接反射占主导地位。因此，随着光亮度的增加，直接光的阴影消失。完美的镜子上没有阴影。
	}

	3 盒投影：有限的空间里反射，反射探针可以设置盒投影区域 Box Projection Bounds
	{
		当一片环境无限远时，确定反射率，我们无需考虑视角位置。但是，当大多数环境都在附近时，我们就需要注意。
		假设我们在一个空的房间中间有一个反射探针。它的环境图包含此房间的墙壁，地板和天花板。如果立方体贴图和房间对齐，则立方体贴图的每个面都与墙壁，地板或天花板之一精确对应。


		反射探针盒：需要初始反射方向，来从中采样的位置，立方体贴图位置以及盒边界
		{
			反射探针的大小和原点确定了相对于其位置的世界空间中的立方区域。它始终与轴对齐，这意味着它将忽略所有旋转。它也忽略缩放。
			该区域用于两个目的。首先，Unity使用这些区域来决定在渲染对象时使用哪个探针。其次，该区域用于盒投影，这就是我们要做的。

			要计算盒投影，需要初始反射方向，来从中采样的位置，立方体贴图位置以及盒边界。为此，在CreateIndirectLight上方的着色器中添加一个函数
			float3 BoxProjection (
				float3 direction, float3 position,
				float4 cubemapPosition, float3 boxMin, float3 boxMax
			) {
				#if UNITY_SPECCUBE_BOX_PROJECTION
					UNITY_BRANCH
					if (cubemapPosition.w > 0) {
						float3 factors =
							((direction > 0 ? boxMax : boxMin) - position) / direction;
						float scalar = min(min(factors.x, factors.y), factors.z);
						direction = direction * scalar + (position - cubemapPosition);
					}
				#endif
				return direction;
			}

			float3 reflectionDir = reflect(-viewDir, i.normal);
			Unity_GlossyEnvironmentData envData;
			envData.roughness = 1 - _Smoothness;

			//BoxProjection 自己封装的函数，计算盒空间反射的，近处物体的反射
			//unity_SpecCube0_ProbePosition 第一个反射探针的位置
			envData.reflUVW = BoxProjection(
				reflectionDir, i.worldPos,
				unity_SpecCube0_ProbePosition,
				unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax
			);


			事实证明，单个盒投影探头产生的反射与九个独立探头的反射非常相似！因此，盒投影是一个非常方便的技巧，尽管它并不完美。
		}

	}

	4 混合反射探针：为了较好的反射融合（房间内到房间外）
	{
		为了获得建筑物内部和外部的良好反射，我们必须使用多个反射探针

		插值探针
		{
			Unity为着色器提供了两个反射探针的数据，因此我们可以在它们之间进行混合。第二个探针是unity_SpecCube1。我们可以对两个环境图都进行采样并根据哪个更占优势进行插值。
			Unity为我们计算此值，并将插值器存储在unity_SpecCube0_BoxMin的第四个坐标中。如果仅使用第一个探针，则将其设置为1；如果存在混合，则将其设置为较低的值。
		}

		重叠探针盒
		{
			为了使混合有效，多个探针的边界必须重叠。因此，调整第二个盒，使其延伸到建筑物中。重叠区域中的球应获得混合反射。网格渲染器组件的检查器还显示了正在使用的探针及其权重。
			如果过渡不够顺畅，你可以在其他两个之间添加第三个探针。该探针的框与其他两个框重叠。因此，在向外移动时，首先要在内部和中间探针之间以及在中间和外部探针之间进行混合。

			还可以在探针和天空盒之间进行混合。你必须将对象的“Reflection Probes”模式从“Blend Probes”更改为“Blend Probes and Skybox”。当对象的边界框部分超出探针边界时，就会发生混合。
		}

		最后实现源码
		{
			UnityIndirect CreateIndirectLight (Interpolators i, float3 viewDir) {
				UnityIndirect indirectLight;
				indirectLight.diffuse = 0;
				indirectLight.specular = 0;

				#if defined(VERTEXLIGHT_ON)
					indirectLight.diffuse = i.vertexLightColor;
				#endif

				#if defined(FORWARD_BASE_PASS)
					indirectLight.diffuse += max(0, ShadeSH9(float4(i.normal, 1)));
					float3 reflectionDir = reflect(-viewDir, i.normal);
					Unity_GlossyEnvironmentData envData;
					envData.roughness = 1 - _Smoothness;

					//BoxProjection 自己封装的函数，计算盒空间反射的，近处物体的反射
					//unity_SpecCube0_ProbePosition 第一个反射探针的位置
					envData.reflUVW = BoxProjection(
						reflectionDir, i.worldPos,
						unity_SpecCube0_ProbePosition,
						unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax
					);

					/*
						Unity_GlossyEnvironment Unity自带函数
						{
							1 转换粗糙度，对立方体贴图采样，可以使用不同的立方体贴图Mipmap达到模糊的反射效果
							2 立方体贴图包含HDR（高动态范围）颜色，这使其可以包含大于1的亮度值，必须HDR格式转换为RGB
						}
					*/
					float3 probe0 = Unity_GlossyEnvironment(
						UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, envData
					);

					//unity_SpecCube1_ProbePosition 第二个反射探针的位置
					envData.reflUVW = BoxProjection(
						reflectionDir, i.worldPos,
						unity_SpecCube1_ProbePosition,
						unity_SpecCube1_BoxMin, unity_SpecCube1_BoxMax
					);
					#if UNITY_SPECCUBE_BLENDING
						float interpolator = unity_SpecCube0_BoxMin.w;
						UNITY_BRANCH
						if (interpolator < 0.99999) {
							float3 probe1 = Unity_GlossyEnvironment(
								UNITY_PASS_TEXCUBE_SAMPLER(unity_SpecCube1, unity_SpecCube0),
								unity_SpecCube0_HDR, envData
							);
							indirectLight.specular = lerp(probe1, probe0, interpolator);
						}
						else {
							indirectLight.specular = probe0;
						}
					#else
						indirectLight.specular = probe0;
					#endif
				#endif

				return indirectLight;
			}
		}

		
	}
}

9 10***************复合材质
{
	自定义材质检查器UI： ShaderGUI
	{
		要使用自定义GUI，必须将CustomEditor指令添加到着色器，后面跟着包含要使用的GUI类名称的字符串。
		Shader "Custom/My First Lighting Shader" {
			…
			
			CustomEditor "MyLightingShaderGUI"
		}

		ShaderGUI类可以放在命名空间中吗？
			是的。但必须在着色器中指定完全限定的类名称。 CustomEditor "MyNamespace.MyShaderGUI"


		要替换默认的检查器，我们必须重写ShaderGUI.OnGUI方法。此方法有两个参数。首先，对MaterialEditor的引用。该对象管理当前选定材质的检查器。其次，包含该材质属性的数组
			MaterialEditor editor;
			MaterialProperty[] properties;

			public override void OnGUI (
				MaterialEditor editor, MaterialProperty[] properties
			) {
				this.editor = editor;
				this.properties = properties;
				DoMain();
			}

		反照率贴图首先显示在标准着色器中。这是主要的纹理。
		它的属性位于properties数组内的某个位置。它的数组索引取决于在着色器中定义属性的顺序。
		但是按名称搜索它会更可靠。ShaderGUI包含FindProperty方法，该方法可以在给定名称和属性数组的情况下做到这一点。
		void DoMain () {
			GUILayout.Label("Main Maps", EditorStyles.boldLabel);

			MaterialProperty mainTex = FindProperty("_MainTex", properties);
			//我们已经在着色器中将主要纹理命名为Albedo。所以我们只能使用该名称，可以通过属性访问该名称。
			GUIContent albedoLabel = new GUIContent(mainTex.displayName);
		}
		

	}

	混合金属和非金属: 金属度贴图 R通道，平滑度贴图 A通道
	{
		因为我们的着色器使用统一的值来确定某种东西的金属性，所以它不能在材质的整个表面上变化。这使我们无法创建实际上代表不同材质混合的复杂材质


		金属贴图:贴图定义了每个纹理像素的金属值,只使用了R通道
		{

			贴图定义了每个纹理像素的金属值，而不是一次定义整个材质。这是一张灰度图，将电路标记为金属，其余标记为非金属。染色的金属较暗，因为其顶部为半透明的脏层。

			//创建一个函数，以插值器作为参数来检索片段的金属值。它只是对金属贴图进行采样，然后将其乘以统一的金属值。Unity使用贴图的R通道
			float GetMetallic (Interpolators i) {
				return tex2D(_MetallicMap, i.uv.xy).r * _Metallic;
			}
		}


		平滑度贴图: 数据存到了a通道，Unity的标准着色器希望将平滑度存储在Alpha通道中
		{
			像金属贴图一样，也可以通过贴图定义平滑度。这是一张电路的灰度平滑纹理。金属部分最光滑。其余部分相当粗糙。污渍比木板光滑，因此那里的纹理更浅。

			实际上，可以实现，金属贴图和平滑贴图在同一纹理中结合在一起

			float GetSmoothness (Interpolators i) {
			#if defined(_METALLIC_MAP)
				return tex2D(_MetallicMap, i.uv.xy).a; //平滑度存在A通道
			#else
				return _Smoothness;
			#endif
		}
		}
	}

	自发光表面：自发光贴图
	{
		HDR自发光
		{
			标准着色器不使用常规颜色进行自发光。相反，它支持高动态范围的颜色。这意味着该颜色的RGB分量可以高于1。这样，你就可以表示非常明亮的光

			要有意义的使用HDR颜色，必须执行色调映射。这意味着你将一种颜色范围转换为另一种颜色范围。我们将在以后的教程中研究色调映射。HDR颜色通常也用于创建光晕效果。

			由于颜色属性是浮点向量，因此我们不仅限于0–1范围内的值。但是，标准颜色挂件在设计时考虑了此限制。
			幸运的是，MaterialEditor包含TexturePropertyWithHDRColor方法，该方法专门用于纹理以及HDR颜色属性。它需要两个附加参数。
			首先，HDR范围的配置选项。其次，是否应该显示Alpha通道，这是我们不想要的。

				static ColorPickerHDRConfig emissionConfig = new ColorPickerHDRConfig(0f, 99f, 1f / 99f, 3f);
				void DoEmission () {
					…
					editor.TexturePropertyWithHDRColor(
						MakeLabel("Emission (RGB)"), map, FindProperty("_Emission"),
						emissionConfig, false
					);
					…
				}
		}
	}

	遮挡区域： 使用遮挡贴图，使用G通道
	{
		//创建一个函数以对贴图进行采样（如果存在）。如果不存在，则不应调制光，结果保持为1
		//当遮挡强度为零时，贴图完全不会影响光线，因此，该函数需要返回1。当处于全强度时，结果恰好是贴图中的结果。我们可以通过基于滑块在1和贴图之间进行插值来实现。
		float GetOcclusion (Interpolators i) {
			#if defined(_OCCLUSION_MAP)
				return lerp(1, tex2D(_OcclusionMap, i.uv.xy).g, _OcclusionStrength);
			#else
				return 1;
			#endif
		}


		UnityLight CreateLight (Interpolators i) {
			…

			UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos);
			attenuation *= GetOcclusion(i);
			light.color = _LightColor0.rgb * attenuation;
			light.ndotl = DotClamped(i.normal, light.dir);
			return light;
		}
	}
}