
--[==[
性能标准
{
	耗时推荐值：FPS 渲染 逻辑代码 UI模块 物理模块 GPU耗时
	内存推荐值：
	{
		Reserved Total: 可以理解为分配总内存
		避免游戏闪退的重点在于控制PSS内存峰值。而PSS内存的大头又在于Reserved Total中的【资源内存和Mono堆内存】。对于使用Lua的项目来说，还应关注Lua内存。

		根据UWA的经验，只有当PSS内存峰值控制在硬件总内存的0.5-0.6倍以下的时候，闪退风险才较低
		举例而言，对于2G的设备而言，PSS内存应控制在1G以下为最佳，3G的设备则应控制在1.5G以下
		而对于大多数项目而言，PSS内存大约高于Reserved Total 200MB-300MB左右，故2G设备的Reserved Total应控制在700MB以下、3G设备则控制在1G以下。

		内存类型
		{
			资源内存
			{
				Texture
				Mesh
				Shader
				Animation Clip
				Audio Clip
				Render Texture
				Font
				ParticleSystem
			}
			Mono堆内存
			Lua内存
		}
	}
}
]==]

--[==[
性能排查工具
{
	Unity Profiler
	Unity FrameDebugger
	Mali Offline Compiler:该工具主要用来计算Shader的复杂度，结合分档的高中低数值来判断Shader是否过于复杂。
	XCode FrameDebugger: 特别强大
	GOT Online
}

]==]

--[==[
策略导致的内存问题
{
	资源冗余: 资源冗余往往是很多项目中最为常见的内存问题之一。而其中往往【AssetBundle打包策略】和【资源加载缓存策略】又是导致冗余的最主要的两种原因
	{
		bundle资源冗余：同样的资源被打了两份造成内存浪费 也会加载多次造成加载时间边长
	}
	代码生成的资源: 运行时通过代码接口生成某些暂时性资源是非常常见的做法，但使用不当也很容易产生性能问题。
	加载和缓存策略:
}


]==]

-----------------------------------------------------<<<<<< 内存优化 >>>>>-----------------------------------------------------------------------------
--[====[


	--------------------------------------Gfx内存优化---------------------------------------------------------
	--[==[

		纹理资源内存优化
		{
			纹理资源是最常用、也是很多项目中占内存大头的一种资源。它还和项目的渲染模块CPU、GPU性能有很大关联。本节主要从内存角度探讨纹理的格式、分辨率、Read/Write Enabled、Mipmap

			压缩格式
			{
			 	平常存在磁盘上的都是jpg或者png的压缩格式，这些格式只能用于减少图片的磁盘占用空间，无法被GPU识别和读取，
			 	所以无论这些图片在电脑种以什么样的格式存储，在导入Unity的时候都会经过import这个过程转换成纹理的格式（ETC或者 ASTC 能直接被Mobile GPU直接读取）

			 	纹理压缩的目的
			 	{
					节省内存
					减少带宽
					降低加载纹理消耗
			 	}

			 	设备平台不支持的格式Unity会自动回退到RGBA格式

			 	ETC2_8bit 与 ATSC4X4 压缩后大小相同
			 	ETC2_4bit 与 ATSC6X6 压缩后大小相同

			}

			MipMap
			{
				优化远处物体的表现效果
				减少Cache Miss, 减少带宽

				缺点是内存增加1/3

				建议
				{
					2d UI，摄像机距离固定，没有远近变化，应关闭
					3d UI以及其他作用3d渲染的纹理，建议开启
				}
			}

			Texture Quality
			{
				可以显著改变纹理的内存占用==》通过减少加载进内存的Mipmap层数来减少纹理的占用，只针对开启Mipmap的纹理生效
				Editor——Project Settings —— Quality —— Textures
				{
					Full Res :加载所有的Mipmap层级
					Half Res ：排除Mipmap0，其他层级都加载
					Quarter Res: 排除Mipmap0 Mipmap1，其他层级都加载
				}

				可以用来画质分级


			}
			Texture Streaming
			{
			
			}
		}

		网格资源内存优化
		{
			网格资源也是非常常用、重要性不逊于纹理一种资源。它同样和项目的渲染模块CPU、GPU性能有很大关联。本节主要从内存角度探讨网格的顶点数和面片数、顶点属性、Read/Write Enabled......等常见设置对性能的影响。

			Read/Write
			{
				开启后，Unity不仅会把网格数据上传到GPU的内存中，还是在CPU里保留一份，这样系统内存就会有2分网格数据
				关闭后，Unity会把网格数据上传到GPU内存后，从CPU内存中删除对应的网格数据

				需要开启的特殊情况
				{
					开启了Mesh Collider(网格碰撞体)
					游戏中需要使用代码修改模型
				}
			}
				

			顶点属性
			{
				网格的顶点数据有很多种，Position Normal，Tangent(只有法线贴图需要用到)，UV0，UV1
				但是在渲染的时候使用的Shader并不一定用到所有种类的顶点属性，多余的属性就会造成不必要的内存浪费

				Tangent属性可以在Mesh的inspect面板中设置
				Project Settings/Player/Optimize Mesh Data勾选后Unity会自动去掉未使用的顶点属性，可能有bug，最好测试下
			}

			骨骼
			{
				静态物体可以去掉骨骼
			}

			静态合批
			{
				多个小网格合并成一个大网格，内存增加
			}
		}


		Shader资源内存优化
		{
			Shader资源作为一种非常重要的渲染资源，直接影响相关渲染对象和DrawCall的GPU开销。本节主要从内存角度探讨Shader资源内存的控制。

			shader的内存优化主要是从变体上着手，可以通过脚本剔除变体以及手动注释关键字
			shader的变体数量事影响内存占用的重要因素

			脚本剔除变体
			{
				Unity提供IPreprocessShader这个接口，可以继承实现OnProcessShader 去除不必要的变体，可以参考shader变体优化那篇笔记

			}

			手动注释关键字
			{
				明确知道一些关键字不会使用就直接注释掉
			}
		}

	]==]


	--------------------------------------Reserved Unity 内存优化---------------------------------------------------------

	--[==[
		Render Texture资源内存优化
		{
			Render Texture同样和GPU性能有很大关联。一般而言，如渲染分辨率、抗锯齿、后处理等设置，都可能同时影响GPU性能和内存占用

			抗锯齿
			{
				MSAA开启后，内存增加明显

			}

			阴影分辨率
			{
				移动端1024就够了
			}

			深度
			{
				深度有好几种，0位 16位 24位，不同位数的资源内存也是不同的
				16位比0位大一半
				24位比16位大一半
				24位比0位大一倍

			}

			HDR格式
			{
				HDR有两种格式选择：FP16格式，R11G11B10格式 两种格式占用内存不同
				
				R11G11B10格式： R通道11位数据，G通道11位数据，B通道10位数据，总共32位，所以不使用alpha通道情况下够用
				FP16格式：其实是ARGB Half 格式带有Alpha通道，每个通道用16位数据存储，总共64位数据，内存占用比R11G11B10格式大一倍

				所以在不需要使用alpha通道的情况下尽量先用R11G11B10格式

			}

		}


		动画资源内存优化
		{
			动画资源内存占用一般相对较小，但仍要关注其中占用较大、时间却相对短的资源，对动画类型、动画压缩算法、动画精度等因素进行适当调整，在动画表现和内存之间做出合理权衡

			Resample Curves
			{
				Unity默认会对动画开启Resample Curves，把动画曲线重新采样为用四元数表示的曲线，模型导入可以在Animation面板关闭，默认是开启的
				开启后关键帧的数量会变少一些，内存会小点
			}

			动画压缩
			{
				Anim.Compression ==> 可以选择动画的压缩方式
					Keyframe Reduction/Optimal, 压缩有可以显著减少动画的内存占用


				Keyframe Reduction
				{
					减少关键帧数量，可以通过调整error参数来控制压缩的程度（百分数，0.5代表0.5%）
				}

				Optimal
				{
					根据启发算法减少关键帧数量或者改变曲线的存储格式
					
					存储格式：
					{
						Constant (直线，常数) 占用最小
						Dense （无切线）
						Stream （有切线） 占用最大
					}

					使用Optimal压缩的时候会把一部分Stream格式存储的曲线改为用Dense或Constant的格式存储，从而减少内存占用
				}
			}

			剔除Scale曲线
			{
				大部分带有骨骼的动画骨骼的尺寸是不会变的，因此并不需要Scale曲线，可以剔除（但是骨骼动画里都带有scale为1的数据，没有啥意义）
				
				如何剔除
				{
					Unity在导入模型的时候有个选项 Animation选项卡Remove Constant Scale Curve，勾选就可以去掉
					但是这个功能不能去掉所有的scale曲线

					可以使用代码去除， scale数据都是常数，去掉后内存变化不大
				}
			}

			降低精度
			{
				降低精度--曲线变直线--存储格式变成Constant--内存下降

				Unity默认存储每一帧的信息用的是10位精度的Float格式数据，可以用脚本变成3位精度

				单纯降低精度并不会降低内存，因为在内存中每个float占用的大小仍然是一样的，但是降低精度后会是动画曲线发生变化，细微的起伏的曲线就变成直线了
				因此在Optimal的压缩格式下存储格式变成Constant了，降低了内存占用

				需要考虑降低精度后动画的表现效果是否会受影响
			}
		}



		音频资源内存优化
		{
			Force To Mono
			{
				在Inspect面板中开启后，Unity会把音频的双声道混合成单声道
			}

			LoadType
			{
				音频的loadtype也对内存有较大的影响

				Decompress On Load : 音频文件加载后就会解压缩，以未压缩的形式存在内存中，内存占用较大 ==》 适用频繁重复的短音频（如枪声）
				Compressed In Memory：音频文件以压缩的形式存在内存中，播放时解压  内存占用小一些 ==》 适用大部分音频
				Streaming : 播放时从磁盘中一遍读取一遍解压缩，使用最少量的内存来缓冲  内存占用最小，但会带来cpu的开销 ==》 适用较长的长音频（如背景音乐）
			}

			Compression Format（压缩格式）在Inspect面板中
			{
				Unity中有4中音频压缩格式
				{
					PCM：未经过任何压缩
					ADPCM：压缩格式
					Mp3：压缩格式
					Vorbis：压缩格式
				}
			}
		}





		字体资源内存优化
		{
			字体资源内存占用一般相对较小，但仍需关注单个过高的资源，可以予以精简。另外，适当情况下制作静态字体并解除依赖，可以直接避免字体资源的内存占用。

			字体瘦身
			{
				字库一般比较大，项目中使用的可能只占一部分
				字体精简工具：FontPruner，FontSubsetGUI
			}

			压缩字体纹理
			{
				使用Unity中的TextMeshPro从原始的TTF字体生成SDF字体
				此时字体变成纹理形式，但不可更改==》 使用代码将字体纹理提取出来，然后对纹理进行压缩格式设置
			}
		}



		粒子系统资源内存优化
		{
			粒子系统资源内存占用受项目粒子特效使用量和缓存策略影响大。除了控制粒子系统本身的使用量外，还需要细致排查不够规范的美术流程和不够合理的缓存策略导致的不必要开销。

			粒子数量
			{
				实际播放的粒子数量有关，跟最大粒子数量无关，实际播放粒子数量为0都会有小部分内存占用
			}
			未播放的粒子
			{
				未被使用的粒子系统导致内存浪费（即使粒子系统未被播放也会暂用内存）

				场景中弃用的粒子系统及时删除
				缓存的粒子系统不要超过实际需要的数量太多，会有使一些缓存的粒子系统永远都播放不到
			}
		}

	]==]


	--------------------------------------托管堆内存优化---------------------------------------------------------
	--[==[

		结合Mono堆内存分配堆栈和曲线，排查单次高分配和持续分配型的函数堆栈情况，从而优化堆内存峰值过高、GC频率过高等堆内存分配引发的性能问题。

		驻留内存过高
		{
			创建了很多 使用的少
		}

		持续分配内存过高
		{
			每一帧都创建mono内存，比如创建数组，可以使用一个变量提前创建，然后更新
			也可以创建内存一次重复使用
		}

	]==]


]====]

-----------------------------------------------------<<<<<< 内存优化 >>>>>-----------------------------------------------------------------------------



-----------------------------------------------------<<<<<< 降低动画模块耗时 >>>>>-----------------------------------------------------------------------------
--[====[
	--------------------------------------Mecanim动画---------------------------------------------------------

	--[==[
	Mecanim动画造成的性能压力主要体现在CPU端的耗时上，它的主要耗时函数是PreLateUpdate.DirectorUpdateAnimationBegin和PreLateUpdate.DirectorUpdateAnimationEnd。
	针对Mecanim动画耗时的优化主要是优化这两个函数的耗时，确认导致这两个函数耗时较高的原因。一般而言，动画系统顶层函数的自身耗时占比是较低的，所以优化的重点在于它的子堆栈的耗时。

	Active Animator数量 （Culling mode）
	{
		Active Animator指的是在场景中会造成Animator.ApplyOnAnimatorMove调用的Animator对象，它的数量越多，Mecanim动画的耗时也就越高。
		建议将Animator的Culling Mode设置为Cull Update Transform，这样当Animator所控制的对象不在任何相机的可见范围内时，
		Unity会停止更新该动画对象的Retarget、IK和Write Transform也就是写回骨骼节点的Transform，但是仍然是会更新动画状态机和根运动。
		但是需要注意的是，如果是用于UI动画的Animator，则其Culling Mode一定要设置为Always Animate，否则会有表现上的错误
	}


	Optimize Game Objects
	{
		Optimize Game Objects是针对骨骼节点数量较多的模型给出的优化项，勾选上之后可以【【减少骨骼节点的Transform回传C#端的耗时】】。
		它的耗时主要体现在Animator.WriteJob函数下，当Animator.WriteJob函数的耗时占比较高时，需要确认场景中的模型的Optimize Game Objects选项是否有勾选上。
	}

	 Apply Root Motion
	 {
		Apply Root Motion的选项只有在动画播放过程中需要位移的对象才有必要勾选，关闭它可以节省部分耗时。
		它的耗时主要体现在【【Animator.ApplyBuiltinRootMotion函数】】，当该函数的耗时占比较高时，需要确认场景中Animator对象是否都需要产生位移。
	 }

	 Compute Skinning
	 {
		Compute Skinning选项是指使用GPU来计算骨骼动画的蒙皮的选项，但是实际上勾选上后性能表现会有所下降，不建议勾选。
	 }

	 Animator.Initialize
	 {
		Animator.Initialize是指Animator所在的对象被激活时会触发的调用，需要确认它的调用频率和耗时情况是否合理。
		【【Animator.Initialize会在含有Animator组件的GameObject被Active或Instantiate时触发，耗时较高】】，
		因此在战斗中不建议过于频繁地对含有Animator的GameObject进行Deactive/ActiveGameObject操作，可以改为Disable并且移除Animator组件的方式来进行Animator动画的激活和禁用。
	 }

	]==]






	--------------------------------------Legacy动画---------------------------------------------------------

	--[==[

		Legacy动画是Unity的老的动画系统，目前也还有一些适合它的场景，比如说用于一些简单的UI动画等。它的主要耗时函数是PreLateUpdate.LegacyAnimationUpdate，一般而言，优化的重点也是它的子堆栈的耗时



		Animation.Sample的调用次数显示了场景中实际在更新的Animation对象的数量，而它的父节点Animation.Update的调用次数则是显示了场景中存在的Animation对象的数量。
		因此，优化Legacy Animation动画耗时则是要减少Animation.Sample的调用次数。


		Culling type
		{
			Always Animate: 一直更新
			Based On Renderers: 视椎体外就不更新了
		}

		激活实例化
		{
			激活一个带有Animation组件的物体时，会触发Animation.RebuildInternalState这个函数，操作非常频繁时会带来较高的耗时
			可以禁用组件以及改变位置（改变缩放也行）的方式来替代激活
		}

		AddClip
		{
			重复执行会产生不必要的耗时
		}


	]==]

]====]

-----------------------------------------------------<<<<<< 降低动画模块耗时 >>>>>-----------------------------------------------------------------------------




-----------------------------------------------------<<<<<< 降低物理模块耗时 >>>>>-----------------------------------------------------------------------------
--[====[

	Unity物理系统的性能瓶颈主要体现在【【CPU端的耗时】】，它的主要耗时函数为【【FixedUpdate.PhysicsFixedUpdate】】。
	在开启Physics设置时，它的主要耗时堆栈是【【Physics.Processing和Physics.Simulate】】，需要针对这两个函数进行优化。
	【【首先会影响这两个函数耗时的是它们的调用次数】】，调用次数越多则耗时也就越高，而物理函数的调用次数受到Time设置里Maximum Allowed Timestep和Fixed TimeStep的影响会存在一个调用次数上限。
	其中，Maximum Allowed TimeStep决定fxf了单帧物理最大调用次数，该值越小，单帧物理最大调用次数越少；Fixed TimeStep决定了FixedUpdate的更新间隔，该值越大，每帧物理更新调用次数越少。
	此外，当游戏陷入卡顿，帧耗时较高时，物理函数的调用次数也会随之增加。

	物理模块的开销：CPU耗时以及堆内存
	{
		CPU耗时
		{
			FixedUpdate.PhysicsFixedUpdate
			{
				Physics.Processing
				Physics.Simulate
			}

			逻辑代码
			{
				OnTriggerEnter,OnCollisionEnter等碰撞事件
				Raycast，OverLap等检测函数
			}
		}
		

		堆内存
		{
			OnTriggerEnter等回调会生成Collision的实例，它被分配到堆内存，产生GC
			Raycast等检测函数会将多个返回物体实例分配到堆内存，产生GC
		}
	}

	简单描述
	{
		Collision的产生
		{
			了解Collision的产生条件，哪些碰撞体之间会产生Collision。每当产生Collision，Unity对象会在OnTriggerEnter、OnCollisionEnter等函数中收到碰撞事件的相关信息并执行其中的相关逻辑。
			因此有必要确认当前Collision的产生情况是否符合预期，是否存在不必要的Collision。
		}


		Trigger的替代方案
		{
			Collider.Bounds实现替代Trigger，避免使用Unity的物理模块。T
			rigger触发是比较方便的能够使用非物理模拟的方式来进行替换的一种Collision，使用C#逻辑来替代掉Trigger可以降低部分物理模块的耗时
		}


		Physics Layer的设置
		{
			Physics Layer中取消不必要的层之间的碰撞检测，避免多余的Contacts的产生。
		}

		物理更新次数
		{
			Time设置会影响到物理更新次数的上限，而具体的物理更新次数会受到帧耗时的影响。
		}

		Auto Simulation
		{
			不需要使用物理模块时直接关闭Auto Simulation以节省物理模拟的耗时，而如果需要使用射线检测时只需要开启Auto Sync Transform即可。
			在使用NGUI时也是可以这么做的，但是开启了Trigger和Collision的粒子系统要想有正常的物理表现则不可以关闭Auto Simulation
		}

		RaycastCommand
		{
			射线检测在场景中碰撞体数量较多时同样会产生较高的CPU端耗时，可以使用Unity提供的RaycastCommand来进行Job化的异步射线检测，减少主线程的耗时。
		}

	}

	物理模拟优化
	{
		Unity中的物理系统以固定的时间间隔允许，默认是0.02秒，
		如果两帧之间的时长大于这个间隔，那它会调用更多次的物理更新来实现
		也就是说可能会调用多次FixedUpdate.PhysicsFixedUpdate,它发生在一帧中靠前的位置

		Unity会自动模拟这些物理更新，带来物理方面的开销
		Unity2017.4后出现Auto Simulation选项，默认为开启状态
		在Editor>Project Settings > Physics中开启或关闭 
		使用脚本关闭：Physics.autoSimulation

		如果项目没有物理模块就不需要开启


		自动同步几何信息 Auto Syns Transform
		2017.2版本后该选项是默认关闭的，
		开启该选项会使每次Transform属性发生变化时，强制与物理系统进行同步，这样会增加物理运算负担
		关闭该选项时，仅在FixedUpdate的物理模拟步骤前进行同步，也可以使用Physics.SyncTransform手动同步变换更改
	}

	物理碰撞优化（物理碰撞跟物理模拟不是一个概念）
	{
		物理碰撞只是物理对象之间发生的Collision与Overlap的回调，并不会对后续的反馈做物理模拟
		物理碰撞需要添加Collider来管理碰撞事件
		物理模拟需要添加Rigidbody来实现基于物理的行为，比如移动 重力 碰撞


		###尽量不要使用Mesh Collider, 可以用多个简单的碰撞体支付复合碰撞体
		###如果一定要使用Mesh Collider,建议开启Play Setting中的Prebake Collision Mesh选项，可以在构建时预先将数据烘焙，不会在运行时带来开销
		
		开启Trigger选项后物理不会表现为实体对象，不会发生碰撞而会直接穿过

		###如果只想检测物体是否碰撞，并且不需要做任何的碰撞模拟效果,使用Trigger会比Collider更高效
		还可以利用Collider.Bounds的检测，使用C#逻辑来代替Trigger的检测，也能降低部分物理模块的耗时

		Rigidbody组件
		{
			Rigidbody组件是实现游戏对象物理行为的主要组件
			添加组件后，对象会立刻相应重力，一般同时还会添加碰撞体组件，这样会因为碰撞而移动

			优化建议
			{
				刚体组件会接管该游戏对象的运动，因此不建议直接对Transform属性做修改，这会导致物理世界中重新计算
				应该使用MovePosition或AddForce函数之类的物理方法来移动对象

				最好在FixedUpdate中移动而不是在Update中
			}

			Rigidbody的Is Kinematic属性
			默认不开启的情况下Rigidbody完全由物理引擎模拟来控制
			开启Is Kinematic后会让物体摆脱物理引擎的控制，允许脚本进行控制
			Kinematic对象不会相应的碰撞或力，但仍然会对其他刚体对象施加物理影响

			静态碰撞体：只有Collider组件没有Rigidbody组件
			刚体碰撞体：有Collider组件有Rigidbody组件，但是Rigidbody的Is Kinematic选项未开启
			运动刚体碰撞体：有Collider组件有Rigidbody组件，Rigidbody的Is Kinematic选项开启
		}
		
	}

	物理更新次数优化
	{
		Unity中的物理以固定的时间间隔运行，这对于模拟的准确性和一致性非常重要
		在每一帧开始时，Unity会根据前一帧花费的时长，执行尽可能多的FixedUpdate，以赶上当前时间
		一般情况下，执行的物理更新次数越多，物理开销越大

		优化建议
		{
			先优化其他模块，以降低每一帧的耗时，从而减少物理的更新次数
			调整Unity中Time的相关次数：Fixed Timestep以及Maximum Allowed Timestep
			{
				Fixed Timestep 确定了执行物理计算和FixedUpdate()事件的时间间隔
				该值越大，每帧调用的物理更新次数就会越少，但物理计算的频率也会降低
				物理计算频率过低可能会造成部分机制异常，比如检测不到碰撞的回调

				优化建议：在可接受范围内尽可能调高Fixed Timestep

				Maximum Allowed Timestep：执行物理计算和FixedUpdate()事件的时间长度不会超过该值
				该值越小，单帧物理最大调用次数越小，但是卡顿时物理可能有问题
				优化建议：一般设置在8~10个FPS之间

			}
		}
	}

	堆内存优化
	{
		碰撞回调的GC开销
		{
			OnCollisionEnter/Stay/Exit会将结果返回生成新的实例分配到堆内存中，因此会触发GC
			优化建议：开启Physics设置中的Reuse Collision Callbacks，碰撞回调会同时使用同一个Instance，减少GC
		}

		检测函数造成的GC开销
		{
			Raycast, boxCast，overlapBox等函数会将返回结果的实例分配到堆内存，因此会触发GC

			优化建议：使用对应的Non-alloc版本的函数，如RaycastNonAlloc,BoxCastNonAlloc,OverlapBoxNonAlloc等，需要预先分配一个较大的容器来存储返回结果，避免了持续的堆内存分配
		}
	}

	其他优化
	{
		可以对单个Rigidbody的solverIteration属性进行设置，该值越大模拟越精准，但是开销越大

		射线检测在场景中碰撞体数量较多的时同样会产生较高的CPU端耗时
		使用RaycastCommand来进行job话的异步检测，减少主线程的耗时
	}


]====]



-----------------------------------------------------<<<<<< 降低物理模块耗时 >>>>>-----------------------------------------------------------------------------




-----------------------------------------------------<<<<<< UGUI热点函数 >>>>>-----------------------------------------------------------------------------

--[====[
	Canvas.SendWillRenderCanvases
	{
		该函数的耗时代表的是UI元素自身的变化带来的更新耗时，可以理解为UI更新的耗时。
		主要是会受到场景中发生属性变化的UI元素的数量以及该UI元素的复杂度的影响。可以考虑减少更新频率，比如隔帧更新等。
	}

	BuildBatch & EmitWorldScreenspaceCameraGeometry
	{
		Canvas.BuildBatch为UI元素合并的Mesh需要改变时所产生的调用。而EmitWorldScreenspaceCameraGeometry为ReBuild时主线程产生等待的耗时
	}

	SyncTransform
	{
		对于UI元素调用SetActive（false改成true）会导致Canvas下所有的UI元素触发SyncTransform，从而导致较高的耗时。
	}

	EventSystem.Update
	{
		EventSystem组件主要负责处理输入、射线投射以及发送事件、UI的创建会自动创建相关组件处理UI点击事件
	}


]====]

-----------------------------------------------------<<<<<< UGUI热点函数 >>>>>-----------------------------------------------------------------------------

-----------------------------------------------------<<<<<< UGUI DrawCall优化 >>>>>-----------------------------------------------------------------------------

--[====[
	合并图集
	{
		尽量整合并制作图集，从而使得不同U元素的材质图集一致。图集中的按钮、图标等需要使用图片的比较小的UI元素，完全可以整合并制作图集。当它们密集地同时出现时，就有效降低了DrawCall
	}

	重叠打断合批
	{
		在同一Canvas下、材质和图集一致的前提下，要避免重叠时的层级穿插。简单概括就是，应使得符合合批条件的UI元素的“层级深度”相同;
	}

	Z！= 0
	{
		当UI元素的Z！=0时，也会产生合批被打断的情况
	}

]====]

-----------------------------------------------------<<<<<< UGUI DrawCall优化 >>>>>-----------------------------------------------------------------------------



-----------------------------------------------------<<<<<< 加载相关函数 优化 >>>>>-----------------------------------------------------------------------------

--[====[
	Loading.UpdatePreloading
	{
		Shader解析和编译
		{
			Shader的解析和编译耗时一般是指，
			在Shader资源被加载进内存后触发的【【Shader.Parse()和Shader.CreateGPUProgram】】两种API的耗时。
			忽视Shader资源加载策略的管理，往往会导致游戏过程中触发以上两种API的不必要耗时峰值。

			如果shader没有单独打包，会在加载assetbundle多次加载，就会多次触发Shader.Parse()和Shader.CreateGPUProgram，造成不必要的耗时

			shader最好单独打包
			将Shader和AB包进行缓存（都不能卸载）


		}

		Resources.UnloadUnusedAssets
		{
			Resources.UnloadUnusedAssets为Unity遍历所有资源的引用情况并卸载Unused对象的API，
			一般在场景切换时由Unity自动触发或由开发者手动调用。往往因其自身底层机理和使用不当造成不必要的耗时峰值。

			Unity在切换场景的时候，Unity会自动调用Resources.UnloadUnusedAssets这个函数来卸载场景中没有被使用的函数，这个函数的单次调用比较高，一般不建议手动调用
			当调用这个函数的时候，会对每一个资源都遍历一次Hierarchy中的GameObject以及堆内存中的对象，判断这个资源是否被GameObject或者组件使用，因此这个函数的单次耗时
			随着GameObject与Mono对象数量之和乘以Asset数量的乘积的变大而变大
		}

		异步加载优先级
		{
			异步加载是很多项目中场景切换时加载资源的做法，但往往受Application.backgroundLoadingPriority这一API的默认设置限制而效率低下

			https://www.jianshu.com/p/a3f252da84e6
			Application.backgroundLoadingPriority 
			{
				后台加载线程的优先级 可用于控制异步加载数据所需的时间以及在后台加载时对游戏的性能影响

				加载对象（Resources.LoadAsync、AssetBundle.LoadAssetAsync、AssetBundle.LoadAllAssetAsync）、场景 (SceneManager.LoadSceneAsync) 的异步加载函数
				在【【单独的后台加载线程中执行数据读取和反序列化，并在主线程中执行对象集成】】。 “集成”取决于对象类型，纹理和网格意味着将数据上传到 GPU，音频剪辑可准备数据以进行播放。

				为了防止卡顿，会限制主线程上执行的时间：
				- ThreadPriority.Low - 2ms;
				- ThreadPriority.BelowNormal - 4ms;
				- ThreadPriority.Normal - 10ms;
				- ThreadPriority.High - 50ms.

				这是异步操作可以在主线程的【【单帧花费】】最长时间
				单帧花费时间越多，可加载的数据越多，因此帧率将有所下降，较为影响游戏性能，但可减少加载资源的时间，能更快的进入游戏
				反之，单帧花费时间越少，可加载的数据越少，对游戏的游戏性能影响越小，可在游戏进行时有很好的后台加载

				异步加载时处于战斗场景：设置调高会增加主线程耗时，可能影响性能
				异步加载时处于加载界面：建议设置调高，尽量缩放加载时间  可以设置ThreadPriority.High
				
			}
			
		}
	}

	合理使用加载的API
	{
		加载和卸载AssetBundle
		{
			使用AssetBundle加载资源是目前移动端项目中比较常见和普遍的做法。而其中对于压缩格式、加载API、加载策略的选用也会一定程度上影响性能

			加载和卸载AssetBundle的API
			{
				LoadFromFile：用于从本地加载AB包
				LoadFromStream：用于AB包需要加密的情况
				DownLoadHandlerAssetBundle：用于从网络下载AB包（热更新）
				{
					using UnityEngine;
					using UnityEngine.Networking; 
					using System.Collections;
					 
					class MyBehaviour: MonoBehaviour {
					    void Start() {
					        StartCoroutine(GetAssetBundle());
					    }
					 
					    IEnumerator GetAssetBundle() {
					        UnityWebRequest www = new UnityWebRequest("http://www.my-server.com");
					        DownloadHandlerAssetBundle handler = new DownloadHandlerAssetBundle(www.url, uint.MaxValue);
					        www.downloadHandler = handler;
					        yield return www.Send();
					 
					        if(www.isError) {
					            Debug.Log(www.error);
					        }
					        else {
					            // Extracts AssetBundle
					            AssetBundle bundle = handler.assetBundle;
					        }
					    }
					}
				}
			}
			
		}

		加载和卸载资源
		{
			使用Resources或AssetBundle的API加载资源本身并没有难度，但仍需要分别关注同步加载和异步加载情况下的一些策略导致的性能问题

			打ab包的时候设置压缩算法会导致加载速度不一样
			{
				BuildAssetBundleOptions.None ==》 使用LZMA算法压缩
				BuildAssetBundleOptions.ChunkBaseCompression==》使用LZ4算法压缩

				LZMA：stream-based（流式压缩），只支持顺序读取，加载需要将整个包解压
				LZ4：chunk-based（块压缩），支持随机读取，加载速度快 （会先加载bundle头部到内存，使用哪个资源就把对应资源加载到内存）
			}

			StreamAssets文件压缩，默认不压缩
			
		}

		实例化和销毁对象
		{
			
			频繁大量的实例化和单次实例化过长都是可能困扰开发者的性能问题，而【【缓存池、分帧加载】】等策略和技巧可能获得良好的优化效果。
		}

		激活和隐藏对象
		{
			
			激活和隐藏的耗时本身不高，但如果单帧的操作次数过多就需要予以关注。可能出于游戏逻辑中的一些判断和条件不够合理，
			很多项目中往往会出现某一种资源的显隐操作次数过多，且其中SetActive(True)远比SetActive(False)次数多得多、或者反之的现象，亦即存在大量不必要的SetActive调用。

			SetScale代替，
		}
	}

]====]

-----------------------------------------------------<<<<<< 加载相关函数 优化 >>>>>-----------------------------------------------------------------------------