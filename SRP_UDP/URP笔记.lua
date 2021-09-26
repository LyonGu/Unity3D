
--[==[
	1 RenderTargetHandle 创建
	{
		RenderTargetHandle m_CameraColorAttachment
		m_CameraColorAttachment.Init("_CameraColorTexture"); // 名字随便取，只是最后使用m_CameraColorAttachment才创建的Rt，代表rt的名字
	}

	2 RenderTextureDescriptor 创建以及获取  //RT描述文件
	{
		Feature的OnCameraSetup方法
		{
			public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
			{
				RenderTextureDescriptor blitTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor; //RT描述文件
			}
		}
		
	

		pass的Configure方法 参数里直接获取
		{
			public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
		    {
	
		        m_OpaqueDesc = cameraTextureDescriptor;
		    }

		}
	}

	3 RenderTargetIdentifier 创建以及获取 ==》 cmd.blit时候要使用 RT目标文件
	{
		int sourceId = Shader.PropertyToID("XXXX"); //
		cmd.GetTemporaryRT(sourceId, blitTargetDescriptor, filterMode); //创建一张RT
		RenderTargetIdentifier source = new RenderTargetIdentifier(sourceId); //把RT绑定到RTTarget对象上
	}

	4 创建一个RT
	{
		int sourceId = Shader.PropertyToID("_SourceTexture"); // 任一名字

		//blitTargetDescriptor 为一个RenderTextureDescriptor对象
        cmd.GetTemporaryRT(sourceId, blitTargetDescriptor, filterMode); //创建一张临时RT


		//m_ActiveCameraColorAttachment为一个RenderTargetHandle对象，必须调用过RenderTargetHandle.Init
        cmd.GetTemporaryRT(m_ActiveCameraColorAttachment.id, colorDescriptor, FilterMode.Bilinear);
	}

	5 两种blit方法
	{
		pass的Blit方法 -》 需要的是RenderTargetIdentifier对象  也可以传一个id进去 RenderTargetIdentifier有个构造方法支持init类型
		{
			public void Blit(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, Material material = null, int passIndex = 0)
	        {
	            ScriptableRenderer.SetRenderTarget(cmd, destination, BuiltinRenderTextureType.CameraTarget, clearFlag, clearColor);
	            cmd.Blit(source, destination, material, passIndex);
	        }

	        Blit(cmd, m_InternalLut.id, m_InternalLut.id, material);
		}

		CommandBuffer的Blit方法，既可以传RenderTargetIdentifier也可以传id
		{
			int blurredID = Shader.PropertyToID("_BlurRT1");
            int blurredID2 = Shader.PropertyToID("_BlurRT2");

            //获得两张RT  using过后会自动调用 cmd.ReleaseTemporaryRT?
            cmd.GetTemporaryRT(blurredID, m_OpaqueDesc, FilterMode.Bilinear);
            cmd.GetTemporaryRT(blurredID2, m_OpaqueDesc, FilterMode.Bilinear);

            cmd.Blit(blurredID2, blurredID, m_BlurMaterial);
		}

		**** RenderTargetIdentifier类型参数都可以用 int类型代替，一个id就行
		cmd.SetGlobalTexture("_GrabBlurTexture", this.blurTemp1.id);
	}

	6 设置渲染器配置文件
	{
		//hight可以直接头拖到对应的脚本上
		GraphicsSettings.renderPipelineAsset = hight;
        QualitySettings.renderPipeline = hight;
	}

	7 如果一个场景里具有摄像机组 MainCamera相机，UI相机 Blur相机
	{
		MainCamera 为Base类型
		UICamera 为OverLay
		BlurCamera 为OverLay

		MainCamera的stack里为UICamera和BlurCamera
		
		blit到frameBuffer只会在最后一个相机里执行（BlurCamera）

	}

	7 每次切换rendertarget都会出现切换出去的rendertarget发生aa的resolve操作  rendertarget的resolveAA
	{
		https://github.com/sienaiwun/Unity_AAResolveOnCommand

		LWRP,URP模板使用RenderTargetIdentifier进行RT的切换，这个类很难设置resolve的频率，但是RT也可以由RenderTexture设置，
		这个类的创建时候的bindMS参数来控制是否自动进行RT的resolve和ResolveAntiAliasedSurface手动控制resolve的操作。具体修改可见resolve rendertarget on command修改。
		存储上，本修改在使用msaa的rendertarget通过开启bindms设置增加一个带msaa的rendertexure(m_color_handle)和一个不带msaa的rendertexture(m_resolve_handle)。
		看上去增加一个rt，但是在unity本身中如果关闭bindms,只用一个rt内部也会有两个rendertexutre handle,一个带msaa的texture2dMS，一个不带msaatexture2d。所以概念上是等同的。
	}

	8 后期组件 Volume ==》 可以拿到stack 后直接获取对应组件
	{
		 var stack = VolumeManager.instance.stack;
		 stack.GetComponent<DepthOfField>()
		 stack.GetComponent<MotionBlur>()
	}

]==]

--[==[
	RT理解:
	{

		1 RT是什么，用在哪
		{
			首先rt是一张特殊贴图，这张贴图对应的是GPU上的FrameBuffer，一般用到的是颜色和深度，从这张图取数据用于计算，或是直接对这张图进行修改，以得到想要的效果。
			FrameBuffer就是gpu里渲染结果的目的地，我们绘制的所有结果（包括color depth stencil等）都最终存在这个这里。现代GPU通常都有几个FBO，实现双缓冲，以及将渲染结果保存在GPU的一块存储区域，之后使用。
			RT的应用主要是几个方式，一是从rt取数据，比如取深度用于各种计算。二是取这张图，比如在UI上显示模型，或是截图保存等。再一个就是是对图进行处理，实现扭曲或是其他全屏效果。
			一般来说，不使用rt也是可以渲染出想要的图的，但是drawcall就会很多，unity内置管线，获取深度就是渲染一遍物体，这必然比直接取buffer消耗要大很多。
			但是从GPU拷贝数据回CPU，需要硬件支持，unity提供了CopyTextureSupport接口判断支持的拷贝方法，按硬件的发展速度应该不支持的会越来越少的。
		}

		2 unity接口
		{
		  	unity对rt的抽象是RenderTexture这个类，定义了一些属性，包括大小，精度等各种
		  	RenderTargetIdentifier
		  	{
				unity CoreModule实现，定义CommandBuffer用到的RenderTextur
				封装了texture的创建，因为texture创建方式有几种，rt，texture，BuiltinRenderTextureType，GetTemporaryRT
				这个类只是定义了rt的各种属性，真正创建应该是在CommandBuffer内部
				BuiltinRenderTextureType类型：cameratarget、depth、gbuffer等多种
				CommandBuffer.SetRenderTarget，可分别设置color和depth，以及贴图处理方式
				CommandBuffer.SetGlobalTexture，效果是把texture赋值给shader变量，shader采样这个texture
		  	}

		  	RenderTextureDescriptor
		  	{
				封装创建rt需要的所有信息，可复用，修改部分值，减少创建消耗
		  	}

		  	RenderTargetHandle
		  	{
				URP对RenderTargetIdentifier的一个封装
				保存shader变量的id，提升性能，避免多次hash计算
				真正用rt的时候，才会创建RenderTargetIdentifier
				定义了一个静态CameraTarget
		  	}
		}

		3 URP用法
		{
			URP不会直接用到rt，而是通过CommandBuffer的接口设置，参数是RenderTargetIdentifier
			CoreUtils封装了SetRenderTarget方法，ScriptableRenderer调用
			ScriptableRenderPass，封装ConfigureTarget方法，可以设置color和depth
			pass设置好color和depth的rt后，renderer执行ExecuteRenderPass函数时读取，并设置给cb
			pass内部会根据需要设置color和depth的渲染内容


			总结下来是由pass决定要渲染到哪个rt，以及用什么方式。然后renderer调用CoreUtils设置，设置好后调用pass的Execute方法渲染。

		}
	}

	RT应用--深度纹理获取
	{
		URP提供了两种获取深度图的方法，一种是像内置管线那样，直接渲染指定pass，另一种是取深度buffer，渲染到一张rt上，优先用取深度buffer的方法，效率更高，但是需要系统和硬件支持。

		1 先看下什么情况会生成深度图
		{
			主动开启，在PipelineAsset选择DepthTexture
			渲染scene相机，固定开启，并用DepthOnly的方式获得
			对于game相机，后处理，SMAA抗锯齿，DOF，运动模糊，用到了一个就会自动开启。判断在UniversalRenderPipeline的CheckPostProcessForDepth方法。
		}

		2 获取深度纹理的方法一：取深度buffer，CopyDepthPass
		{
			CanCopyDepth函数判断当前环境是否可开启。现在看起来需要系统支持拷贝深度贴图并且不能开启MSAA，看注释之后的版本会支持MSAA
			这个pass一般在不透明渲染之后执行。看代码对scene相机执行时间不同，可是现在scene不会用这个渲染，可能是给以后留的吧。

			实现方法?????
			{
				FrowardRender.SetUp：设置源rt关联到shader的_CameraDepthAttachment。目标rt关联到_CameraDepthTexture。
				CopyDepthPass.OnCameraSetup：设置目标rt格式，colorFormat为Depth，32位，msaa为1不开启，filterMode为Point ???? 没找到这个方法

				Execute， ？？？？？？
				{
					先将源rt的内容赋值到shader定义的_CameraDepthAttachment贴图中
					然后调用基类的Blit方法，先设置管线的color为depth，也就是将depth渲染到color buffer中，然后执行Blit指令，用CopyDepth shader将buffer渲染到指定贴图上，后续shader直接采样这张贴图。

					vert函数，坐标转换，object-clip，实际是没用的，有用的操作是设置uv，用于采样buffer，实际uv应该就是对应分辨率的。
					frag：定义了msaa的处理，现在不会用到。直接采样_CameraDepthAttachment输出颜色，按现在的写法，应该直接用_CameraDepthAttachment就行了。额外渲染一次，应该是为了msaa准备的。



					流程总结：先设置管线的color buffer为depth，将相机得到的深度渲染到_CameraDepthAttachment，再调用CopyDepth shader渲染到_CameraDepthTexture
				}
			}
		}

		3 获取深度纹理的方法二：DepthOnlyPass
		{
			渲染所有shader中有DepthOnly pass的物体到指定RT上：_CameraDepthTexture。

		}

		总结下： DepthOnlyPass和CopyDepthPass是互斥的，
				只要配置文件上开启了msaa，CanCopyDepth就返回false，关闭msaa就返回ture
            	所以 CopyDepthPass和 DepthOnlyPass的使用谁可以通过是否开启msaa来控制，CopyDepthPass从buffer里直接取，少了很多drawCall
	}

	RT应用-opaque纹理获取
	{
		这个在内置的管线，是通过shader的grab指令获取的，移动设备支持的不好，URP加了一个拷贝buffer的方式，性能会好一些。

		通过CopyColor pass实现 相比深度纹理简单很多，不需要判断硬件是否支持

	}

	ForwardRenderer对depth和opaque贴图处理流程
	{
		renderer类相当于pass和unity底层交互的一个接口，定义了各个opaque和depth的rt
		各个rt默认是相机目标，可用于获取color和depth的buffer
		如果需要渲染color或depth到贴图中，在添加pass之前要做一些操作，只有base相机才能渲染到color和depth
		{
			判断createColorTexture和createDepthTexture，设置m_CameraColorAttachment和m_CameraDepthAttachment
			如果需要创建texture，执行CreateCameraRenderTarget函数，这个函数会调用GetTemporaryRT生成rt，函数执行后，color的depthBufferBits可能会设置为32位
			设置好后，会传给基类的m_CameraColorTarget。每个pass执行Execute方法前，会先设置camera的rt。
		}
	}

	


]==]

--[==[
	Feature 和 pass 函数执行顺序

	Feature  --> 验证下
	{
		1 Create
		2 AddRenderPasses 
		3 Execute

		2 和 3 每一帧都会调用
	}

	Pass
	{
		

		以下方法都是在Render的Execute里被调用
		{
			1 OnCameraSetup
			2 Configure
			3 Execute
			4 FrameCleanup
			{
				//销毁临时RT
				if (destinationId != -1)
	            cmd.ReleaseTemporaryRT(destinationId);
			}
			5 OnFinishCameraStackRendering // 一些清理工作

			12345 每一帧都会调用
		}
		

	}

]==]

首先URP分为4个部分，pipeline，renderer，feature和pass。

渲染基于pass，pass设定渲染对象和方法的各种指令，许多pass组合成为renderer。Feature是存储自定义pass数据的容器，可存储的数量不限，支持任何类型的数据。Feature一般用于扩展。

pipeline是一个整体的管理类，通过一系列的指令和设置渲染一帧，负责渲染每个相机，每个相机有自己的renderer，这个renderer才是常规说的渲染管线，URP默认有forward和2D两个。


1 渲染管线设置
{
	UniversalRenderPipelineAsset : RenderPipelineAsset ==》 // 继承RenderPipelineAsset 实现CreatePipeline方法
	{
		//创建当前渲染资产文件对应的渲染对象：基本都是ForwardRender类型
        CreateRenderers();

		new UniversalRenderPipeline(this); //UniversalRenderPipeline是继承RenderPipeline，会实现Render方法
	}
}

2 渲染管线流程
{
	UniversalRenderPipeline.Render
	{
		 //是否使用线性空间
        GraphicsSettings.lightsUseLinearIntensity = (QualitySettings.activeColorSpace == ColorSpace.Linear);

        //是否开启SRP Batcher
        GraphicsSettings.useScriptableRenderPipelineBatching = asset.useSRPBatcher;

        //设置shader变量值
        //主要设置了未开启环境反射时的默认颜色、阴影颜色
        SetupPerFrameShaderConstants();

        //通过相机深度进行排序
        SortCameras(cameras);

        遍历所有的相机，游戏窗口相机，场景窗口相机
        {
        	//每个相机渲染的总入口， 先渲染baseCamera，再渲染overLay相机
        	RenderCameraStack(renderContext, camera);
        	{
        		//读取UniversalAdditionalCameraData数据 可以理解为UniversalAdditionalCameraData其实就是camera的一个分身数据代表
	            //UniversalAdditionalCameraDatas 组件上有一个相机的stack
	            baseCamera.TryGetComponent<UniversalAdditionalCameraData>(out var baseCameraAdditionalData);

	            //确定最后一个激活的摄像机，来把最后的渲染数据解析到屏幕上（其实就是用最后一个激活相机渲染到屏幕）

	            //对volume系统的支持，核心是调用VolumeManager的Update方法
	            UpdateVolumeFramework(baseCamera, baseCameraAdditionalData);

	            //给相机初始化数据CameraData，使用baseCameraAdditionalData填充CameraData结构体，这里传入的是baseCamera
		        //isStackedRendering标识stack里有相机需要渲染
	            /*
	             *    //cameraData初始化了一些基本数据： InitializeStackedCameraData
	             *         targetTexture，cameraType，volumeLayerMask，抗锯齿，HDR，屏幕大小数据，renderScale，RT描述器对象
	                 
	                 //cameraData初始化了一些额外数据：InitializeAdditionalCameraData
	                        camera对象，renderType，是否清除深度，是否开启后效（后面还有判断），最大阴影距离，是否需要深度贴图，是否需要颜色贴图，渲染器对象，
	                        是否需要最后绘制到屏幕上，矩阵信息
	             */
	            InitializeCameraData(baseCamera, baseCameraAdditionalData, !isStackedRendering, out var baseCameraData);

	            //每个相机真正的渲染
                //anyPostProcessingEnabled 只要有一个相机开启了后效这个值就为true
	            RenderSingleCamera(context, baseCameraData, anyPostProcessingEnabled);
	            {
	            	//调用裁剪接口 CPU层先裁剪一遍 避免多余的点进行空间位置转换
	            	//返回失败会直接退出，成功的话会调用context.Cull(ref cullingParameters)进行真正裁剪
		            if (!TryGetCullingParameters(cameraData, out var cullingParameters))
		                return;
			        }

			        //重置RT附件，颜色RT，深度RT，
                	//设置颜色缓冲区目标和深度缓冲区目标为屏幕
                	renderer.Clear(cameraData.renderType);

                	//******override 方法 设置裁剪参数 每个Render可以自己实现, 一般是调用forwardRender的SetupCullingParameters方法
                    renderer.SetupCullingParameters(ref cullingParameters, ref cameraData);

                    //将commanderbuffer中所有的命令都发给context，其实是一个复制的过程
                    context.ExecuteCommandBuffer(cmd); // Send all the commands enqueued so far in the CommandBuffer cmd, to the ScriptableRenderContext context
                    //发送完清除，可复用对象
                	cmd.Clear();

                	//得到裁剪结果
                	var cullResults = context.Cull(ref cullingParameters);

                	//填充RenderingData结构体，这个结构体包含渲染相关的所有参数，裁剪信息，相机数据，光照阴影数据，后处理等很多
                	InitializeRenderingData(asset, ref cameraData, ref cullResults, anyPostProcessingEnabled, out var renderingData);

                	//******override 方法抽象方法 每个Render自己实现 一般是调用forwardRender的Setup方法
                    renderer.Setup(context, ref renderingData);

                    // ******override 方法Timing scope inside 一般是调用forwardRender的Execute方法
                	renderer.Execute(context, ref renderingData);

                	//再次复制commandbuffer里的命令给context
                	context.ExecuteCommandBuffer(cmd);

                	//真正执行command的地方，把之前的设置统一执行一遍, 提交给GPU
                	context.Submit(); // Actually execute the commands that we previously sent to the ScriptableRenderContext context
        		}

        		//遍历stack里所有的相机，开始渲染overlay相机
        		{
        			//Stack里的overlay相机开始渲染, 走一遍跟base相机一样的流程
        			{
        				//拿到每个overlay相机的UniversalAdditionalCameraData数据，其实就是对应camera的一个分身数据
	        			currCamera.TryGetComponent<UniversalAdditionalCameraData>(out var currCameraData);

	        			//copy 一份baseCamera数据
	        			CameraData overlayCameraData = baseCameraData;

	        			//判断是否是最后一个激活相机
                        bool lastCamera = i == lastActiveOverlayCameraIndex;

                        //调用overlay相机的VolumeManager的Update方法，后效方法
	        			UpdateVolumeFramework(currCamera, currCameraData);
	        			//仅仅只初始化了一些额外数据
	        			InitializeAdditionalCameraData(currCamera, currCameraData, lastCamera, ref overlayCameraData);

	        			//overlay相机 填充RenderingData结构体，这个结构体包含渲染相关的所有参数，裁剪信息，相机数据，光照阴影数据，后处理等很多
	        			RenderSingleCamera(context, overlayCameraData, anyPostProcessingEnabled);()
        			}
        		}
        		
        		
        }
	}



	总结下：
		UniversalRenderPipeline.Render
			==》 1 设置shader变量值 SetupPerFrameShaderConstants
			==》 2 通过相机深度进行排序 SortCameras(cameras);
			==》 3 遍历所有的相机，游戏窗口相机，场景窗口相机
			{
				3.1 游戏相机
				{
					3.1.1 RenderCameraStack(renderContext, camera);
					{
						==》 3.1.1.1 渲染base相机
						{
							a: UpdateVolumeFramework
							b:InitializeCameraData （包含InitializeStackedCameraData和InitializeAdditionalCameraData）
							c:*****RenderSingleCamera (这个方法很重要)
							{
								==》调用裁剪接口 CPU层先裁剪一遍 避免多余的点进行空间位置转换 TryGetCullingParameters
								==》重置RT附件，颜色RT，深度RT，设置颜色缓冲区目标和深度缓冲区目标为屏幕 renderer.Clear(cameraData.renderType);
								==》将commanderbuffer中所有的命令都发给context，其实是一个复制的过程 context.ExecuteCommandBuffer(cmd)
								==》发送完清除，可复用对象 cmd.Clear();
								==》进行裁剪，得到裁剪结果 var cullResults = context.Cull(ref cullingParameters);
								==》****InitializeRenderingData 填充RenderingData结构体，这个结构体包含渲染相关的所有参数，裁剪信息，相机数据，光照阴影数据，后处理等很多
								==》****对应Render的SetUp和Excute方法，默认是走forwardRender的方法
								==》再次复制commandbuffer里的命令给context
								==》提交命令给GPU，执行命令 context.Submit()
							}

						}
						==》 3.1.1.2 渲染overlay相机
						{
							a: UpdateVolumeFramework
							b: InitializeAdditionalCameraData
							c: RenderSingleCamera
							{
								-- 跟base相机的一样
							}
						}
					}
				}

				3.2 场景相机
				{
					3.2.1 UpdateVolumeFramework
					3.2.2 RenderSingleCamera
				}
			}
}

3 ForwardRenderer
{
	核心部分，pass ： ForwardRenderer可以简单理解成驱动各个pass执行的一个管理者，pass则实现了具体的渲染逻辑。
	pass的功能，分为两部分，配置rt和执行最终渲染。

	pass-事件
	{
		基类文件中定义了一系列的渲染事件，RenderPassEvent。每个pass，在初始化的时候，都定义了一个event，这个event用于pass的排序。id之间间隔50，可加offet以添加额外的event。
	}

	pass-配置rt
	{
		pass在渲染前需要先配置渲染目标，renderer基类调用pass的Configure抽象函数
		renderPass.Configure(cmd, cameraData.cameraTargetDescriptor); 

		每个pass实现具体逻辑，pass子类调用基类的ConfigureTarget方法，配置渲染目标和clear方法，子类没实现则渲染到相机的目标。

		渲染目标分两个，color和depth，depth只有一个，color是个数组，默认第一个是相机目标，最大值在SystemInfo.supportedRenderTargetCount定义。注意这个步骤只是设置了pass内部的数据，并没有真的通知到管线。
		RenderTargetIdentifier[] m_ColorAttachments = new RenderTargetIdentifier[]{BuiltinRenderTextureType.CameraTarget}; 
		RenderTargetIdentifier m_DepthAttachment = BuiltinRenderTextureType.CameraTarget; 

		真正设置渲染目标，是通过CommandBuffer的SetRenderTarget方法，
		URP在CoreUtils类封装了一个静态函数SetRenderTarget。
		ScriptableRenderer类在ExecuteRenderPass方法中，先调用pass的Config函数，然后取pass的color和depth数据，设置为真正的渲染目标。
	}

	构造方法 ForwardRenderer
	{
		1 提前创建一堆材质信息
		2 是否重载模板测试信息
		3 定义一堆pass，使用事件排序，越小排到越前，越先执行
		{
			//主光阴影ShadowCaster，附光阴影ShadowCaster  50
			//pre深度pass  150
			//深度+法线pass 150
			//颜色分级Lut pass 150
			//不透明物体pass 250
			//天空盒绘制Pass 350
			//CopyDepthPass 400
			//透明物体设置渲染pass 450
			//透明物体渲染pass 450
			//无效物体渲染Pass，callback  550
			//后效pass 550
			//最终的pass 1000+1
			//截屏pass 1000
			//FinalBlitPass 最后输出到屏幕的pass  1000+1
		}

		4 定义各种RT名称 RenderTargetHandle.id 
		{
			m_CameraColorAttachment ==> _CameraColorTexture
			m_CameraDepthAttachment ==> _CameraDepthAttachment
			m_DepthTexture ==> _CameraDepthTexture
			m_NormalsTexture ==> _CameraNormalsTexture
			m_OpaqueColor ==> _CameraOpaqueTexture
			m_AfterPostProcessColor ==> _AfterPostProcessTexture
			m_ColorGradingLut ==> _InternalGradingLut
			m_DepthInfoTexture ==> _DepthInfoTexture
			m_TileDepthInfoTexture ==> _TileDepthInfoTexture
		}
	}

	ForwardRenderer.Setup (//根据配置确定是否加入对应的Pass参与渲染  UniversalRenderPipeline.RenderSingleCamera 会调用这个方法)
	{
		1 判断当前相机是只渲染深度： 只渲染不透明物体pass 天空盒pass 透明物体pass
		2 第一次判断是否需要创建颜色纹理 ColorTexture，有自定义feature以及不是preveiew相机 ==》 设置相机的颜色缓冲区目标对象 为一张rt，名字叫“_CameraColorTexture”
		3 RenderFeature Pass 调用每个激活featrure的 AddRenderPasses
		4 设置一些逻辑变量
		{
			//正在渲染的相机上是否开启后效,只要相机上开了就行
            bool applyPostProcessing = cameraData.postProcessEnabled;

            //只要有一个相机开启了后效就为true，包括 base Camera和 overlay Camera
            bool anyPostProcessing = renderingData.postProcessingEnabled;

            bool generateColorGradingLUT = cameraData.postProcessEnabled; //是否生成LUT 表  后期开启了就会生成LUT表
            bool isSceneViewCamera = cameraData.isSceneViewCamera; //Scene窗口相机
		}

		5 再次判断是否需要深度图， 
		{
			bool requiresDepthTexture = cameraData.requiresDepthTexture || renderPassInputs.requiresDepthTexture || this.actualRenderingMode == RenderingMode.Deferred;
		}
		6 判断主光和副光是否需要阴影
		7 判断是不是需要产生深度的pass，并且设置对应pass的event
		{
			bool requiresDepthPrepass = requiresDepthTexture && !CanCopyDepth(ref renderingData.cameraData);
            requiresDepthPrepass |= isSceneViewCamera;
            requiresDepthPrepass |= isPreviewCamera;
            requiresDepthPrepass |= renderPassInputs.requiresDepthPrepass; //一般为false
            requiresDepthPrepass |= renderPassInputs.requiresNormalsTexture; //一般为false

            //需要深度贴图==》 直接在渲染完不透明物体后渲染
            //不需要深度贴图 + (当前相机开启了后效或者当前相机是场景相机) ==》直接在渲染完透明物体后渲染
            m_CopyDepthPass.renderPassEvent = (!requiresDepthTexture && (applyPostProcessing || isSceneViewCamera)) ? RenderPassEvent.AfterRenderingTransparents : RenderPassEvent.AfterRenderingOpaques;
		}
		8 第二次判断是否需要产生颜色纹理RT _CameraColorTexture
		{
			 bool IntermediateColorTexture = RequiresIntermediateColorTexture(ref cameraData); //是否产生中间RT，一般返回true
            createColorTexture |= IntermediateColorTexture; 
            createColorTexture |= renderPassInputs.requiresColorTexture;
            createColorTexture &= !isPreviewCamera;
		}
		9 最终判断是否需要产生深度纹理RT _CameraDepthAttachment
		{
			bool createDepthTexture = cameraData.requiresDepthTexture && !requiresDepthPrepass;
            
            //cameraData.resolveFinalTarget 当前相机是否需要最后绘制到屏幕上，如果当前相机的camerastack里有激活的相机就不需要绘制到屏幕
            createDepthTexture |= (cameraData.renderType == CameraRenderType.Base && !cameraData.resolveFinalTarget);
            // Deferred renderer always need to access depth buffer.
            createDepthTexture |= this.actualRenderingMode == RenderingMode.Deferred;
		}

		9 设置摄像机的渲染目标：颜色缓冲和深度缓冲
		{
			baseCamera==》如果有需要颜色rt和深度rt，渲染目标就设置为rt纹理，不需要的话就设置为默认的帧缓冲
			overlay相机==》渲染目标设置为默认的帧缓冲
		}
		10 根据逻辑判断是否把对应的pass加入队列，并且调用pass的SetUp方法 ***
		11 判断是否要最后执行一次 final blit，满足条件之一即可
	}


	/*超级重点*/

	//各个render都没有实现Execute方法，调用基类的ScriptableRenderer.Execute
	ScriptableRenderer.Execute(UniversalRenderPipeline.RenderSingleCamera 会调用)
	{
		1 渲染开始 调用每个激活pass的OnCameraSetup方法
		{
			//每个pass如果没有重载就调用基类ScriptableRenderPass的OnCameraSetup方法，是个空方法
            //DepthOnlyPass, CopyDepthPass, CopyColorPass, PostProcessPass 这几种pass重载了 大致功能都是==>配置RT 设置渲染目标 清除帧缓冲一些数据（深度或者颜色）

            //context.ExecuteCommandBuffer(cmd); 把coommandbuff里的执行命令复制到connext里
		}
		2 提前设置一些shader变量，把命令从commandbuffer复制到context里，并且清除commandbuffer
		{
			//重置一些shader里的宏设置

			//设置一些摄像机相关shader的参数
            // 摄像机的世界坐标 "_WorldSpaceCameraPos", 屏幕参数 “_ScreenParams”

			//设置shader里的时间参数
            // _Time  _SinTime  _CosTime  unity_DeltaTime  _TimeParameters

            context.ExecuteCommandBuffer(cmd);  //复制命令到context里
            cmd.Clear(); //cmd清理，可以重复使用同一个实例对象
		}

		3 renderpass的排序，重载了< 操作符,根据pass的渲染事件 ，越小的排在越前

		4 渲染pass分类
		{
			 //RenderBlocks是封装好的数据结构，主要是让pass分类，存储在m_BlockRanges字段里，m_BlockRanges[0]为0,从1开始
			 * 将pass按event顺序，分成四块
                      BeforeRendering：用于处理阴影等，不是实际的渲染，只作为功能使用 事件顺序小于BeforeRenderingPrepasses（150）
                      MainRenderingOpaque：渲染不透明物体，事件顺序小于AfterRenderingOpaques (300) 大于等于 BeforeRenderingPrepasses (150)
                      MainRenderingTransparent：透明物体  事件顺序小于AfterRenderingPostProcessing（600）大于等于 AfterRenderingOpaques (300)
                      AfterRendering：在后处理之后，事件顺序小于(RenderPassEvent) Int32.MaxValue，大于等于AfterRenderingPostProcessing（600）
               */
		}
		5 Render自己实现 设置光照需要的一系列参数 ，调用render的SetupLights方法


		接下来*****按照分类分别执行ExecuteBlock方法和ExecuteRenderPass方法，调用不同的pass的Configure方法和Execute方法，注意这里暂时不把渲染命令提交给GPU

		6 ********先执行渲染之前的处理，主要是阴影相关的pass，这里调用的是时间顺序小于150的, 主要是阴影相关的一些pass，MainLightShadowCasterPass和AdditionalLightsShadowCasterPass
		{

			ExecuteBlock
			{
				遍历m_ActiveRenderPassQueue，分别调用ExecuteRenderPass(context, renderPass, ref renderingData)
			}

			ExecuteRenderPass*******
			{
				//********先调用pass的Configure方法 大部分内置的pass都没有实现，自定义的pass可以自己实现
	            renderPass.Configure(cmd, cameraData.cameraTargetDescriptor);

                //*******设置pass的渲染目标对象，Clore和Depth，摄像机真正的渲染目标对象
                SetRenderPassAttachments(cmd, renderPass, ref cameraData);
                {
                	//设置渲染目标
                	SetRenderTarget
                	{
                		//渲染时颜色信息配置
                		RenderBufferLoadAction colorLoadAction = ((uint)clearFlag & (uint)ClearFlag.Color) != 0 ?

                			RenderBufferLoadAction.DontCare : RenderBufferLoadAction.Load;
                		//渲染时深度信息配置
                		RenderBufferLoadAction depthLoadAction = ((uint)clearFlag & (uint)ClearFlag.Depth) != 0 ?
                			RenderBufferLoadAction.DontCare : RenderBufferLoadAction.Load;

                		//继续调用重载方法SetRenderTarget，设置颜色缓冲区和深度缓冲区的操作，快到末尾了
                		SetRenderTarget(cmd, colorAttachment, colorLoadAction, RenderBufferStoreAction.Store,
                			depthAttachment, depthLoadAction, RenderBufferStoreAction.Store, clearFlag, clearColor);
                		{
                			//*******真正设置渲染目标，是通过CommandBuffer的SetRenderTarget方法和ClearRenderTarget方法
                			CoreUtils.SetRenderTarget
                			{
                				//******这里的colorBuffer和depthBuffer 其实就是pass上colorAttachment和depthAttachment
                				cmd.SetRenderTarget(colorBuffer, colorLoadAction, colorStoreAction, depthBuffer, depthLoadAction, depthStoreAction);
            					ClearRenderTarget(cmd, clearFlag, clearColor);
            					{
            						cmd.ClearRenderTarget((clearFlag & ClearFlag.Depth) != 0, (clearFlag & ClearFlag.Color) != 0, clearColor);
            					}
                			}
                		}
                	}
                }

                //又执行了一次拷贝渲染命令到context里
                context.ExecuteCommandBuffer(cmd);
                
                //*******再调用pass的Execute方法，基本都重载了
            	renderPass.Execute(context, ref renderingData);
			}



			//渲染之前的处理：这里调用的是时间顺序小于150的, 主要是阴影相关的一些pass，MainLightShadowCasterPass和AdditionalLightsShadowCasterPass

		}

		7 设置摄像机数据，设置矩阵以及其他一些属性 unity_MatrixVP 视图投影矩阵
		{
			context.SetupCameraProperties(camera);
            SetCameraMatrices(cmd, ref cameraData, true);

            SetShaderTimeValues(cmd, time, deltaTime, smoothDeltaTime);
		}
		8 渲染之前再次复制渲染命令到context里，复制完成后调用commanderbuffer的clear方法，重用commanderbuffer对象
		{
			context.ExecuteCommandBuffer(cmd); //继续执行cmd 
            cmd.Clear();
		}
		9 渲染不透明物体:里面对m_ActiveRenderPassQueue中的pass 进行调用，先调用pass的Configure方法 再调用pass的Execute方法  暂时不提交给GPU 
		{
			//这里调用的是时间顺序小于300大于等于150的,
            //相关的一些pass: DepthOnlyPass,DepthNormalOnlyPass,ColorGradingLutPass，DrawObjectsPass(不透明)
            ExecuteBlock(RenderPassBlock.MainRenderingOpaque, in renderBlocks, context, ref renderingData);
		}
		10 渲染透明物体里面,对m_ActiveRenderPassQueue中的pass 进行调用，先调用pass的Configure方法 再调用pass的Execute方法  暂时不提交给GPU 
		{
			//这里调用的是时间顺序小于600大于等于300的,
            //相关的一些pass:
            //DrawSkyboxPass, CopyDepthPass, CopyColorPass, TransparentSettingsPass, DrawObjectsPass(透明), InvokeOnRenderObjectCallbackPass
            //PostProcessPass(后效pass，这个变量m_PostProcessPass)
		}

		11 编辑器下生效 DrawGizmos
		12 渲染完成之后的处理,对m_ActiveRenderPassQueue中的pass 进行调用，先调用pass的Configure方法 再调用pass的Execute方法  暂时不提交给GPU 
		{
			//这里调用的是时间顺序大于等于600的,
            //相关的一些pass: PostProcessPass(后效pass，这个变量m_FinalPostProcessPass)，CapturePass， FinalBlitPass
            ExecuteBlock(RenderPassBlock.AfterRendering, in renderBlocks, context, ref renderingData);
		}
		13 编辑器下生效DrawWireOverlay(context, camera); DrawGizmos(context, camera, GizmoSubset.PostImageEffects);
		14 cleanup了一下所有的pass，释放了RT，重置渲染对象，清空pass队列 InternalFinishRendering
		{
			//执行每个pass的FrameCleanup方法
            //最后一个输出到帧缓冲的相机调用：
            //    pass的FrameCleanup方法和OnFinishCameraStackRendering方法、
            //    对应Render的FinishRendering方法，一般调用ForwarRender的FinishRendering
		}
		15 再次复制渲染命令到context里，context.ExecuteCommandBuffer(cmd);
	}
}

4 一些pass的SetUp方法，以及前向渲染相关各种pass的重载方法：OnCameraSetup， Configure，Execute， FrameCleanup，OnFinishCameraStackRendering
{
	1 MainLightShadowCasterPass：渲染结果最后绘制在RT上
	{
		Setup
		{
			判断是否需要绘制阴影，不需要直接返回false
			ShadowMap分辨率大小 最大阴影距离设置
		}

		Configure
		{
			//获取一张阴影贴图RT
            m_MainLightShadowmapTexture = ShadowUtils.GetTemporaryShadowTexture(m_ShadowmapWidth,
                    m_ShadowmapHeight, k_ShadowmapBufferBits);
            //配置color buffer渲染目标，设置颜色渲染到RT上
            ConfigureTarget(new RenderTargetIdentifier(m_MainLightShadowmapTexture));
            //清一遍帧缓冲数据
            ConfigureClear(ClearFlag.All, Color.black);
		}

		Execute
		{
			RenderMainLightCascadeShadowmap
			{
				 //根据阴影联级，给shader中的一些变量设置值：_ShadowBias:阴影偏移，是为了自遮挡阴影瑕疵(shadow acne)，LightDirection:光的方向
				 ShadowUtils.SetupShadowCasterConstantBuffer(cmd, ref shadowLight, shadowBias);

				 //设置视口，设置vp矩阵，绘制阴影，添加渲染命令到context里 ******
				  ShadowUtils.RenderShadowSlice

				  //shader变体设置，开启和关闭对应的宏
				  	CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.MainLightShadows, true);
                	CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.MainLightShadowCascades, shadowData.mainLightShadowCascadesCount > 1);
                	CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.SoftShadows, softShadows);
                

				   //设置shader相关常量
				   SetupMainLightShadowReceiverConstants(cmd, shadowLight, shadowData.supportsSoftShadows);


				   //复制渲染命令到context
            		context.ExecuteCommandBuffer(cmd);
			}
		}
		
	}
	——————————————————————————————————————————————————————————
	2 AdditionalLightsShadowCasterPass：渲染结果最后绘制在RT上
	{
		Setup
		{
			设置副光源阴影数据
		}

		Configure
		{
			//设置阴影RT
            m_AdditionalLightsShadowmapTexture = ShadowUtils.GetTemporaryShadowTexture(m_ShadowmapWidth, m_ShadowmapHeight, k_ShadowmapBufferBits);
            //配置color buffer渲染目标，设置颜色渲染到RT上
            ConfigureTarget(new RenderTargetIdentifier(m_AdditionalLightsShadowmapTexture));
            //配置清一遍所有帧缓冲数据，颜色+深度+模板都清除
            ConfigureClear(ClearFlag.All, Color.black);
		}

		Execute
		{
			RenderAdditionalShadowmapAtlas
			{
				//根据阴影联级，给shader中的一些变量设置值：_ShadowBias:阴影偏移，是为了自遮挡阴影瑕疵(shadow acne)，LightDirection:光的方向
				ShadowUtils.SetupShadowCasterConstantBuffer(cmd, ref shadowLight, shadowBias);

				//设置视口，设置vp矩阵，绘制阴影，添加渲染命令到context里 ******
				ShadowUtils.RenderShadowSlice

			  	//shader变体设置，开启和关闭对应的宏
			    CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.AdditionalLightShadows, anyShadowSliceRenderer);
            	CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.SoftShadows, softShadows);
			}
		}
	}
	——————————————————————————————————————————————————————————
	3 DepthOnlyPass：渲染结果最后绘制在RT上，名为_CameraDepthTexture
	{
		Setup
		{
			/*
             RenderTextureDescriptor 结构，里面记录的是对于RT的一些描述信息，depthBufferBits默认给的32bit。
             RenderTargetHandle结构主要记录了一个shader property id
             */
            this.depthAttachmentHandle = depthAttachmentHandle;
            
            //先设置管线的color为depth
            baseDescriptor.colorFormat = RenderTextureFormat.Depth; 
            //32位 深度通道
            baseDescriptor.depthBufferBits = kDepthBufferBits;

            //禁用MSAA
            baseDescriptor.msaaSamples = 1;
            descriptor = baseDescriptor;
		}

		OnCameraSetup
		{
			//创建一个rt记录深度值，“_CameraDepthTexture”
            cmd.GetTemporaryRT(depthAttachmentHandle.id, descriptor, FilterMode.Point);
            
            //设置颜色缓冲渲染目标
            ConfigureTarget(new RenderTargetIdentifier(depthAttachmentHandle.Identifier(), 0, CubemapFace.Unknown, -1));
            
            //清除所有信息（颜色+深度+模板）
            ConfigureClear(ClearFlag.All, Color.black);
		}

		Execute
		{
			//复制绘制命令到context里
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            //设置一些渲染相关的参数：排序方式，passId，
            //不透明物体排序
            var sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;

            //只绘制具有"DepthOnly"pass的物体
            //创建一个DrawingSettings结构体 非CameraType.Preview相机会开启GPUInstancing
            var drawSettings = CreateDrawingSettings(m_ShaderTagId, ref renderingData, sortFlags);
            drawSettings.perObjectData = PerObjectData.None;
            
            //绘制几何体：其实这里也只是把命令添加到队列里，需要 context.Submit()后才会真正绘制
            context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref m_FilteringSettings);
		}

		OnCameraCleanup
		{
			//渲染目标如果不是默认帧缓冲需要执行清理操作
            if (depthAttachmentHandle != RenderTargetHandle.CameraTarget)
            {
                cmd.ReleaseTemporaryRT(depthAttachmentHandle.id);
                depthAttachmentHandle = RenderTargetHandle.CameraTarget;
            }
		}

	}
	——————————————————————————————————————————————————————————
}

5 Feature相关

