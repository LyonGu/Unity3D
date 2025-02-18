
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


		//通过RenderTargetHandle对象创建RT
		cmd.GetTemporaryRT(depthAttachmentHandle.id, descriptor, FilterMode.Point);

		//创建一个RenderTargetHandle对象
		RenderTargetHandle rtHandle = new RenderTargetHandle()
		rtHandle.Init("_CameraColorTexture");

		//通过RenderTargetHandle对象返回一个RenderTargetIdentifier对象
		RenderTargetIdentifier identifier = RenderTargetHandle.Identifier()

		//通过一个RT 创建RenderTargetIdentifier对象
		m_AdditionalLightsShadowmapTexture = ShadowUtils.GetTemporaryShadowTexture(m_ShadowmapWidth, m_ShadowmapHeight, k_ShadowmapBufferBits);
        //配置color buffer渲染目标，设置颜色渲染到RT上
        new RenderTargetIdentifier(m_AdditionalLightsShadowmapTexture);


		*****BuiltinRenderTextureType.CameraTarget意思就是当前上摄像机的目标纹理，如果camera的targetTexure没有设置，就是默认帧缓冲，设置了就是相机的TargetTexure
		*****RenderTargetHandle.CameraTarget 表示帧缓冲对象

        
        RenderTargetIdentifier identifier = BuiltinRenderTextureType.CameraTarget

		//RenderTargetHandle对象为RenderTargetHandle.CameraTarget 表示帧缓冲对象
        RenderTargetHandle depthAttachmentHandle = RenderTargetHandle.CameraTarget
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

			//m_InternalLut为RenderTargetHandle类型对象
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
				CopyDepthPass.OnCameraSetup：设置目标rt格式，colorFormat为Depth，32位，msaa为1不开启，filterMode为Point 

				CopyDepthPass.Execute
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
            	
            	//开启了MSAA 这个requiresDepthCopyPass 一般返回true
            	// 开启了MSAA ： requiresDepthPrepass 为false 、renderingData.cameraData.requiresDepthTexture 为true， createDepthTexture 为true
            	// 会使用CopyDepthPass，设置渲染目标为RT:_CameraDepthTexture
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

	
UniversalRenderPipelineAsset.DestroyRenderers
{
	DestroyRenderer(Render)
	==> renderer.Dispose();
		==> {
			rendererFeature.Dispose 
			各个render自己实现的Dispose(true)
		}
}

*******最后pass的渲染目标
{
	ScriptableRenderer里有m_CameraColorTarget和m_CameraDepthTarget，默认都是帧缓冲BuiltinRenderTextureType.CameraTarget
	ScriptableRenderPass每个pass里自己有
	{
		m_ColorAttachments ==》 一个数组，第一个元素为 BuiltinRenderTextureType.CameraTarget 

		colorAttachment = m_ColorAttachments[0]
		m_DepthAttachment = BuiltinRenderTextureType.CameraTarget 
		默认都是帧缓冲

		colorAttachment和m_DepthAttachment类型为 RenderTargetIdentifier
	}


	ScriptableRenderer.SetRenderPassAttachments方法里会有判断
	{
		//默认是使用pass的colorAttachment和depthAttachment
		RenderTargetIdentifier passColorAttachment = renderPass.colorAttachment;
        RenderTargetIdentifier passDepthAttachment = renderPass.depthAttachment;

		if（！renderPass.overrideCameraTarget）
		{
			//未重载，直接使用ScriptableRenderer的渲染目标变量作为最后输出
			passColorAttachment = m_CameraColorTarget;
            passDepthAttachment = m_CameraDepthTarget;
		}
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
		1 首先调用基类ScriptableRenderer的构造函数，根据data.rendererFeatures判断是否要创建自定义feature
		2 提前创建一堆材质信息
		3 是否重载模板测试信息
		4 定义一堆pass，使用事件排序，越小排到越前，越先执行
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

		5定义各种RT名称 RenderTargetHandle.id 
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
                		//渲染时颜色信息配置 如果clearFlag为ClearFlag.Color或者ClearFlag.All, 返回RenderBufferLoadAction.DontCare，效率高
                		RenderBufferLoadAction colorLoadAction = ((uint)clearFlag & (uint)ClearFlag.Color) != 0 ?
                			RenderBufferLoadAction.DontCare : RenderBufferLoadAction.Load;

                		//渲染时深度信息配置，如果clearFlag为ClearFlag.Depth或者ClearFlag.All, 返回RenderBufferLoadAction.DontCare，效率高
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
            						//clearFlag为ALL或者Depth时清除深度缓冲
            						//clearFlag为All或者Color时清除颜色缓冲
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

4 一些pass的SetUp方法，以及前向渲染相关各种pass的重载方法：OnCameraSetup， Configure，Execute， FrameCleanup（最后调用pass的OnCameraCleanup），OnFinishCameraStackRendering（只要最后一个输出到屏幕的相机才会调用OnFinishCameraStackRendering）
{
	OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) ==》 可以拿到 RenderingData对象
	Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) ==》  可以拿到 RenderTextureDescriptor对象
	Execute(ScriptableRenderContext context, ref RenderingData renderingData); ==》 可以拿到 RenderingData对象
	FrameCleanup ==》 大部分pass都没有实现，调用基类的，最后会调用到pass的OnCameraCleanup
	OnFinishCameraStackRendering(CommandBuffer cmd)
	——————————————————————————————————————————————————————————
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
			//复制绘制命令到context里，因为OnCameraSetup里创建了RT，要把命令复制给context
            //后面context.submit时才会把命令提交给GPU
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

            context.ExecuteCommandBuffer(cmd);
		}

		OnCameraCleanup
		{
			//渲染目标如果不是默认帧缓冲需要执行清理操作
            if (depthAttachmentHandle != RenderTargetHandle.CameraTarget)
            {
            	//释放深度图RT
                cmd.ReleaseTemporaryRT(depthAttachmentHandle.id);

                //重置深度缓冲的渲染目标为帧缓冲
                depthAttachmentHandle = RenderTargetHandle.CameraTarget;
            }
		}

	}
	——————————————————————————————————————————————————————————
	4 DepthNormalOnlyPass：渲染结果绘制到RT上，颜色缓冲区_CameraNormalsTexture和深度缓冲区_CameraDepthTexture分开了
	{
		Setup
		{
			//设置深度信息渲染目标RT _CameraDepthTexture 
            this.depthHandle = depthHandle;
            baseDescriptor.colorFormat = RenderTextureFormat.Depth;
            baseDescriptor.depthBufferBits = k_DepthBufferBits;
            baseDescriptor.msaaSamples = 1;// 禁用 MSAA
            depthDescriptor = baseDescriptor; //结构体 每次都是拷贝
            
            //设置法线信息渲染目标RT _CameraNormalsTexture
            this.normalHandle = normalHandle;
            baseDescriptor.colorFormat = RenderTextureFormat.RGHalf;
            baseDescriptor.depthBufferBits = 0;
            baseDescriptor.msaaSamples = 1;
            normalDescriptor = baseDescriptor;
		}

		OnCameraSetup
		{
			//创建 _CameraDepthTexture RT 和  _CameraNormalsTexture RT
            cmd.GetTemporaryRT(normalHandle.id, normalDescriptor, FilterMode.Point);
            cmd.GetTemporaryRT(depthHandle.id, depthDescriptor, FilterMode.Point);
            //配置渲染目标
            //颜色缓冲区==》存储法线的RT
            //深度缓冲区==》存储深度的RT
            ConfigureTarget(
                new RenderTargetIdentifier(normalHandle.Identifier(), 0, CubemapFace.Unknown, -1),
                new RenderTargetIdentifier(depthHandle.Identifier(), 0, CubemapFace.Unknown, -1)
                );
            ConfigureClear(ClearFlag.All, Color.black);
		}

		Execute
		{
			//复制绘制命令到context里，因为OnCameraSetup里创建了RT，要把命令复制给context
            //后面context.submit时才会把命令提交给GPU
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            //设置一些渲染相关的参数：排序方式，passId，
            //不透明物体排序
            var sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
            
            //只绘制具有"DepthNormals"pass的物体
            //创建一个DrawingSettings结构体 非CameraType.Preview相机会开启GPUInstancing
            var drawSettings = CreateDrawingSettings(m_ShaderTagId, ref renderingData, sortFlags);
            drawSettings.perObjectData = PerObjectData.None;

            ref CameraData cameraData = ref renderingData.cameraData;
            Camera camera = cameraData.camera;
            //绘制几何体：其实这里也只是把命令添加到队列里，需要 context.Submit()后才会真正绘制
            context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref m_FilteringSettings);

            context.ExecuteCommandBuffer(cmd);

		}

		OnCameraCleanup
		{
			//渲染目标如果不是默认帧缓冲需要执行清理操作
            if (depthHandle != RenderTargetHandle.CameraTarget)
            {
                //释放深度图RT和法线图RT
                cmd.ReleaseTemporaryRT(normalHandle.id);
                cmd.ReleaseTemporaryRT(depthHandle.id);
                
                //重置渲染目标为帧缓冲
                normalHandle = RenderTargetHandle.CameraTarget;
                depthHandle = RenderTargetHandle.CameraTarget;
            }
		}
	}
	——————————————————————————————————————————————————————————
	//LUT贴图 根据一个像素的RGB查表取得另一个RGB并且替换
	5 ColorGradingLutPass(颜色分级lut Pass)：渲染目标为颜色缓冲渲染目标为RT（名字为 _InternalGradingLut），深度缓冲目标为帧缓冲
	{
		Setup
		{
			//设置RenderTargetHandle 
			m_InternalLut = internalLut;
		}

		Execute
		{
			//获取所有颜色分级设置
			//Volume下的后效效果组件: ChannelMixer,ColorAdjustments,ColorCurves,LiftGammaGain,ShadowsMidtonesHighlights....

			//创建一个RT，名字为 _InternalGradingLut
			cmd.GetTemporaryRT(m_InternalLut.id, desc, FilterMode.Bilinear);

			//设置一系列shader属性

			//绘制命令，scr和des都传参为 m_InternalLut.id
			Blit(cmd, m_InternalLut.id, m_InternalLut.id, material);
			{
				//设置颜色缓冲渲染目标为RT，深度缓冲目标为帧缓冲
				ScriptableRenderer.SetRenderTarget(cmd, destination, BuiltinRenderTextureType.CameraTarget, clearFlag, clearColor);

				//执行对应pass，把最后输出结果到原始RT上
            	cmd.Blit(source, destination, material, passIndex);
			}

			//复制渲染命令到context
			context.ExecuteCommandBuffer(cmd);
		}

		OnFinishCameraStackRendering
		{
			cmd.ReleaseTemporaryRT(m_InternalLut.id);
		}
	}
	——————————————————————————————————————————————————————————
	//不透明物体和透明物体都是用这个pass，
	//默认有 SRPDefaultUnlit UniversalForward UniversalForwardOnly LightweightForward
	6 DrawObjectsPass:  
	{
		pass没有重置渲染目标，直接使用ScriptableRenderer的渲染目标变量作为最后输出,
		如果没有中间RT产生(颜色RT和深度RT)，输出到帧缓冲
		如果有中间RT产生，颜色缓冲数据输出到_CameraColorTexture，深度缓冲数据输出到 _CameraDepthAttachment

		Execute
		{
			 //设置shader属性_DrawObjectPassData，w分量标识为透明或者不透明
			 cmd.SetGlobalVector(s_DrawObjectPassDataPropID, drawObjectPassData);

			 //设置shader属性_ScaleBiasRt
              cmd.SetGlobalVector(ShaderPropertyId.scaleBiasRt, scaleBias);

             //复制命令到context里
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear(); //ExecuteCommandBuffer 执行后就把相关参数给设置好了，然后调用cmd.clear可以复用cmd对象

            //排序设置是根据Opaque字段决定，DrawSettings通过CreateDrawingSettings方法生成
            var sortFlags = (m_IsOpaque) ? renderingData.cameraData.defaultOpaqueSortFlags : SortingCriteria.CommonTransparent;
            var drawSettings = CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, sortFlags);
            var filterSettings = m_FilteringSettings;

            //绘制几何体，如果overrideCameraTarget为false在，最后会使用ScriptableRenderer的渲染目标变量作为最后输出
            //m_CameraColorTarget 和 m_CameraDepthTarget
            context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filterSettings, ref m_RenderStateBlock);

            // Render objects that did not match any shader pass with error shader
            // fallback shader
            RenderingUtils.RenderObjectsWithError(context, ref renderingData.cullResults, camera, filterSettings, SortingCriteria.None);

            context.ExecuteCommandBuffer(cmd);
		}
	}
	——————————————————————————————————————————————————————————
	7 DrawSkyboxPass
	{
		pass没有重置渲染目标，直接使用ScriptableRenderer的渲染目标变量作为最后输出,
		如果没有中间RT产生(颜色RT和深度RT)，输出到帧缓冲
		如果有中间RT产生，颜色缓冲数据输出到_CameraColorTexture，深度缓冲数据输出到 _CameraDepthAttachment

		Execute
		{
			//直接有接口可以绘制天空盒
			context.DrawSkybox(renderingData.cameraData.camera);
		}
	}
	——————————————————————————————————————————————————————————
	8 CopyColorPass：渲染结果绘制到 颜色缓冲数据绘制到 “m_OpaqueColor” RT，不需要深度信息
	{
		绘制颜色RT  _CameraOpaqueTexture，只要不透明颜色数据

		Setup
		{
			//设置源目标为 m_ActiveCameraColorAttachment所对应的目标
            //m_ActiveCameraColorAttachment为ScriptableRenderer的变量，可能为_CameraColorTexture也可能为默认帧缓冲
            this.source = source;
            
            //设置target目标 “m_OpaqueColor” RT
            this.destination = destination;
            
            //降采样信息
            m_DownsamplingMethod = downsampling;
		}

		OnCameraSetup
		{
			RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            
            //禁用msaa
            descriptor.msaaSamples = 1;

            //depthBufferBits 为0 不需要深度信息
            descriptor.depthBufferBits = 0;
            
            //设置降采样后RT的尺寸
            if (m_DownsamplingMethod == Downsampling._2xBilinear)
            {
                //双性过滤降采样
                descriptor.width /= 2;
                descriptor.height /= 2;
            }
            else if (m_DownsamplingMethod == Downsampling._4xBox || m_DownsamplingMethod == Downsampling._4xBilinear)
            {
                descriptor.width /= 4;
                descriptor.height /= 4;
            }
            
            //创建“m_OpaqueColor” RT
            cmd.GetTemporaryRT(destination.id, descriptor, m_DownsamplingMethod == Downsampling.None ? FilterMode.Point : FilterMode.Bilinear);
		}

		Execute
		{
			//设置渲染目标，颜色缓冲区目标为opaqueColorRT，深度缓冲区目标为帧缓冲
            //clearFlag为None
            ScriptableRenderer.SetRenderTarget(cmd, opaqueColorRT, BuiltinRenderTextureType.CameraTarget, clearFlag,
                clearColor);

            RenderingUtils.Blit(cmd, source, opaqueColorRT, m_CopyColorMaterial, 0, useDrawProceduleBlit);
            {
            	默认参数使用都是load
            	RenderBufferLoadAction colorLoadAction = RenderBufferLoadAction.Load, //效率最低
	            RenderBufferStoreAction colorStoreAction = RenderBufferStoreAction.Store,
	            RenderBufferLoadAction depthLoadAction = RenderBufferLoadAction.Load,
	            RenderBufferStoreAction depthStoreAction = RenderBufferStoreAction.Store


	             //设置shader全局属性_SourceTex
            	cmd.SetGlobalTexture(ShaderPropertyId.sourceTex, source);

	            //设置渲染目标，这里参数使用的都是默认值 这里destination就是opaqueColorRT
                cmd.SetRenderTarget(destination, colorLoadAction, colorStoreAction, depthLoadAction, depthStoreAction);

                /????TODO
                cmd.Blit(source, BuiltinRenderTextureType.CurrentActive, material, passIndex);
            }
		}

		OnCameraCleanup
		{
			//destination如果不是默认帧缓冲需要执行清理操作
            if (destination != RenderTargetHandle.CameraTarget)
            {
                //释放RT
                cmd.ReleaseTemporaryRT(destination.id);
                //重置destination为帧缓冲
                destination = RenderTargetHandle.CameraTarget;
            }
		}
	}
	——————————————————————————————————————————————————————————
	9 CopyDepthPass: 渲染结果 颜色缓冲数据绘制到_CameraDepthTexture RT(存的其实是深度数据) ，深度缓冲数据会绘制到帧缓冲
	{
		Setup
		{
			//设置源目标为 m_ActiveCameraColorAttachment所对应的目标
            //m_ActiveCameraColorAttachment为ScriptableRenderer的变量，可能为_CameraColorTexture也可能为默认帧缓冲
            this.source = source;
            
            //destination为_CameraDepthTexture RT
            this.destination = destination;
            this.AllocateRT = AllocateRT && !destination.HasInternalRenderTargetId();
		}

		OnCameraSetup
		{
			//设置RT的格式：colorFormat为Depth，32位，msaa为1不开启，filterMode为Point
            var descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.colorFormat = RenderTextureFormat.Depth; //先设置管线的color为depth，也就是将depth渲染到color buffer中
            descriptor.depthBufferBits = 32; //TODO: do we really need this. double check;
            descriptor.msaaSamples = 1;
            if (this.AllocateRT)
                cmd.GetTemporaryRT(destination.id, descriptor, FilterMode.Point);

             //设置颜色缓冲目标为_CameraDepthTexture RT， 设置了pass的overrideCameraTarget为true，会使用pass的渲染目标
            ConfigureTarget(new RenderTargetIdentifier(destination.Identifier(), 0, CubemapFace.Unknown, -1));
            
            //这里设置m_ClearFlag为None，，这样什么都不会清除
            ConfigureClear(ClearFlag.None, Color.black);
		}

		Execute
		{
			//复制一份rt描述数据
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            int cameraSamples = descriptor.msaaSamples;

            CameraData cameraData = renderingData.cameraData;
            //判断是否开启MSAA，设置shader的变体宏开关

            //设置shader的全局属性 _CameraDepthAttachment
            cmd.SetGlobalTexture("_CameraDepthAttachment", source.Identifier());

            //shader全局属性 _ScaleBiasRt
            cmd.SetGlobalVector(ShaderPropertyId.scaleBiasRt, scaleBiasRt);
            //commanderbuffer绘制命令，
            cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, m_CopyDepthMaterial);

            //复制命令到context里
            context.ExecuteCommandBuffer(cmd);
		}

		OnCameraCleanup
		{
			//释放RT
            if (this.AllocateRT)
                cmd.ReleaseTemporaryRT(destination.id);
            //重置为帧缓冲
            destination = RenderTargetHandle.CameraTarget;
		}
	}
	——————————————————————————————————————————————————————————
	10 PostProcessPass:
	{
		分两种后期：
		{
			1 如果是finalPass，渲染结果 颜色缓冲和深度缓冲都绘制到帧缓冲
			2 如果是不是finalPass,渲染结果根据后期效果会使用到中间RT，不绘制深度信息
			{
				先把颜色缓冲绘制到 _AfterPostProcessTexture RT, 最后所有合并特效完成后绘制到 帧缓冲或者_CameraColorTexture
			}
		}

		Setup
		{
			//复制rt描述文件，设置useMipMap和autoGenerateMips为false
            m_Descriptor = baseDescriptor;
            m_Descriptor.useMipMap = false;
            m_Descriptor.autoGenerateMips = false;
            
            //ForwardRender的m_ActiveCameraColorAttachment，有可能是默认帧缓冲也有可能是RT _CameraColorTexture
            m_Source = source;
            //destination为RT _AfterPostProcessTexture 或者 默认帧缓冲
            m_Destination = destination;
            
            //ForwardRender的m_ActiveCameraDepthAttachment，有可能是默认帧缓冲也有可能是RT _CameraDepthAttachment
            m_Depth = depth;
            
            m_InternalLut = internalLut;
            m_IsFinalPass = false;
            
            //ForwardRender的applyFinalPostProcessing:当前相机开启后效&&当前相机是最后输出到屏幕的相机&&抗锯齿为FAXX
            //是否是最后的后期pass，如果这个为true，finalBlitPass就不需要执行
            m_HasFinalPass = hasFinalPass;
            
            //是否需要对sRGB做转换,destination为帧缓冲时就需要，destination为RT时不需要
            m_EnableSRGBConversionIfNeeded = enableSRGBConversion;
		}

		OnCameraSetup
		{
			//m_Destination为帧缓冲直接返回
            if (m_Destination == RenderTargetHandle.CameraTarget)
                return;

            if (m_Destination.HasInternalRenderTargetId())
                return;

            var desc = GetCompatibleDescriptor();
            
            //设置rt的渲染纹理深度缓冲区的精度，0表示不需要深度信息
            desc.depthBufferBits = 0;
            
            //创建一个RT， _AfterPostProcessTexture
            cmd.GetTemporaryRT(m_Destination.id, desc, FilterMode.Point);
		}

		Execute
		{
			//定义一系列后效
      
            if (m_IsFinalPass)
            {
                //如果是最后一个pass
                var cmd = CommandBufferPool.Get();
                using (new ProfilingScope(cmd, m_ProfilingRenderFinalPostProcessing))
                {
                    //最后一个pass
                    RenderFinalPass(cmd, ref renderingData)
                    {
                    	//finalPass材质的对象
			            var material = m_Materials.finalPass;
			            material.shaderKeywords = null;

			            // FXAA setup
			            if (cameraData.antialiasing == AntialiasingMode.FastApproximateAntialiasing)
			                material.EnableKeyword(ShaderKeywordStrings.Fxaa);
			            
			            //cameraData.cameraTargetDescriptor RT的描述文件对象
			            //设置shader全局属性 _SourceSize
			            PostProcessUtils.SetSourceSize(cmd, cameraData.cameraTargetDescriptor);
			            
			            //如果对应效果开启，设置shader关键字或贴图，shader用的是finalPass。这俩并不对应具体的shader。
			            //Film Grain
			            SetupGrain(cameraData, material);
			            SetupDithering(cameraData, material);
			            
			            //是否SRGB转换，有的设备不支持SRGB，需要在shader里转换下
			            if (RequireSRGBConversionBlitToBackBuffer(cameraData))
			                material.EnableKeyword(ShaderKeywordStrings.LinearToSRGBConversion);
			            // shader 全局属性 "_SourceTex"
			            cmd.SetGlobalTexture(ShaderPropertyId.sourceTex, m_Source.Identifier());
			            
			            //是否是默认视口（看到全屏）,true就直接RenderBufferLoadAction.DontCare，直接清除不需要知道上一帧数据，效率较高
			            var colorLoadAction = cameraData.isDefaultViewport ? RenderBufferLoadAction.DontCare : RenderBufferLoadAction.Load;
			            
			            //获取代表默认帧缓冲的RenderTargetHandle对象
			            RenderTargetHandle cameraTargetHandle = RenderTargetHandle.GetCameraTarget(cameraData.xr);

			            //如果相机上有targetTexture，就使用targetTexture对作为渲染目标，否则使用默认帧缓冲
		                RenderTargetIdentifier cameraTarget = (cameraData.targetTexture != null) ? new RenderTargetIdentifier(cameraData.targetTexture) : cameraTargetHandle.Identifier();
		                
		                //Add a "set active render target" command
		                //Render target to set for both color & depth buffers
		                //*******把渲染数据（颜色+深度）信息都绘制到cameraTarget上
		                cmd.SetRenderTarget(cameraTarget, colorLoadAction, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
		                //设置VP矩阵
		                cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
		                //设置V矩阵
		                cmd.SetViewport(cameraData.pixelRect);
		                //添加绘制命令 Add a "draw mesh" command
		                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, material);
		                cmd.SetViewProjectionMatrices(cameraData.camera.worldToCameraMatrix, cameraData.camera.projectionMatrix);

                	}
                }
                //复制命令到context里
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
            else if (CanRunOnTile())
            {
                // TODO: Add a fast render path if only on-tile compatible effects are used and we're actually running on a platform that supports it
                // Note: we can still work on-tile if FXAA is enabled, it'd be part of the final pass
            }
            else
            {
                //后期pass
         
                var cmd = CommandBufferPool.Get();
                using (new ProfilingScope(cmd, m_ProfilingRenderPostProcessing))
                {
                	//后期效果都在这
                    Render(cmd, ref renderingData);
                    {
                    	//后效的顺序都是固定的
			            /*
			                DepthOfField ;
			                MotionBlur ;
			                PaniniProjection ;
			                Bloom ;
			                LensDistortion ;
			                ChromaticAberration ;
			                Vignette ;
			                ColorLookup ;
			                ColorAdjustments ;
			                Tonemapping ;

			                ColorLookup/ColorAdjustments/Tonemapping 这三个统一用ColorGrading处理了
			 
			             */

			            //DoDepthOfField 使用了中间RT _HalfCoCTexture和_FullCoCTexture，最后输出到_TempTarget上

			            //DoMotionBlur  最终输出是_TempTarget2

			            //DoPaniniProjection 最终输出是_TempTarget

			            //这几种后效会是合并后效效果 最终输出都是m_Destination
                		//Bloom LensDistortion ChromaticAberration Vignette ColorGrading

                		//最后所有后效完成后在进行一次绘制，输出到帧缓冲或者_CameraColorTexture
                	}
                }
                //复制命令到context里
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
		}

		OnCameraCleanup
		{
			//m_Destination为帧缓冲直接返回
			if (m_Destination == RenderTargetHandle.CameraTarget)
                return;

            // Logic here matches the if check in OnCameraSetup
            if (m_Destination.HasInternalRenderTargetId())
                return;
            //释放RT
            cmd.ReleaseTemporaryRT(m_Destination.id);
		}
	}
	——————————————————————————————————————————————————————————
	11 CapturePass
	{
		Setup
		{
			//当前相机开启了后效，colorHandle为 Render的m_AfterPostProcessColor， RT _AfterPostProcessTexture
        	//当前相机未开启了后效，colorHandle为 Render的m_ActiveCameraColorAttachment，可能是RT _CameraColorTexture，也可能是默认帧缓冲
        	m_CameraColorHandle = colorHandle;
		}

		Execute
		{
			CommandBuffer cmdBuf = CommandBufferPool.Get();
            using (new ProfilingScope(cmdBuf, m_ProfilingSampler))
            {
                
                var colorAttachmentIdentifier = m_CameraColorHandle.Identifier();
                var captureActions = renderingData.cameraData.captureActions;
                //传到定义好的回调里处理, 参数为颜色附件rt以及commanderbuffer对象
                for (captureActions.Reset(); captureActions.MoveNext();)
                    captureActions.Current(colorAttachmentIdentifier, cmdBuf);
            }
            
            //回调处理完再次把命令复制到context里
            context.ExecuteCommandBuffer(cmdBuf);
            CommandBufferPool.Release(cmdBuf);
		}
	}
	——————————————————————————————————————————————————————————
	12 FinalBlitPass: camera的targetexture未设置，渲染结果最后绘制到帧缓冲，设置了就渲染到相机的targetTexture上
	{
		Setup
		{
			//当前相机开启了后效，colorHandle为 Render的m_AfterPostProcessColor， RT _AfterPostProcessTexture
            //当前相机未开启了后效，colorHandle为  Render的m_ActiveCameraColorAttachment，可能是RT _CameraColorTexture，也可能是默认帧缓冲
            m_Source = colorHandle;
		}

		Execute
		{
			ref CameraData cameraData = ref renderingData.cameraData;
            //cameraTarget为targetTexture，或者 默认帧缓冲
            RenderTargetIdentifier cameraTarget = (cameraData.targetTexture != null) ? new RenderTargetIdentifier(cameraData.targetTexture) : BuiltinRenderTextureType.CameraTarget;

         	/*
             RenderBufferLoadAction : 表示GUP渲染时对当前目标像素加载的操作, 当给目标纹理绘制的时候，目标纹理时有颜色的
             RenderBufferStoreAction：表示GUP渲染结束后对当前目标像素保存的操作
             */
            if (isSceneViewCamera || cameraData.isDefaultViewport)
            {
                //场景相机或者全屏视口，设置帧缓冲为颜色和深度数据的渲染目标
                //BuiltinRenderTextureType.CameraTarget 代表 Target texture of currently rendering camera
                //如果camera的targetTexure没有设置，就是默认帧缓冲，设置了就是相机的TargetTexure
                cmd.SetRenderTarget(BuiltinRenderTextureType.CameraTarget,
                    RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, // color
                    RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare); // depth
                //执行对应的pass，从m_Source输出到cameraTarget
                //Add a "blit into a render texture" command.
                //cameraTarget 大部分情况是BuiltinRenderTextureType.CameraTarget
                cmd.Blit(m_Source.Identifier(), cameraTarget, m_BlitMaterial);
            }
		}
	}
	——————————————————————————————————————————————————————————



}

5 Feature相关，继承ScriptableRendererFeature，自己实现create方法和AddRenderPasses方法就能加入自定义pass了
{
	在创建Render对象时，就会调用基类ScriptableRenderer的构造函数，根据data.rendererFeatures判断是否要创建自定义feature
	public ScriptableRenderer(ScriptableRendererData data)
    {
        profilingExecute = new ProfilingSampler($"{nameof(ScriptableRenderer)}.{nameof(ScriptableRenderer.Execute)}: {data.name}");
    
        foreach (var feature in data.rendererFeatures)
        {
            if (feature == null)
                continue;
            //调用feature的Create方法，并且加入到 m_RendererFeatures
            feature.Create();
            m_RendererFeatures.Add(feature);
        }
        Clear(CameraRenderType.Base);
        m_ActiveRenderPassQueue.Clear();
    }


    ForwardRender.Setup里调用feature的AddRenderPasses方法
    {
    	//RenderFeature Pass 调用每个激活featrure的 AddRenderPasses
    	AddRenderPasses
    	{
    		// Add render passes from custom renderer features
            for (int i = 0; i < rendererFeatures.Count; ++i)
            {
            	//不激活的直接跳过
                if (!rendererFeatures[i].isActive)
                {
                    continue;
                }
                //每个自定义的Feature需要自己实现AddRenderPasses
                rendererFeatures[i].AddRenderPasses(this, ref renderingData);
            }
    	}
    }

	——————————————————————————————————————————————————————————    
	RenderObjects：ScriptableRendererFeature
	{
		Create
		{
			//settings为序列化对象，面板上的配置
            FilterSettings filter = settings.filterSettings;

 			//限制最小事件
            if (settings.Event < RenderPassEvent.BeforeRenderingPrepasses)
                settings.Event = RenderPassEvent.BeforeRenderingPrepasses;
            
            //*****创建对应的Pass
            //filter.RenderQueueType 代表只渲染哪个队列，目前只有透明和不透明两种选择
            //filter.LayerMask 过滤层
            //filter.PassNames 绘制时使用的的pass，这里传入的是shader里"LightMode"对应的名字 Tags {"LightMode" = "xxxx"}
            //settings.cameraSettings 代表相机设置信息：
            renderObjectsPass = new RenderObjectsPass(settings.passTag, settings.Event, filter.PassNames,
                filter.RenderQueueType, filter.LayerMask, settings.cameraSettings);
            
            //是否重载材质信息
            renderObjectsPass.overrideMaterial = settings.overrideMaterial;
            renderObjectsPass.overrideMaterialPassIndex = settings.overrideMaterialPassIndex;
            
            //是否重载深度信息
            if (settings.overrideDepthState)
                renderObjectsPass.SetDetphState(settings.enableWrite, settings.depthCompareFunction);
            
            //是否重载模板信息
            if (settings.stencilSettings.overrideStencilState)
                renderObjectsPass.SetStencilState(settings.stencilSettings.stencilReference,
                    settings.stencilSettings.stencilCompareFunction, settings.stencilSettings.passOperation,
                    settings.stencilSettings.failOperation, settings.stencilSettings.zFailOperation);
		}

		AddRenderPasses
		{
			//把pass加入到渲染队列里
			renderer.EnqueuePass(renderObjectsPass);
		}

		//后面的操作就跟内置的pass一样了，会依次调用Pass的OnCameraSetup， Configure，Execute， FrameCleanup，OnFinishCameraStackRendering
	}

	RenderObjectsPass
	{
		Execute
		{
			//设置排序规则
            SortingCriteria sortingCriteria = (renderQueueType == RenderQueueType.Transparent)
                ? SortingCriteria.CommonTransparent
                : renderingData.cameraData.defaultOpaqueSortFlags;
            
            //创建绘制设置对象DrawingSettings,m_ShaderTagIdList里记录了使用的pass，并且把自定义pass加入到DrawingSettings对象里
            DrawingSettings drawingSettings = CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, sortingCriteria);
            drawingSettings.overrideMaterial = overrideMaterial;
            drawingSettings.overrideMaterialPassIndex = overrideMaterialPassIndex;

            //判断是否重载了相机设置，如果是，使用配置信息重新创建投影矩阵/重新计算视口矩阵/重新设置shader中变量使用

            //添加绘制几何体命令
                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref m_FilteringSettings,
                    ref m_RenderStateBlock);

            //把cmd里的命令复制到context上
            context.ExecuteCommandBuffer(cmd);
		}
	}
	——————————————————————————————————————————————————————————
}

6 ForwardRender多相机组合，一个base和多个overlay（overlay相机有激活的）Render的渲染目标是什么
{
	————————————————————————————————————————————————————————————————————————————————
	Render的渲染目标都是在Render的SetUp方法里设置的, Pass里也可以设置渲染目标
	最后在Render的SetRenderPassAttachments方法里会有判断
	{
		//默认是使用pass的colorAttachment和depthAttachment
		RenderTargetIdentifier passColorAttachment = renderPass.colorAttachment;
        RenderTargetIdentifier passDepthAttachment = renderPass.depthAttachment;

		if（！renderPass.overrideCameraTarget）
		{
			//未重载，直接使用ScriptableRenderer的渲染目标变量作为最后输出
			passColorAttachment = m_CameraColorTarget;
            passDepthAttachment = m_CameraDepthTarget;
		}
	}

	————————————————————————————————————————————————————————————————————————————————
	****先不考虑camera的targetTexture不为空的情况，大部分是targetTexture为空的

	设置Render的渲染目标
	{
		1 颜色缓冲区的渲染目标，默认是帧缓冲 
		{
			1 对于非Preview相机来说，使用的渲染器对象(Render对象)中有自定义的feature，就会产生中间RT _CameraColorTexture，颜色缓冲区的渲染目标就是 _CameraColorTexture

			2 BaseCamera，一定要是非Preview相机，基本都满足
			{
				对应render的createColorTexture变量为true就会产生RT 并设置颜色缓冲渲染目标为RT，满足以下一个就行
				{
					1 渲染器对象(Render对象)中有自定义的feature
					2 RequiresIntermediateColorTexture 返回true，在一个base多overlay且有激活overlay下这个方法一定返回true ******
					3 renderPassInputs.requiresColorTexture 绝大部分返回false
				}

				********总结：BaseCamera在这种情况下Render的颜色缓冲区目标都会被设置为RT  _CameraColorTexture
				
			}
			3 overlay相机
			{
				if(渲染器对象(Render对象)中有自定义的feature，（如果和base相机使用同样的渲染器对象）)
				{
					会产生RT 并设置颜色缓冲渲染目标为RT _CameraColorTexture
				}
				else
				{
					直接使用BaseCamera设置过的颜色缓冲渲染目标，因为base相机的颜色缓冲渲染目标一定为RT，所以overlay的颜色渲染目标也为RT
				}

				********总结：overlay相机在这种情况下Render的颜色缓冲区目标都会被设置为RT  _CameraColorTexture

			}
		}

		2 深度缓冲区渲染目标，默认是帧缓冲
		{
			1 BaseCamera，是根据createDepthTexture变量来判断是否产生RT的， 满足以下中一个就行
			{
				1 cameraData.requiresDepthTexture && !requiresDepthPrepass
				{
					cameraData.requiresDepthTexture的逻辑判断
					{
						* if(配置文件勾选了开启获取深度)
			             * {
			             *     return true;
			             * }
			             * else
			             * {
			             *    if(相机为场景相机)
			             *     {
			             *        return true;
			             *     }
			             *     else
			             *     {
			             *         if(相机未开启后效)
			             *             return false;
			             *         if(相机的抗锯齿模式 == SMAA)
			             *             return true;
			             *         if(开启后效里有运动模糊和景深效果)
			             *             return true;
			             *         return false
			             *     }
			             * }
			             */
					}

					requiresDepthPrepass，满足以下一个就为true，感觉就是cameraData.requiresDepthTexture和是否开启msaa来决定的
					{
						requiresDepthTexture && !CanCopyDepth(ref renderingData.cameraData)
						{
							bool requiresDepthTexture = cameraData.requiresDepthTexture || renderPassInputs.requiresDepthTexture || this.actualRenderingMode == RenderingMode.Deferred;
							{
								cameraData.requiresDepthTexture 跟之前的逻辑一样
								renderPassInputs.requiresDepthTexture 一般为false
								this.actualRenderingMode == RenderingMode.Deferred 一般手机上都为前向渲染，基本返回false

							}

							CanCopyDepth 只要配置文件上开启了msaa，CanCopyDepth就返回false，关闭msaa就返回ture
						}

						isSceneViewCamera
						isPreviewCamera
						renderPassInputs.requiresDepthPrepass; //一般为false
						renderPassInputs.requiresNormalsTexture; //一般为false
					}
				}

				2 (cameraData.renderType == CameraRenderType.Base && !cameraData.resolveFinalTarget); 基本都返回true ****
				{
					base相机不是最后一个激活的相机，对于一个base多overlay模式基本都满足
				}
				3 createDepthTexture |= this.actualRenderingMode == RenderingMode.Deferred; 手机上基本都返回false 


				********总结：BaseCamera在这种情况下Render的深度缓冲区目标都会被设置为RT _CameraDepthAttachment
			}

			2 overlay Camera，
			{
				直接使用BaseCamera设置过的深度缓冲渲染目标，因为BaseCamera在这种情况下Render的深度缓冲区目标都会被设置为RT，所以overlay相机也是一样的深度缓冲渲染目标

				********总结：overlay Camera在这种情况下Render的深度缓冲区目标都会被设置为RT _CameraDepthAttachment
			}
		}

		********总结：base相机和overlay相机对应的render对象
						颜色缓冲区的渲染目标为RT _CameraColorTexture  深度缓冲区渲染目标为RT _CameraDepthAttachment，但是最后的输出还要看对应的pass是否要使用自己的渲染目标对象

	}
	
}

7 BaseCamera多个后效怎样叠加的 一个base和多个overlay（overlay相机有激活的） 不是最后一个后效Pass的情况
{
	BaseCamera这种情况对应PostProcessPass的SetUp
	{
		目标destination 为m_AfterPostProcessColor 后效RT _AfterPostProcessTexture
		m_Source为 m_ActiveCameraColorAttachment,==》 RT  _CameraColorTexture


	}

	PostProcessPass的Render方法（不是最后一个后期pass会调用）
	{
		//使用camera上的targetexture，或者帧缓冲
        RenderTargetHandle cameraTargetHandle = RenderTargetHandle.GetCameraTarget(cameraData.xr);
        RenderTargetIdentifier cameraTarget = (cameraData.targetTexture != null && !cameraData.xr.enabled) ? new RenderTargetIdentifier(cameraData.targetTexture) : cameraTargetHandle.Identifier();
        
        //使用外部变量m_Destination再次判断下cameraTarget
        //如果m_Destination为默认帧缓冲，cameraTarget保持不变，否则替换为m_Destination代表目标
        cameraTarget = (m_Destination == RenderTargetHandle.CameraTarget) ? cameraTarget : m_Destination.Identifier();

        //使用相机堆叠，我们并不总是解析到最终屏幕，因为我们可能会在堆栈的中间运行后处理
        bool finishPostProcessOnScreen = cameraData.resolveFinalTarget || (m_Destination == cameraTargetHandle || m_HasFinalPass == true);

        //深度和颜色信息都绘制到cameraTarget上
        cmd.SetRenderTarget(cameraTarget, colorLoadAction, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
        cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);

        总结：一个base和多个overlay（overlay相机有激活的） 不是最后一个后效Pass的情况下
        {
        	//DoDepthOfField 最终输出是_HalfCoCTexture和_FullCoCTexture

            //DoMotionBlur  最终输出是_TempTarget或者_TempTarget2

            //DoPaniniProjection 最终输出是_TempTarget或者_TempTarget2

            //这几种后效会是合并后效效果 最终输出都是_AfterPostProcessTexture
    		//Bloom LensDistortion ChromaticAberration Vignette ColorGrading

    		//如果不是最后一个后期pass，设置cameraTarget为 shader的_SourceTex
            cmd.SetGlobalTexture(ShaderPropertyId.sourceTex, cameraTarget);
            
            //最后设置渲染目标为m_Source，进行最后一次绘制命令 m_Source 其实就是_CameraColorTexture，所以最后还是会从cameraTarget绘制到_CameraColorTexture
            cmd.SetRenderTarget(m_Source.id, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
            cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, m_BlitMaterial);
        }
	}

	所有后效完成后都要从_AfterPostProcessTexture绘制到 _CameraColorTexture上
}

8 如果把后效的中间RT _AfterPostProcessTexture去掉，直接绘制在帧缓冲是否会有正确表现
{
	表现不正确，如果把baseCamera的后效数据直接绘制到帧缓冲，
	overlay相机执行FinalBlitPass时使用的原始数据都来自RT _CameraColorTexture，最后相机的绘制到屏幕时候是不包含后效效果的

	********除非overlay相机不执行FinalBlitPass，也直接绘制到帧缓冲

}

9 多个相机时是否能减少blit次数（减少后效拷贝到_AfterPostProcessTexture，减少最后一次FinalBlitPass），模糊好像不支持了
{
	因为开启模糊，overlay相机需要使用base相机输出到_CameraColorTexture的数据进行模糊处理后再次blit到_CameraColorTexture中，
	最后还要执行FinalBlitPass，把_CameraColorTexture中的数据拷贝到帧缓冲中

	修正
	{
		BaseCamera还是输出到_CameraColorTexture，进行模糊后直接输出到帧缓冲，中间的UICamera输出到_CameraColorTexture，最后一个UICamera输出到帧缓冲
			不能减少后效拷贝到_AfterPostProcessTexture，但是模糊shader的最后输出必须是帧缓冲，可以减少最后一次FinalBlitPass
	}
}

10 多个base相机，渲染的先后顺序
{
	****相机上的Priority其实就是Camera.depth的意思

	优先比较targetTexture属性，不为空的优先渲染
	再次比较Camera.depth，低的先渲染

	当两个targetTexture属性都不为空的时候，比较Camera.depth 

	都一样的时候就没法确定了。。。。
}


--[==[
	1 默认不渲染枪械层，非枪械层，进行模板测试，只有0的才通过 （刚开始模板值都为0）
	2 第一个自定义pass RenderObjectFeature==》 不透明物体，在所有不透明物体渲染之前执行，只渲染枪械层，模板缓冲区模板值置为1
	3 第二个自定义pass RenderObjectFeature==》 透明物体，在所有透明和不透明物体渲染完执行，只渲染枪械层，进行通过模板测试 只有模板值为1的才通过，模板缓冲区值保持不变
	4 第三个自定义pass RenderObjectFeature==》 透明物体 在所有透明和不透明物体渲染完执行，只渲染枪械层，进行通过模板测试 只有模板值为0的才通过，模板缓冲区值保持不变 并开启深度检测进行深度写入
]==]
