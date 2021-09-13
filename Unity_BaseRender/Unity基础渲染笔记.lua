

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

11***************透明物体
{
	Cutout Rendering：alpha测试，直接丢弃像素
	{
		要创建透明材质，我们必须知道每个片段的透明度。此信息通常存储在颜色的Alpha通道中。在我们的例子中，这是主反照率纹理的Alpha通道，以及颜色色调的Alpha通道。

		float GetAlpha (Interpolators i) {
			return _Tint.a * tex2D(_MainTex, i.uv.xy).a;
		}


		Cutout: 使用Clip函数
		{
			对于不透明的材质，将渲染通过深度测试的每个片段。所有片段都是完全不透明的，并写入深度缓冲区。透明度让这里变得更复杂。
			实现透明性的最简单方法是使其保持二进制状态。片段是完全不透明的，或者是完全透明的。如果它是透明的，那么根本就不会渲染。这使得可以在某表面上切孔。

			要中止渲染片段，可以使用clip函数。如果此函数的参数为负，则片段将被丢弃

			GPU不会混合其颜色，也不会写入深度缓冲区。如果发生这种情况，我们不必担心所有其他材质特性。因此，尽早clip是最有效的方法。
			在我们的例子中，那是MyFragmentProgram函数的开始

			我们将使用alpha值来确定是否应该裁剪。由于alpha介于零和一之间，因此我们必须减去一些值使其变为负数。
			通过减去½，我们将使alpha范围的下半部分为负。这意味着将渲染alpha值至少为½的片段，而所有其他片段将被剪切掉。
			float4 MyFragmentProgram (Interpolators i) : SV_TARGET {
				float alpha = GetAlpha(i);
				clip(alpha - 0.5);

				…
			}


			不透明渲染队列为2000
			CutOut渲染队列为2450
			他们将cutout 材质放入了不透明材质的不同渲染队列中。
			不透明的东西首先被渲染，然后是cutout的东西。这样做是因为cutout更加昂贵。首先渲染不透明的对象意味着我们永远不会渲染最终在实体对象之后的剪切对象。
		}
	}


	半透明渲染：在表面的不透明部分和透明部分之间有平滑过渡效果
	{
		当想在某个物体上切一个洞时，cutout 渲染就足够了，但是当你需要半透明效果时就不行了。同样，cutout 渲染是针对每个片段的，这意味着边缘会出现锯齿。
		因为在表面的不透明部分和透明部分之间没有平滑过渡。为了解决这个问题，我们必须增加对另一种渲染模式的支持。此模式将支持半透明。Unity的标准着色器将此模式命名为Fade

		渲染队列值为3000，这是透明对象的默认值。渲染类型为“Transparent”。

		如果同时具有不透明对象和透明对象，则将同时调用Render.OpaqueGeometry和Render.TransparentGeometry方法。
		首先渲染不透明和cut off的几何体，然后渲染透明的几何体。因此，半透明对象永远不会在实体对象之后绘制。


		混合片段
		{
			当alpha为1时，渲染完全不透明的东西。在那种情况下，应该像往常一样将Blend One Zero用作基础pass，将Blend One one用作附加pass。
			但是当alpha为零时，我们呈现的内容是完全透明的。如果是这样，我们不需要改变任何事情。然后，两次pass的混合模式必须为Blend   Zero   One 。如果alpha为¼，那么我们需要Blend 0.25 0.75和Blend 0.25 One之类的东西。

			为了实现这个效果，可以使用SrcAlpha和OneMinusSrcAlpha混合关键字。
			Pass {
				Tags {
					"LightMode" = "ForwardBase"
				}
				Blend SrcAlpha OneMinusSrcAlpha

				…
			}

			Pass {
				Tags {
					"LightMode" = "ForwardAdd"
				}

				Blend SrcAlpha One
				ZWrite Off

				…
			}

			使用这些float属性代替必须可变的blend关键字。你需要将它们放在方括号内。这是旧的着色器语法，用于配置GPU。我们不需要在我们的顶点和片段程序中访问这些属性。
			Properties {
				…
				
				_SrcBlend ("_SrcBlend", Float) = 1
				_DstBlend ("_DstBlend", Float) = 0
			}

			[HideInInspector] _SrcBlend ("_SrcBlend", Float) = 1
			[HideInInspector] _DstBlend ("_DstBlend", Float) = 0

			Pass {
				Tags {
					"LightMode" = "ForwardBase"
				}
				Blend [_SrcBlend] [_DstBlend]

				…
			}

			Pass {
				Tags {
					"LightMode" = "ForwardAdd"
				}

				Blend [_SrcBlend] One
				ZWrite Off

				…
			}


			在使用Fade渲染模式时，必须禁用对深度缓冲区的写入
			Pass {
				Tags {
					"LightMode" = "ForwardBase"
				}
				Blend [_SrcBlend] [_DstBlend]
				ZWrite [_ZWrite] //Off
			}
		}
	}
}

12***************透明物体的阴影
{
	Cutout阴影
	{
		为了考虑透明度，我们需要访问阴影投射器着色器通道中的alpha值,这意味着我们需要对反照率纹理进行采样

		丢弃阴影片段
		{
			首先要处理cutout阴影。通过丢弃片段来在阴影中切出洞，就像在其他渲染过程中对Cutout渲染模式所做的那样。为此，我们需要材质的色调，反照率纹理和Alpha Cut设置
			#include "UnityCG.cginc"

			float4 _Tint;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _AlphaCutoff;


			当我们使用Cutout渲染模式时，必须对反照率纹理进行采样。需要将UV坐标传递给片段程序
			Interpolators MyShadowVertexProgram (VertexData v) {
				…

				#if SHADOWS_NEED_UV
					i.uv = TRANSFORM_TEX(v.uv, _MainTex);
				#endif
				return i;
			}

			float GetAlpha (Interpolators i) {
				float alpha = _Tint.a;
				#if SHADOWS_NEED_UV
					alpha *= tex2D(_MainTex, i.uv.xy).a;
				#endif
				return alpha;
			}


			float4 MyShadowFragmentProgram (Interpolators i) : SV_TARGET {
				float alpha = GetAlpha(i);
				#if defined(_RENDERING_CUTOUT)
					clip(alpha - _AlphaCutoff);
				#endif
				
				…
			}
		}



	}

	半透明阴影：同时支持“Fade”和“Transprant”渲染模式的阴影，技术实现有点麻烦
	{
		抖动
		{
			阴影贴图包含到阻挡光线的表面的距离。光线被阻挡了一定距离，或者没有被阻挡。因此，没有办法指定光被半透明表面部分阻挡。
			我们能做的就是将阴影表面的一部分剪掉。这也是我们为cutoff阴影所做的。
			但是，除了基于阈值进行裁剪外，我们还可以统一裁剪片段。例如，如果一个表面让一半的光通过。总而言之，生成的阴影将显示为完整阴影的一半。
			不必总是使用相同的模式。依靠alpha值，我们可以使用带有更多或更少孔的图案。
			而且，如果我们混合这些模式，则可以创建阴影密度的平滑过渡。基本上，我们仅使用两种状态来近似渐变。这种技术被称为抖动（Dither）。

			Unity包含我们可以使用的抖动模式图集。它包含4 x 4像素的16种不同图案。它以完全空的模式开始。每个连续的图案填充一个附加像素，直到填充了七个像素。然后反转，直到所有像素都被填充。
		}

		VPOS
		{
			要对我们的阴影应用抖动模式，我们需要对其进行采样。
			不能使用网格的UV坐标，因为它们在阴影空间中不一致。
			相反，我们需要使用片段的屏幕空间坐标。从光的角度渲染阴影贴图时，这会使图案与阴影贴图对齐。
		}
	}
}


13***************延迟着色：延迟渲染路径，多光源比较合适，但是使用缓冲区内存较大,移动设备上很难支持，也无法MSAA，绘制不透明物体和cutout物体，透明物体还是前向渲染
{
	切换路径
	{
		使用哪个渲染路径由项目设置的图形设置定义的。你可以通过“Edit/ Project Settings/Graphics”到达那里。渲染路径和其他一些设置分为三个层级。
		这些层级对应于不同类别的GPU。GPU越好，Unity使用的层级就越高。你可以通过“Editor /Graphics Emulation”子菜单选择编辑器使用的层级。
	}

	为什么MSAA无法在延迟模式下工作？
		延迟着色依赖于每个片段存储的数据，这是通过纹理完成的。
		这与MSAA不兼容，因为该抗锯齿技术依赖于子像素数据。尽管三角形边缘仍然可以从MSAA中受益，但延迟的数据仍会锯齿。你必须依靠一个后处理过滤器来进行抗锯齿。

	看起来deferred总共只绘制每个对象一次，而不是每个灯光一次


	与前向阴影相比，在渲染多个光源时，延迟阴影似乎更有效。
	前向渲染需要每个物体每个灯光额外增加一次pass，但延迟渲染不需要这样做。当然，两者仍然都必须渲染阴影贴图，但是延迟不必为定向阴影所需的深度纹理支付额外的费用

	延迟渲染路径是如何解决它的呢？==》 Deffered存储了一些着色器必须获取网格数据到Gbuffer中，前向着色器必须对受光对象的每个像素光重复所有这些操作
	{
		要渲染物体，着色器必须获取网格数据，将其转换为正确的空间，对其进行插值，检索和导出表面属性，并计算照明度。前向着色器必须对受光对象的每个像素光重复所有这些操作。
		附加的通道比基本通道便宜一些，因为深度缓冲区已经准备好了，它们不会被间接光打扰。但是他们仍然必须重复基本通道已经完成的大部分工作

		由于几何的属性每次都是相同的，为什么不缓存它们呢？让基本通道将它们存储在缓冲区中。然后，附加通道可以重复使用该数据，从而消除了重复工作。
		我们必须按片段存储此数据，因此我们需要一个适合显示的缓冲区，就像深度缓冲区和帧缓冲区一样。

		现在，缓冲区中提供了照明所需的所有几何数据。唯一缺少的是灯光本身。
		但这意味着我们不再需要渲染几何体。可以只渲染光就足够了。此外，基本通道只需要填充缓冲区。可以推迟所有直接照明计算，直到分别渲染它们为止。因此叫延迟着色。
	}

	更多的灯光
	{
		如果只使用一个光源，那么单个延迟将不会带来任何好处。但是当使用非常多灯光时，它就派上大用场了。只要不投射阴影，每增加一个灯光就只会增加一点点额外的工作。
		同样，当分别渲染几何图形和灯光时，可以影响对象的灯光数量没有限制。所有的灯都是像素灯，并照亮其范围内的所有物体。质量设置里“Pixel Light Count ”不再适用。
	}

	渲染灯光：使用Internal-DeferredShading着色器渲染， Unity内置的，也可以自己实现
	{
		那么灯光本身如何渲染？由于定向光源会影响所有事物，因此将使用覆盖整个视图的单个四边形对其进行渲染。
		该四边形使用Internal-DeferredShading着色器渲染。它的片段程序从缓冲区获取几何数据，并依赖UnityDeferredLibrary包含文件来配置灯光。然后，它像前向着色器一样计算照明。
	}
	
	几何缓冲区（GBuffers）
	{
		缓存数据的缺点是必须将其存储在某个位置。为此，延迟的渲染路径使用了多个渲染纹理。这些纹理称为几何缓冲区，简称G缓冲区。
		延迟着色需要四个G缓冲区。对于LDR，它们的组合大小为每像素160位，对于HDR，它们的组合大小为每像素192位。这比单个32位帧缓冲区要多得多。
		现代的台式机GPU可以解决这个问题，但是移动甚至笔记本电脑的GPU在分辨率更高时都会遇到麻烦
	}

	填充G-Buffers
	{
		Unity检测到我们的着色器具有延迟的pass，因此它包含在延迟阶段使用我们的着色器的不透明对象和剪切对象。当然，透明对象仍将在透明阶段渲染。

		4个输出,四个缓冲区, 移动设备有的也不支持多目标输出
		{
			struct FragmentOutput {
			#if defined(DEFERRED_PASS)
				float4 gBuffer0 : SV_Target0;
				float4 gBuffer1 : SV_Target1;
				float4 gBuffer2 : SV_Target2;
				float4 gBuffer3 : SV_Target3;
			#else
				float4 color : SV_Target;
			#endif
			};

			不应该是SV_TARGET吗？
				可以混合使用大写字母和小写字母作为目标语义，Unity可以全部理解。在这里，我使用的是Unity最新着色器的相同格式。


			Buffer 0 ==》 albedo和Occlusion
			{
				第一个G缓冲区用于存储漫反射反照率和表面遮挡
				它是ARGB32纹理，就像常规的帧缓冲区一样。反照率存储在RGB通道中，遮挡存储在A通道中

				output.gBuffer0.rgb = albedo;
				output.gBuffer0.a = GetOcclusion(i);
			}

			Buffer 1 ==》 specular和Smoothness
			{
				第二个G缓冲区用于在RGB通道中存储镜面颜色，在A通道中存储平滑度值
				它也是ARGB32纹理。我们知道镜面反射的色调是什么，并且可以使用GetSmoothness检索平滑度值

				output.gBuffer1.rgb = specularTint;
				output.gBuffer1.a = GetSmoothness(i);
			}

			Buffer 2 ==》 世界空间法线向量
			{
				第三个G缓冲区包含世界空间法线向量。
				它们存储在ARGB2101010纹理的RGB通道中。这意味着每个坐标使用10位存储，而不是通常的8位，这使它们更加精确。A通道只有2位-因此总数又是32位，但它未使用，因此我们将其设置为1

				output.gBuffer2 = float4(i.normal * 0.5 + 0.5, 1); //存储的范围为0~1
			}

			Buffer 3 ==》 自发光
			{
				添加到此缓冲区的第一个光是自发光。

				最终的G缓冲区用于累积场景的光照。其格式取决于相机是设置为LDR还是HDR。
				就LDR而言，它是ARGB2101010纹理，就像法线的缓冲区一样。启用HDR时，格式为ARGBHalf，每个通道存储一个16位浮点值，总共64位。
				因此，HDR版本是其他缓冲区的两倍。仅使用RGB通道，因此可以将A通道再次设置为1

				我们使用ARGBHalf的原因是大多数GPU都使用四个字节的块。大多数纹理是每个像素32位，相当于一个块。64位需要两个块，因此也可以使用。
				但是48位对应于1.5个块。这会导致未对齐，可以通过将两个块用于48位来避免。这导致每个像素填充16位，又与ARGBHalf相同了。
			}
		}


		HDR和LDR
		{
			我们的着色器在正向和延迟模式下都产生相同的结果，至少在使用HDR摄像机时是这样。在LDR模式下看起来也很不对劲
			发生这种情况是因为Unity期望对LDR数据进行对数编码，如前所述。因此，对于自发光和环境影响，我们也必须使用这种编码
				#if defined(DEFERRED_PASS)
					#if !defined(UNITY_HDR_ON)
						color.rgb = exp2(-color.rgb);
					#endif
					…
				#else
					output.color = color;
				#endif
		}
	}


	延迟反射：延迟路径下渲染支持反射
	{
		逐像素探针
		{
			延迟模式的不同之处在于，不会针对每个对象混合探针。相反，它们是按像素混合的
		}
	}

}

15***************延迟光照：将自己渲染这些灯光
{
	场景中的所有对象都使用我们自己的着色器渲染到G缓冲区。但是灯光是使用Unity的默认延迟着色器渲染的，该着色器名为Hidden Internal-DefferedShader。
	你可以通过“EditProject Settings / Graphics”进入图形设置，然后将“Deferredshader”模式切换到“Custom shader”，以验证这一点。


	1 灯光着色器
	{
		第二个Pass
		{
			切换到我们的着色器后，Unity报错说它没有足够的通道数量。显然，它需要第二个pass。我们只复制已经拥有的pass，看看会发生什么。
			现在，Unity接受我们的着色器，并使用它来渲染定向光。结果，一切都变黑了。唯一的例外是天空。把模板缓冲区用作遮罩以避免在此处进行渲染，因为定向光不会影响背景

			但是为什么要使用第二个pass呢？
				请记住，禁用HDR后，灯光数据将会进行对数编码。最后的pass需要转换此编码。那就是第二个pass的目的。因此，如果你为相机禁用了HDR，那么我们着色器的第二个pass也要被用一次
		}

		避开天空
		{
			在LDR模式下渲染时，你可能还会看到天空也变黑了。这可以在场景视图或游戏视图中发生。如果天空变黑，则转换过程将无法正确使用模板缓冲区作为遮罩。
			要解决此问题，请显式配置第二个Pass的模板设置，仅在处理不属于背景的片段时才应该渲染。通过_StencilNonBackground提供适当的模板值。

			Pass {
				Cull Off
				ZTest Always
				ZWrite Off

				Stencil {
					Ref [_StencilNonBackground]
					ReadMask [_StencilNonBackground]
					CompBack Equal
					CompFront Equal
				}
				
				…
			}
		}

		转换颜色
		{
			为了使第二个pass工作正常，必须转换灯光缓冲区中的数据。像我们的雾着色器一样，使用UV坐标绘制全屏四边形，可用于对缓冲区进行采样。
		}
	}

	2 方向光
	{
		第一个pass负责渲染灯光,因此它会相当复杂。让我们为其创建一个包含文件，名为MyDeferredShading.cginc

		我们需要所有可能的灯光配置的着色器变体。multi_compile_lightpass编译器指令创建我们需要的所有关键字组合。唯一的例外是HDR模式。为此，我们必须添加一个单独的多编译指令。

		G-Buffer UV 坐标
		{
			我们需要UV坐标才能从G缓冲区采样。不幸的是，Unity不提供具有方便的纹理坐标的灯光pass。相反，必须从剪辑空间位置间接获取它们
			以使用在UnityCG中定义的ComputeScreenPos，该函数产生齐次坐标，就像剪辑空间坐标一样，因此需要使用float4来存储它们

			struct Interpolators {
				float4 pos : SV_POSITION;
				float4 uv : TEXCOORD0;
			};

			Interpolators VertexProgram (VertexData v) {
				Interpolators i;
				i.pos = UnityObjectToClipPos(v.vertex);
				i.uv = ComputeScreenPos(i.pos);
				return i;
			}

			//在片段程序中，我们可以计算最终的2D坐标
			float4 FragmentProgram (Interpolators i) : SV_Target {
				float2 uv = i.uv.xy / i.uv.w;

				return 0;
			}
		}


		世界坐标
		{
			必须找出片段与相机的距离。这个实现过程是通过从相机发射穿过每个片段到远平面的射线，然后按片段的深度值缩放这些光线
			在定向光的情况下，将四边形的四个顶点的光线作为法线矢量提供。因此，我们可以将它们传递给顶点程序并进行插值。

			struct VertexData {
				float4 vertex : POSITION;
				float3 normal : NORMAL; //***
			};

			struct Interpolators {
				float4 pos : SV_POSITION;
			    float4 uv : TEXCOORD0;
			    float3 ray : TEXCOORD1; //***
			};

			Interpolators VertexProgram (VertexData v) {
				Interpolators i;
				i.pos = UnityObjectToClipPos(v.vertex);
				i.uv = ComputeScreenPos(i.pos);
				i.ray = v.normal;
				return i;
			}

			可以通过采样_CameraDepthTexture纹理并将其线性化来在片段程序中找到深度值，
			float4 FragmentProgram (Interpolators i) : SV_Target {
				float2 uv = i.uv.xy / i.uv.w;
				
				float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv); //****
				depth = Linear01Depth(depth);

				return 0;
			}

			但是，最大的不同是我们将到达远平面的光线提供给了雾的着色器。这时，我们会获得到达近平面的射线。需要按比例缩放它们，以便获得到达远平面的射线。通过缩放射线使其Z坐标变为1并将其乘以远平面距离来完成。
			float3 rayToFarPlane = i.ray * _ProjectionParams.z / i.ray.z;


			按深度值缩放此射线可得到一个位置。因为所提供的光线在视图空间中定义的，所以得到的空间也是相机的局部空间。因此，我们现在也以片段在视图空间中的位置作为终点。
			float3 viewPos = rayToFarPlane * depth; //视觉空间下位置

			从相机空间到世界空间的转换是通过在ShaderVariables中定义的unity_CameraToWorld矩阵完成的。

			float3 worldPos = mul(unity_CameraToWorld, float4(viewPos, 1)).xyz; //得到世界空间下位置

			完整的代码
			float4 FragmentProgram (Interpolators i) : SV_Target {
				float2 uv = i.uv.xy / i.uv.w;
				
				float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv); //****
				depth = Linear01Depth(depth);
				float3 rayToFarPlane = i.ray * _ProjectionParams.z / i.ray.z;
				float3 viewPos = rayToFarPlane * depth; //视觉空间下位置
				float3 worldPos = mul(unity_CameraToWorld, float4(viewPos, 1)).xyz; //得到世界空间下位置

				return 0;
			}

		}

		读取 G-Buffer数据
		{
			我们需要访问G缓冲区以检索表面属性。通过三个_CameraGBufferTexture变量可以使用这些缓冲区。
			sampler2D _CameraGBufferTexture0;
			sampler2D _CameraGBufferTexture1;
			sampler2D _CameraGBufferTexture2;


			float4 FragmentProgram (Interpolators i) : SV_Target {
				...
				float3 worldPos = mul(unity_CameraToWorld, float4(viewPos, 1)).xyz;

				float3 albedo = tex2D(_CameraGBufferTexture0, uv).rgb;
				float3 specularTint = tex2D(_CameraGBufferTexture1, uv).rgb;
				float3 smoothness = tex2D(_CameraGBufferTexture1, uv).a;
				float3 normal = tex2D(_CameraGBufferTexture2, uv).rgb * 2 - 1; //存出的范围为0~1, 转成法线的范围-1~1

			}
			
		}

		计算BRDF以及配置灯光：UNITY_BRDF_PBS
		{
			FragmentOutput MyFragmentProgram (Interpolators i) {
				float alpha = GetAlpha(i);
				#if defined(_RENDERING_CUTOUT)
					clip(alpha - _AlphaCutoff);
				#endif

				InitializeFragmentNormal(i);

				float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos.xyz);

				float3 specularTint;
				float oneMinusReflectivity;
				float3 albedo = DiffuseAndSpecularFromMetallic(
					GetAlbedo(i), GetMetallic(i), specularTint, oneMinusReflectivity
				);
				#if defined(_RENDERING_TRANSPARENT)
					albedo *= alpha;
					alpha = 1 - oneMinusReflectivity + alpha * oneMinusReflectivity;
				#endif

				//计算BRDF
				float4 color = UNITY_BRDF_PBS(
					albedo, specularTint,
					oneMinusReflectivity, GetSmoothness(i),
					i.normal, viewDir,
					CreateLight(i), CreateIndirectLight(i, viewDir)
				);
				color.rgb += GetEmission(i);
				#if defined(_RENDERING_FADE) || defined(_RENDERING_TRANSPARENT)
					color.a = alpha;
				#endif

				FragmentOutput output;
				#if defined(DEFERRED_PASS)
					#if !defined(UNITY_HDR_ON)
						color.rgb = exp2(-color.rgb);
					#endif
					output.gBuffer0.rgb = albedo;
					output.gBuffer0.a = GetOcclusion(i);
					output.gBuffer1.rgb = specularTint;
					output.gBuffer1.a = GetSmoothness(i);
					output.gBuffer2 = float4(i.normal * 0.5 + 0.5, 1);
					output.gBuffer3 = color;
				#else
					output.color = ApplyFog(color, i);
				#endif
				return output;
			}
		}


		阴影：_ShadowMapTexture 屏幕空间阴影贴图
		{	

			MyDeferredShading.cginc ==> 延迟渲染下，灯光使用的shader

			在“My Lighting”中，我们依靠AutoLight中的宏来确定由阴影引起的光衰减。遗憾的是，该文件在编写时并没有考虑到延迟光照的情况。因此，我们需要自己进行阴影采样。通过_ShadowMapTexture变量可以访问阴影贴图
			UnityLight CreateLight (float2 uv) {
				UnityLight light;
				light.dir = -_LightDir;
				float shadowAttenuation = tex2D(_ShadowMapTexture, uv).r;
				light.color = _LightColor.rgb * shadowAttenuation;
				return light;
			}
		}
	}

	


}

16***************静态光照：其实就是使用烘焙贴图（数据被认为是间接光照），只能作用于静态物体，包含漫射光不支持镜面光，不会投射实时阴影，光照探针可以支持动态物体（较小的）
{
	1 Lightmapping 光照贴图 ==> 静态物体
	{
		执行照明计算非常昂贵。延迟渲染使我们可以使用很多灯光，但是阴影仍然是一个限制因素。
		如果场景是动态的，那么我们将不可避免地执行这些计算。但是，如果光源和几何物体都不变，那么我们可以只计算一次光源并重复使用它。
		这样的话就可以在我们的场景中放置许多灯光，而又不必在运行时渲染它们。也可以使用区域光，但这些区域光同样不能用作实时照明

		烘焙光：只作用于静态对象
		{
			开始进行光照映射之前，请将唯一的灯光对象的“Mode”更改为“Baked”，而不是“Realtime”。
			将主定向光转换为烘焙光后，它将不再包含在动态光照中。从动态对象的角度来看，就不存在光了。唯一剩下的就是环境照明，它仍然基于主光源

			要实际启用光照贴图Lightmapping，请在照明窗口的“Mixed Lighting”部分中启用“Baked Global Illumination”。然后将“Lighting Mode”设置为“Baked Indirect”。
			尽管是间接光的名称，但它也包括直接照明。它通常仅用于向场景添加间接光。另外，请确保已禁用“Realtime Global Illumination”，因为我们尚不支持。
		}

		标记对象为static：  静态几何体

		请注意，与使用实时照明相比，光照贴图的结果亮度较弱。那是因为缺少镜面光，它只是包含漫射光。
		因为镜面光取决于视角，也就是取决于相机。通常，相机是可移动的，因此不能包含在光照贴图中。此限制意味着光照贴图可以用于微弱的灯光和暗淡的表面，但不适用于强直射的灯光或闪亮的表面。
		如果要使用镜面光，则必须使用实时照明。因此，通常最终会混合使用烘焙光和实时光。

		光照贴图设置
		{
			现在都使用Progressive
			在执行其他任何操作之前，请将“Directional”设置为“Non-Direction”。稍后我们将讨论其他模式。// 其实区别就是是否把法线信息也烘焙到光照贴图里

			Unity的默认对象都具有配置为光照贴图的UV坐标。对于导入的网格，你可以提供自己的坐标，或者让Unity为你生成它们。
			烘焙后，可以在光照贴图中看到纹理展开。它们需要多少空间取决于场景中对象的大小和光照贴图分辨率设置
		}

		间接光：烘焙光意味着我们失去了镜面光，但我们获得了间接光
		{
			烘焙间接光时，Unity考虑到这一点。结果就是，物体会根据附近的物体进行上色。
			自发光表面也会影响烘焙的光线。它们成为间接光源。
		}

		光照贴图最多可以处理半透明的表面。光线将通过它们，其颜色不会被它们过滤。
		cutout 材质也可以
	}

	2 使用光照贴图：数据被认为是间接光照
	{
		对光照贴图进行采样
		{
			当着色器应该使用光照贴图时，Unity将寻找与LIGHTMAP_ON关键字关联的变体。因此，我们必须为此关键字添加一个多编译指令。使用前向渲染路径时，仅在基本pass中对光照贴图进行采样

			#pragma multi_compile _ LIGHTMAP_ON

			使用光照贴图时，Unity将永远不会包含顶点光照。他们的关键字是互斥的。因此，我们不需要同时具有VERTEXLIGHT_ON和LIGHTMAP_ON的变体
			延迟渲染路径中也支持光照贴图，因此也应将关键字添加到延迟pass中
		}

		光照贴图坐标
		{
			用于采样光照贴图的坐标存储在第二个纹理坐标通道uv1中。因此，将此通道添加到“My Lighting”中的VertexData。
			struct VertexData {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float2 uv : TEXCOORD0;
				float2 uv1 : TEXCOORD1; //光照贴图的纹理坐标
			};

			顶点数据中的坐标定义了用于光照贴图的网格的纹理展开。但这并没有告诉我们该展开的位置在光照图中的位置，也没有告诉我们其大小
			我们必须缩放和偏移坐标才能得出最终的光照贴图坐标

			不幸的是，我们不能使用方便的TRANSFORM_TEX宏，因为它假定光照贴图转换定义为unity_Lightmap_ST，而实际上是unity_LightmapST。由于这种不一致，我们必须手动进行操作。
			i.lightmapUV = v.uv1 * unity_LightmapST.xy + unity_LightmapST.zw;
		}

		采样光照贴图: 使用UNITY_SAMPLE_TEX2D
		{
			因为光照贴图数据被认为是间接光照，所以我们将在CreateIndirectLight函数中对其进行采样。当有光照贴图可用时，我们必须将它们用作间接光照的源，而不是球谐函数
			UnityIndirect CreateIndirectLight (Interpolators i, float3 viewDir) {
				…
				
				#if defined(VERTEXLIGHT_ON)
					indirectLight.diffuse = i.vertexLightColor;
				#endif

				#if defined(FORWARD_BASE_PASS) || defined(DEFERRED_PASS)
					#if defined(LIGHTMAP_ON)
						indirectLight.diffuse = 0; //****
					#else
						indirectLight.diffuse += max(0, ShadeSH9(float4(i.normal, 1)));
					#endif
					float3 reflectionDir = reflect(-viewDir, i.normal);
					…
				#endif

				return indirectLight;
			}


			unity_Lightmap的确切形式取决于目标平台。它定义为UNITY_DECLARE_TEX2D（unity_Lightmap）。为了对其进行采样，我们将使用UNITY_SAMPLE_TEX2D宏而不是tex2D

			indirectLight.diffuse =
				UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lightmapUV); //表现不对，还需要解码


			现在我们得到了间接照明，但看起来不对。那是因为光照图数据已被编码。颜色以RGBM格式存储或以半强度存储，以支持高强度光。UnityCG的DecodeLightmap函数负责为我们解码
			indirectLight.diffuse = DecodeLightmap(
				UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lightmapUV)
			);


			完整的代码
			{
				#if defined(LIGHTMAP_ON)
					//光照贴图数据被认为是间接光照
					//必须将它们用作间接光照的源，而不是球谐函数
					//采样光照贴图 UNITY_SAMPLE_TEX2D
					//因为光照图数据已被编码,颜色以RGBM格式存储或以半强度存储，以支持高强度光
					//DecodeLightmap解码函数
					indirectLight.diffuse =
						DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lightmapUV));
					
					#if defined(DIRLIGHTMAP_COMBINED)
						//直接需要烘焙光方向 方向图可通过unity_LightmapInd获得
						float4 lightmapDirection = UNITY_SAMPLE_TEX2D_SAMPLER(
							unity_LightmapInd, unity_Lightmap, i.lightmapUV
						);
						//解码
						indirectLight.diffuse = DecodeDirectionalLightmap(
							indirectLight.diffuse, lightmapDirection, i.normal
						);
					#endif
				#else
					//球谐函数
					indirectLight.diffuse += max(0, ShadeSH9(float4(i.normal, 1)));
				#endif
			}
		}
	}

	3 创建光照贴图：继续支持透明物体
	{
		虽然光照贴图似乎已经可以与我们的着色器一起使用，但这仅适用于我们简单的测试场景。
		当前，光照贴图器始终将我们的对象视为不透明和纯白色，即使它们并非如此。我们必须对着色器进行一些调整，甚至还要添加另一个pass来完全支持光照贴图

		半透明阴影：_Color 属性
		{
			光照贴图器不使用实时渲染管道，因此不使用着色器来完成其工作。当尝试使用半透明阴影时，这是最明显的。通过给它的色调的alpha分量设置为小于1的材质，使立方体顶面为半透明的。==》 （半透明的顶，错误的阴影）

			光照贴图器仍将屋顶视为实心，这是不正确的。它使用材质的渲染类型来确定如何处理表面，这应该告诉我们我们的对象是半透明的。
			实际上，它确实知道屋顶是半透明的，只是将其视为完全不透明。发生这种情况是因为它使用_Color材质属性的alpha成分以及主纹理来设置不透明度。但是我们没有该属性，而是使用_Tint！

			更糟糕的是，没有办法告诉灯光映射器要使用哪个属性。因此，要使光照贴图起作用，除了将_Tint的用法替换为_Color之外，我们别无选择。首先，更新我们的着色器的属性
			Properties {
			//		_Tint ("Tint", Color) = (1, 1, 1, 1)
					_Color ("Tint", Color) = (1, 1, 1, 1)
					…
				}

			//float4 _Tint;
			float4 _Color;
			…

			float3 GetAlbedo (Interpolators i) {
				float3 albedo = tex2D(_MainTex, i.uv.xy).rgb * _Color.rgb;
				…
			}

			float GetAlpha (Interpolators i) {
				float alpha = _Color.a;
				…
			}


			The same goes for My Shadows.
			//float4 _Tint;
			float4 _Color;
			…

			float GetAlpha (Interpolators i) {
				float alpha = _Color.a;
				…
			}

		}

		Cutout 阴影：_Cutoff属性
		{
			Cutout 阴影也有类似的问题。光照贴图器希望将alpha截止值存储在_Cutoff属性中，但是我们正在使用_AlphaCutoff。结果，它使用默认截止值为1

				Properties {
					…

			//		_AlphaCutoff ("Alpha Cutoff", Range(0, 1)) = 0.5
					_Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5

					…
				}

				//float _AlphaCutoff;
				float _Cutoff;

				…

				FragmentOutput MyFragmentProgram (Interpolators i) {
					float alpha = GetAlpha(i);
					#if defined(_RENDERING_CUTOUT)
						clip(alpha - _Cutoff);
					#endif

					…
				}


				Update My Shadows too

				//float _AlphaCutoff;
				float _Cutoff;

				…

				float4 MyShadowFragmentProgram (Interpolators i) : SV_TARGET {
					float alpha = GetAlpha(i);
					#if defined(_RENDERING_CUTOUT)
						clip(alpha - _Cutoff);
					#endif

					…
				}
		}


		增加meta pass ：弄清楚对象的表面颜色
		{
			下一步是确保光照贴图器使用正确的表面反照率和发射率。现在，一切总是纯白色的。你可以通过将地板变绿来看到此情况。它应该导致绿色的间接光，但仍然是白色。

			为了弄清楚对象的表面颜色，光照贴图器查找其光照模式设置为Meta的着色器通道
			让我们向着色器添加这样的pass。这是一个基本pass，不应使用剔除
			Pass {
				Tags {
					"LightMode" = "Meta"
				}

				Cull Off

				CGPROGRAM

				#pragma vertex MyLightmappingVertexProgram
				#pragma fragment MyLightmappingFragmentProgram

				#include "My Lightmapping.cginc"

				ENDCG
			}
		}

		完整的代码
		{
			Interpolators MyLightmappingVertexProgram (VertexData v) {
				Interpolators i;

				//必须使用光照贴图坐标
				v.vertex.xy = v.uv1 * unity_LightmapST.xy + unity_LightmapST.zw;
				v.vertex.z = v.vertex.z > 0 ? 0.0001 : 0;

			    i.pos = UnityObjectToClipPos(v.vertex);

				i.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
				i.uv.zw = TRANSFORM_TEX(v.uv, _DetailTex);
				return i;
			}

			float4 MyLightmappingFragmentProgram (Interpolators i) : SV_TARGET {
				UnityMetaInput surfaceData;
				surfaceData.Emission = GetEmission(i);
				float oneMinusReflectivity;

				//要获得反照率，必须再次使用DiffuseAndSpecularFromMetallic。该函数具有用于镜面反射的颜色和反射率的输出参数
				//surfaceData.SpecularColor捕捉镜面颜色
				surfaceData.Albedo = DiffuseAndSpecularFromMetallic(
					GetAlbedo(i), GetMetallic(i),
					surfaceData.SpecularColor, oneMinusReflectivity
				);
				
				//非常粗糙的金属应该产生比我们目前的计算结果更多的间接光
				//标准着色器通过将部分镜面反射颜色添加到反照率来对此进行补偿
				float roughness = SmoothnessToRoughness(GetSmoothness(i)) * 0.5;
				surfaceData.Albedo += surfaceData.SpecularColor * roughness;

				return UnityMetaFragment(surfaceData);
			}
		}
	}

	4 定向光照贴图:包含了大多数烘焙光所来自的方向
	{
		光照贴图器仅使用几何图形的顶点数据，不考虑法线贴图。光照贴图分辨率太低，无法捕获典型法线贴图提供的细节。这意味着静态照明将是平坦的。当使用具有法线贴图的材质时，这一点变得非常明显。

		定向
		{
			通过将“Directional Mode”改回“Directional”，可以使法线贴图在烘焙的光照下工作。
			使用定向光照贴图时，Unity将创建两个贴图，而不只是一个。第一张图包含照常的照明信息，称为强度图。第二张地图称为方向图。它包含了大多数烘焙光所来自的方向。

			当方向图可用时，我们可以使用它来对烘焙的光执行简单的漫反射着色。这使得可以应用法线贴图
		}

		采样方向
		{
			当有方向性光照贴图可用时，Unity将寻找同时带有LIGHTMAP_ON和DIRLIGHTMAP_COMBINED关键字的着色器变体。无需手动为此添加多编译指令，我们可以在正向基本传递中使用#pragma multi_compile_fwdbase

			#if defined(DIRLIGHTMAP_COMBINED)
				float4 lightmapDirection = UNITY_SAMPLE_TEX2D_SAMPLER(
					unity_LightmapInd, unity_Lightmap, i.lightmapUV
				);
			#endif
		}

		使用方向: 先解碼
		{
			要使用此方向，我们首先必须对其进行解码。然后，我们可以使用法线向量执行点积运算，以找到漫反射因子并将其应用于颜色。
			但是方向贴图实际上并不包含单位长度方向，它要更复杂一些。幸运的是，我们可以使用UnityCG的DecodeDirectionalLightmap函数解码方向性数据并为我们执行着色。

				float4 lightmapDirection = UNITY_SAMPLE_TEX2D_SAMPLER(
					unity_LightmapInd, unity_Lightmap, i.lightmapUV
				);

				//****
				indirectLight.diffuse = DecodeDirectionalLightmap(
					indirectLight.diffuse, lightmapDirection, i.normal
				);
		}
	}

	5 光照探针:Light Probes, 用于动态物体，使用球谐函数存储照明信息，仅适用于相当小的对象
	{
		光照贴图仅适用于静态对象，不适用于动态对象。结果，动态对象无法放入带有烘焙照明的场景中。当根本没有实时照明时，这是非常明显的。
		为了更好地混合静态和动态对象，我们还必须以某种方式将烘焙的光照应用于动态对象。Unity为此提供了光照探针

		光照探针是空间中的一个点，具有有关该位置的照明的信息。代替纹理，它使用球谐函数来存储此信息

		如果可用，这些探针将用于动态对象，而不是全局环境数据。因此，我们要做的就是创建一些探针，等到烘焙完成，我们的着色器将自动使用它们


		创建一个光探针组
		{
			通过GameObject/ Light / Light Probe Group将一组光探测器添加到场景中。
			这将创建一个新的游戏对象，其中包含八个以立方体形式排列的探针。在为动态对象着色时将立即使用它
		}

		放置光照探针
		{
			光探针组将其包围的体积划分为四面体区域。四个探针定义了四面体的角。对这些探针进行插值，以确定动态对象所用的最终球谐函数，具体取决于其在四面体内部的位置。
			这意味着将动态对象视为单个点，因此它仅适用于相当小的对象。
			放置光探针只需调整一下，直到获得可接受的结果，就像操作光贴图设置一样。首先将要包含动态对象的区域包围起来。

			然后根据照明条件的变化添加更多的探头。请勿将它们放置在静态几何体中，这一点至关重要。也不要将它们放在不透明的单面几何图形的错误一侧。
		}
	}
}


17***************混合光照：烘焙光和实时光，项目里用的最多,支持实时阴影
{
	烘焙光的缺点
		首先，镜面照明无法烘焙。
		其次，烘焙的光仅通过光探头影响动态物体。
		第三，烘焙光不会投射实时阴影


	1 烘焙间接光
	{
		混合模式：
		{
			间接照明是烘焙照明有，而实时照明的没有的东西，因为它需要光照贴图。由于间接光照可以为场景增加很多真实感，因此如果我们将其与实时光照结合起来，那就更好了。

			这当然是可以的，但这也意味着阴影会变得更加昂贵。它要求将Mixed Lighting下的的Lighting Mode设置为Baked Indirect。
			要使用混合照明，必须将光源的“Mode”设置为“Mixed”

			将主定向光转换为混合光后，将发生两件事。首先，Unity将再次烘焙光照贴图。但这次，它仅存储间接光照，因此生成的光照贴图会比以前更暗

			其次，所有东西都会被照亮，就好像主光源设置为实时一样，只是表现有所不同。
			光照贴图用于将间接光添加到静态对象，而不是球谐函数或探针。
			动态对象仍将光探针用作间接光
		}
	}

	2 使用阴影遮罩Shadowmask： LightingMode设置为shadowmask, 会比Baked Indirect效率高
	{
		间接照明的混合模式光非常昂贵。它们需要的工作量与实时照明一样多，此外还需要间接照明的光照贴图
		与完全烘焙的灯光相比，最重要的是添加了实时阴影。幸运的是，结合实时阴影，有一种方法仍然可以将阴影烘焙到光照贴图中

		在此模式下，间接光照和混合光照的阴影衰减都存储在光照贴图中。阴影存储在单独的贴图中（ shadowmask），称为阴影遮罩
		仅使用主定向光时，所有照亮的光源将在阴影遮罩中显示为红色。之所以为红色，是因为阴影信息存储在纹理的R通道中。实际上，由于地图具有四个通道，因此最多可以存储四个灯光的阴影。

		Unity创建阴影遮罩后，静态对象投射的阴影将消失。仅光探针仍会考虑它们。动态对象的阴影不受影响。

		采样阴影遮罩Shadowmask
		{
			UnitySampleBakedOcclusion函数。它需要光照贴图的UV坐标和世界位置作为参数
					float FadeShadows (Interpolators i, float attenuation) {
			#if HANDLE_SHADOWS_BLENDING_IN_GI
				…
				float bakedAttenuation =
					UnitySampleBakedOcclusion(i.lightmapUV, i.worldPos); //***
				attenuation = saturate(attenuation + shadowFade);
			#endif
			return attenuation;
			}
		}

		现在，我们可以在静态对象上同时获取实时阴影和烘焙阴影，并且它们可以正确融合。实时阴影仍会超出阴影距离逐渐消失，但烘焙的阴影不会消失


		延迟渲染需要把shadowmask信息添加到G-Buffer里才能支持

		多灯光
		{
			由于阴影遮罩具有四个通道，因此可以一次支持多达四个重叠的光。
			例如，以下是屏幕快照，其中包含场景的光照贴图以及其他三个聚光灯。我降低了主光源的强度，因此更容易看到聚光灯
			主方向光的阴影仍存储在R通道中。你还可以看到G和B通道中存储的聚光灯的阴影。最后一个聚光灯的阴影存储在A通道中，该通道不可见。
		}
	}

	3 阴影减法 Subtractive Shadows： 仅适用于正向渲染
	{
		混合照明是不错的选择，但它不如完全烘焙的照明便宜。如果你以低性能的硬件为目标，那么混合照明是不可行的。可以使用烘焙的照明，
		但是你可能确实需要让动态对象在静态对象上投射阴影。在这种情况下，可以使用Subtractive 混合照明模式。

		切换到减法模式后，场景会变亮很多。发生这种情况是因为静态对象现在同时使用完全烘焙的光照贴图和直接光照。像往常一样，动态对象仍然使用光探针和直接照明。
		（静态对象会受光两次）
		减法模式仅适用于正向渲染。使用延迟渲染路径时，相关对象将像透明对象一样回退到前向。
	}

}

18***************实时光全局光照、探针体积、LOD组
{
	1 实时全局光照: 光源经常变化的，天气系统之类的，在运行时计算光照贴图和探针
	{
		得益于光探针的原理，烘焙光对于静态几何体非常友好，对于动态几何体也非常适用。但是，它不能处理动态光。混合模式下的光源可以进行一些实时调整，
		但是太多的物体因为烘焙的间接光源，需要保持不变是显而易见的。因此，当你有户外场景时，太阳必须保持不变。它不能像现实生活中那样穿越天空，因为那样需要逐渐改变GI。

		开启实时全局光照：“Lighting”窗口的“Realtime Lighting”部分中的复选框启用该功能。
		要查看实时GI的实际效果，请将测试场景中的主光源模式设置为实时
		只有动态物体接受实时GI

		事实证明，只有动态对象才能从实时GI中受益。静态对象变暗了。那是因为光探针会自动包含实时GI。静态对象必须采样实时光照贴图，该实时光照贴图与烘焙的光照贴图不同。我们的着色器尚未执行此操作。

		烘焙实时GI
		{
			尽管实时光照贴图已经烘焙，并且可能看起来正确，但是我们的meta pass实际上使用了错误的坐标。实时GI具有自己的光照贴图坐标，最终可能与静态光照贴图的坐标不同。
			Unity根据光照贴图和对象设置自动生成这些坐标。它们存储在第三个网格UV通道中。因此，将此数据添加到“My Lightmapping”中的VertexData。


			struct VertexData {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float2 uv : TEXCOORD0;
				float2 uv1 : TEXCOORD1; //烘焙光照贴图坐标
				float2 uv2 : TEXCOORD2; //实时光照贴图坐标
			};
		}

		采样实时光照贴图
		{
			struct Interpolators {
				…

				#if defined(DYNAMICLIGHTMAP_ON)
					float2 dynamicLightmapUV : TEXCOORD7;
				#endif
			};

			Interpolators MyVertexProgram (VertexData v) {
				…

				#if defined(LIGHTMAP_ON) || ADDITIONAL_MASKED_DIRECTIONAL_SHADOWS
					i.lightmapUV = v.uv1 * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif

				#if defined(DYNAMICLIGHTMAP_ON)
					i.dynamicLightmapUV =
						v.uv2 * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				#endif

				…
			}


			UnityIndirect CreateIndirectLight (Interpolators i, float3 viewDir)
			{
				...
				#if defined(DYNAMICLIGHTMAP_ON)
					//采样实时光照贴图 unity_DynamicLightmap
					float3 dynamicLightDiffuse = DecodeRealtimeLightmap(
						UNITY_SAMPLE_TEX2D(unity_DynamicLightmap, i.dynamicLightmapUV)
					);

					#if defined(DIRLIGHTMAP_COMBINED)
						float4 dynamicLightmapDirection = UNITY_SAMPLE_TEX2D_SAMPLER(
							unity_DynamicDirectionality, unity_DynamicLightmap,
							i.dynamicLightmapUV
						);
		            	indirectLight.diffuse += DecodeDirectionalLightmap(
		            		dynamicLightDiffuse, dynamicLightmapDirection, i.normal
		            	);
					#else
						indirectLight.diffuse += dynamicLightDiffuse;
					#endif
				#endif

				...
			}
			
		}
	}

	2 光探针代理体积（LPPVs）
	{
		烘焙GI和实时GI都通过光探针应用于动态对象。对象的位置用于内插值光探针数据，然后用于应用GI。这适用于比较小的对象，但对于较大的对象而言过于粗糙。
	}

	3 LOD组件
	{
		当对象最终仅覆盖应用程序窗口的一小部分时，你不需要高度详细的网格即可对其进行渲染。
		可以根据对象的视图大小使用不同的网格。这称为细节级别（level of detail），或简称LOD。Unity允许我们通过LOD Group组件执行此操作

		LOD不同级别之间的淡入淡出
		{
			LOD组的缺点是，当LOD级别更改时，它在视觉上很明显。几何突然出现，消失或改变形状。可以通过将相邻LOD级别之间的交叉淡入淡出来缓解这种情况，
			这可以通过将组的“Fade Mode”设置为“Cross Fade”来实现
		}

		支持交叉淡化: 支持几何体以及阴影
		{
			默认情况下，Unity的标准着色器不支持交叉淡化。
			需要复制标准着色器，并为LOD_FADE_CROSSFADE关键字添加一个多编译指令。我们也需要添加该指令以支持My First Lighting Shader的交叉渐变。将指令添加到除meta pass之外的所有pass中。
			我们将使用抖动在LOD级别之间进行转换。该方法适用于正向和延迟渲染以及阴影

			可以在片段程序开始时使用UnityApplyDitherCrossFade函数执行交叉淡化。
			FragmentOutput MyFragmentProgram (Interpolators i) {
				#if defined(LOD_FADE_CROSSFADE)
					UnityApplyDitherCrossFade(i.vpos);
				#endif

				…
			}

			交叉淡化现在适用于几何体了。为了使它也适用于阴影，我们必须调整“My Shadows”。首先，在进行交叉淡入淡出时必须使用vpos。其次，我们还必须在片段程序开始时使用UnityApplyDitherCrossFade。
			struct Interpolators {
				#if SHADOWS_SEMITRANSPARENT || defined(LOD_FADE_CROSSFADE)
					UNITY_VPOS_TYPE vpos : VPOS;
				#else
					float4 positions : SV_POSITION;
				#endif

				…
			};

			…

			float4 MyShadowFragmentProgram (Interpolators i) : SV_TARGET {
				#if defined(LOD_FADE_CROSSFADE)
					UnityApplyDitherCrossFade(i.vpos);
				#endif

				…
			}
		}
	}
}

19***************GPU实例（Instancing）：相同的mesh和material
{
	1、合并实例 Batching Instances
	{
		指示GPU绘制图像需要花费时间。为其提供数据（包括网格和材质属性）也需要时间。我们已经知道有两种方法可以减少绘制调用的数量，即静态和动态批处理。
		Unity可以将静态对象的网格合并为更大的静态网格，从而减少draw calls。但只有使用相同材质的对象才能以这种方式组合，它是以存储更多网格数据为代价的。
		启用动态批处理后，Unity在运行时会对视图中的动态对象执行相同的操作。但仅适用于小型网格，否则会适得其反，开销反而变得非常大。

		还有另一种组合绘图调用的方法。被称为GPUinstancing 或几何instancing 。与动态批处理一样，此操作在运行时针对可见对象完成。这个想法是让GPU一次性渲染同一网格多次。
		因此，它不能组合不同的网格或材质，但不局限于小网格。这里我们将试试这个方法。


		支持实例化（Instancing）
		{
			默认情况下，还无法进行GPU实例化。必须设计着色器来支持它。我们需要给每种材质显式的启用实例化。Unity的标准着色器对此有一个开关。
			我们也向MyLightingShaderGUI添加实例化的开关。像标准着色器的GUI一样，我们将为其创建“Advanced Options”部分。
			可以通过调用MaterialEditor.EnableInstancingField方法来添加开关。在一个新的DoAdvanced方法里添加逻辑吧。

			仅当着色器实际支持实例化时，才会显示该开关。我们可以通过将#pragma multi_compile_instancing指令添加到着色器来启用此支持。
			这将为一些关键字启用着色器变体，在我们的示例中为INSTANCING_ON，但其他关键字也是可以的。为“My First Lighting”的base pass执行此操作。


		}

		实例 Ids： 与实例相对应的数组索引称为其实例ID
		{
			GPU通过顶点数据将其传递到着色器的顶点程序。在大多数平台上，它是一个无符号整数，名为instanceID，具有SV_InstanceID语义。
			我们可以简单地使用UNITY_VERTEX_INPUT_INSTANCE_ID宏将其包含在我们的VertexData结构中
			struct VertexData {
				UNITY_VERTEX_INPUT_INSTANCE_ID
				float4 vertex : POSITION;
				…
			};

			//启用实例化后，我们现在可以在顶点程序中访问实例ID。有了它，就可以在变换顶点位置时使用正确的矩阵。
			//但是，UnityObjectToClipPos没有矩阵参数。它始终使用unity_ObjectToWorld。
			//要解决此问题，UnityInstancing包含文件会使用使用矩阵数组的宏覆盖unity_ObjectToWorld。这可以被认为是一种宏的
			//要使Hack工作，实例的数组索引必须对所有着色器代码全局可用。我们通过UNITY_SETUP_INSTANCE_ID宏进行手动设置，该宏必须在顶点程序中完成，然后再执行任何可能需要它的代码。
			InterpolatorsVertex MyVertexProgram (VertexData v) {
				InterpolatorsVertex i;
				UNITY_INITIALIZE_OUTPUT(Interpolators, i);
				UNITY_SETUP_INSTANCE_ID(v);
				i.pos = UnityObjectToClipPos(v.vertex);
				…

			}
		}

		合批大小: 缓冲区的大小不同平台导致合批次数不同
		{
			你最终得到的批次数量可能与我得到的数量不同。在我的情况下，以40批渲染5000个球体实例，这意味着每批125个球体。
			每个批次都需要自己的矩阵数组，此数据发送到GPU并存储在内存缓冲区中，
			在Direct3D中称为常量缓冲区，在OpenGL中称为统一（uniform）缓冲区。这些缓冲区具有最大容量限制，它限制了一个批次中可以容纳多少个实例。假设台式机GPU每个缓冲区的限制为64KB
		}

		实例化阴影
		{
			为5000个球体渲染阴影会给GPU造成巨大损失。但是我们也可以在渲染球体阴影时使用GPU实例化。将所需指令添加到阴影caster pass中。
			#pragma multi_compile_shadowcaster
			#pragma multi_compile_instancing

			再将UNITY_VERTEX_INPUT_INSTANCE_ID和UNITY_SETUP_INSTANCE_ID添加到“My Shadows”中。
			struct VertexData {
				UNITY_VERTEX_INPUT_INSTANCE_ID
				…
			};

			…

			InterpolatorsVertex MyShadowVertexProgram (VertexData v) {
				InterpolatorsVertex i;
				UNITY_SETUP_INSTANCE_ID(v);
				…
			}


			现在批次有了大幅度的降低。
		}

		多灯光： 只能使用延迟渲染
		{
			我们仅在base pass和shadow caster pass中添加了实例化支持。
			因此，批处理不适用于其他光源。要验证这一点，请停用主光源并添加一些会影响多个球体的聚光灯或点光源。但不要为它们打开阴影，因为那样会降低帧率

			事实证明，不受额外光照影响的球体仍与阴影一起进行批处理。但是其他区域甚至没有在其base pass中分批处理。对于这些情况，Unity完全不支持批处理。要将实例化与多个光源结合使用，
			现在别无选择，只能切换到deferred rendering 路径。为此，请将所需的编译器指令添加到着色器的deferred pass中。
		}
	}

	2 混合材质属性
	{
		所有批处理形式的限制之一是它们仅限于具有相同材质的对象。当我们希望渲染的对象具有多样性时，此限制就会成为阻碍。

		材质属性块 Material Property Blocks
		{
			除了使用每个球体创建新的材质实例外，我们还可以使用材质属性块。这些是小的对象，其中包含着色器属性的重写。设置属性块的颜色并将其传递给球体的渲染器，而不是直接分配材质的颜色
			MaterialPropertyBlock properties = new MaterialPropertyBlock();
			properties.SetColor(
				"_Color", new Color(Random.value, Random.value, Random.value)
			);
			t.GetComponent<MeshRenderer>().SetPropertyBlock(properties);

			MeshRenderer.SetPropertyBlock方法复制该块的数据，因此不依赖于我们在本地创建的块。这使我们可以重用一个块来配置所有实例。
			void Start () {
				MaterialPropertyBlock properties = new MaterialPropertyBlock();
				for (int i = 0; i < instances; i++) {
					Transform t = Instantiate(prefab);
					t.localPosition = Random.insideUnitSphere * radius;
					t.SetParent(transform);

					//MaterialPropertyBlock properties = new MaterialPropertyBlock();
					properties.SetColor(
						"_Color", new Color(Random.value, Random.value, Random.value)
					);
					t.GetComponent<MeshRenderer>().SetPropertyBlock(properties);
				}
			}
		}

		Property Buffers：UNITY_INSTANCING_CBUFFER_START ，UNITY_INSTANCING_CBUFFER_END
		{
			渲染实例对象时，Unity通过将数组上传到其内存来使转换矩阵可用于GPU。Unity对存储在材料属性块（Material Property Blocks）中的属性执行相同的操作。
			但这要起作用的话，必须在“My Lighting”中定义一个适当的缓冲区。

			声明实例化缓冲区的工作类似于创建诸如插值器之类的结构，但是确切的语法因平台而异。
			我们可以使用UNITY_INSTANCING_CBUFFER_START和UNITY_INSTANCING_CBUFFER_END宏来解决差异。启用实例化后，它们还不会做任何操作。

			将_Color变量的定义放在实例缓冲区中。UNITY_INSTANCING_CBUFFER_START宏需要一个名称参数。实际名称无关紧要。宏以UnityInstancing_为其前缀，以防止名称冲突
			UNITY_INSTANCING_CBUFFER_START(InstanceProperties)
				float4 _Color;
			UNITY_INSTANCING_CBUFFER_END

			像变换矩阵一样，启用实例化后，颜色数据将作为数组上传到GPU。UNITY_DEFINE_INSTANCED_PROP宏会为我们处理正确的声明语法。
			UNITY_INSTANCING_CBUFFER_START(InstanceProperties)
			//	float4 _Color;
				UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
			UNITY_INSTANCING_CBUFFER_END


			要访问片段程序中的数组，我们还需要在其中知道实例ID。因此，将其添加到interpolator 结构中。
			struct InterpolatorsVertex {
				UNITY_VERTEX_INPUT_INSTANCE_ID
				…
			};

			struct Interpolators {
				UNITY_VERTEX_INPUT_INSTANCE_ID
				…
			};


			在顶点程序中，将ID从顶点数据复制到interpolators。启用实例化时，UNITY_TRANSFER_INSTANCE_ID宏定义此简单操作，否则不执行任何操作

			InterpolatorsVertex MyVertexProgram (VertexData v) {
				InterpolatorsVertex i;
				UNITY_INITIALIZE_OUTPUT(Interpolators, i);

				//要使Hack工作，实例的数组索引必须对所有着色器代码全局可用。
				//我们通过UNITY_SETUP_INSTANCE_ID宏进行手动设置，该宏必须在顶点程序中完成，然后再执行任何可能需要它的代码。
				UNITY_SETUP_INSTANCE_ID(v);

				//从顶点数据复制到interpolators
				UNITY_TRANSFER_INSTANCE_ID(v, i);
				…
			}

			在片段程序的开头，使ID全局可用，就像在顶点程序中一样
			FragmentOutput MyFragmentProgram (Interpolators i) {
				UNITY_SETUP_INSTANCE_ID(i);
				…
			}

			现在，我们必须在不使用实例化时以_Color的形式访问颜色，而在启用实例化时以_Color [unity_InstanceID]的形式访问颜色。我们可以为此使用UNITY_ACCESS_INSTANCED_PROP宏。
			float3 GetAlbedo (Interpolators i) {
				float3 albedo =
					tex2D(_MainTex, i.uv.xy).rgb * UNITY_ACCESS_INSTANCED_PROP(_Color).rgb;
				…
			}

			float GetAlpha (Interpolators i) {
				float alpha = UNITY_ACCESS_INSTANCED_PROP(_Color).a;
				…
			}
		}
	}

}