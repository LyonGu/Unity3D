
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
	}

	6 设置渲染器配置文件
	{
		GraphicsSettings.renderPipelineAsset = pipelineAsset;
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

]==]
