
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

                	//override 方法 设置裁剪参数 每个Render可以自己实现, 一般是调用forwardRender的SetupCullingParameters方法
                    renderer.SetupCullingParameters(ref cullingParameters, ref cameraData);

                    //将commanderbuffer中所有的命令都发给context，其实是一个复制的过程
                    context.ExecuteCommandBuffer(cmd); // Send all the commands enqueued so far in the CommandBuffer cmd, to the ScriptableRenderContext context
                    //发送完清除，可复用对象
                	cmd.Clear();

                	//得到裁剪结果
                	var cullResults = context.Cull(ref cullingParameters);

                	//填充RenderingData结构体，这个结构体包含渲染相关的所有参数，裁剪信息，相机数据，光照阴影数据，后处理等很多
                	InitializeRenderingData(asset, ref cameraData, ref cullResults, anyPostProcessingEnabled, out var renderingData);

                	//抽象方法 每个Render自己实现 一般是调用forwardRender的Setup方法
                    renderer.Setup(context, ref renderingData);

                    // Timing scope inside 一般是调用forwardRender的Execute方法
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
								==》****对应Render的SetUp和Excute方法，这里可以自定义pass实现重载，默认是走forwardRender的方法
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

		真正设置渲染目标，是通过CommandBuffer的SetRenderTarget方法，URP在CoreUtils类封装了一个静态函数SetRenderTarget。ScriptableRenderer类在ExecuteRenderPass方法中，先调用pass的Config函数，然后取pass的color和depth数据，设置为真正的渲染目标。
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

	}
}

