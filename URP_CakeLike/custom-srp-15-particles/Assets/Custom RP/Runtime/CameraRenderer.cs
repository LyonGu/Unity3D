using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer {

	const string bufferName = "Render Camera";

	static ShaderTagId
		unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit"),
		litShaderTagId = new ShaderTagId("CustomLit");

	//颜色缓冲区，深度缓冲
	/*
	 * 到目前为止，我们一直为相机使用单个帧缓冲区，其中包含颜色和深度信息。
	 * 这是典型的帧缓冲区配置，但是颜色和深度数据始终存储在单独的缓冲区中，称为帧缓冲区附件。要访问深度缓冲区，我们需要分开定义这些附件
	 *
	 * 我们不能在深度缓冲区用于渲染的同时对其进行采样。我们需要复制它。因此，引入_CameraDepthTexture标识符，并添加一个布尔值字段以指示我们是否正在使用深度纹理。
	 * 仅应在需要时才考虑复制深度，这将在获取相机设置后在Render中确定。但是我们一开始只是始终启用它。
	 */
	static int
		colorAttachmentId = Shader.PropertyToID("_CameraColorAttachment"),
		depthAttachmentId = Shader.PropertyToID("_CameraDepthAttachment"),
		colorTextureId = Shader.PropertyToID("_CameraColorTexture"),
		depthTextureId = Shader.PropertyToID("_CameraDepthTexture"),
		sourceTextureId = Shader.PropertyToID("_SourceTexture"),
		srcBlendId = Shader.PropertyToID("_CameraSrcBlend"),
		dstBlendId = Shader.PropertyToID("_CameraDstBlend");

	static CameraSettings defaultCameraSettings = new CameraSettings();

	static bool copyTextureSupported =
		SystemInfo.copyTextureSupport > CopyTextureSupport.None;

	CommandBuffer buffer = new CommandBuffer {
		name = bufferName
	};

	ScriptableRenderContext context;

	Camera camera;

	CullingResults cullingResults;

	Lighting lighting = new Lighting();

	PostFXStack postFXStack = new PostFXStack();

	bool useHDR;

	bool useColorTexture, useDepthTexture, useIntermediateBuffer;

	Material material;

	Texture2D missingTexture;

	public CameraRenderer (Shader shader) {
		material = CoreUtils.CreateEngineMaterial(shader);
		missingTexture = new Texture2D(1, 1) {
			hideFlags = HideFlags.HideAndDontSave,
			name = "Missing"
		};
		missingTexture.SetPixel(0, 0, Color.white * 0.5f);
		missingTexture.Apply(true, true);
	}

	public void Dispose () {
		CoreUtils.Destroy(material);
		CoreUtils.Destroy(missingTexture);
	}

	public void Render (
		ScriptableRenderContext context, Camera camera,
		CameraBufferSettings bufferSettings,
		bool useDynamicBatching, bool useGPUInstancing, bool useLightsPerObject,
		ShadowSettings shadowSettings, PostFXSettings postFXSettings,
		int colorLUTResolution
	) {
		this.context = context;
		this.camera = camera;

		var crpCamera = camera.GetComponent<CustomRenderPipelineCamera>();
		CameraSettings cameraSettings =
			crpCamera ? crpCamera.Settings : defaultCameraSettings;

		if (camera.cameraType == CameraType.Reflection) {
			useColorTexture = bufferSettings.copyColorReflection;
			useDepthTexture = bufferSettings.copyDepthReflection;
		}
		else {
			useColorTexture = bufferSettings.copyColor && cameraSettings.copyColor;
			useDepthTexture = bufferSettings.copyDepth && cameraSettings.copyDepth;
		}

		if (cameraSettings.overridePostFX) {
			postFXSettings = cameraSettings.postFXSettings;
		}

		PrepareBuffer();
		PrepareForSceneWindow();
		if (!Cull(shadowSettings.maxDistance)) {
			return;
		}
		useHDR = bufferSettings.allowHDR && camera.allowHDR;

		buffer.BeginSample(SampleName);
		ExecuteBuffer();
		lighting.Setup(
			context, cullingResults, shadowSettings, useLightsPerObject,
			cameraSettings.maskLights ? cameraSettings.renderingLayerMask : -1
		);
		postFXStack.Setup(
			context, camera, postFXSettings, useHDR, colorLUTResolution,
			cameraSettings.finalBlendMode
		);
		buffer.EndSample(SampleName);
		Setup();
		DrawVisibleGeometry(
			useDynamicBatching, useGPUInstancing, useLightsPerObject,
			cameraSettings.renderingLayerMask
		);
		DrawUnsupportedShaders();
		DrawGizmosBeforeFX();
		if (postFXStack.IsActive) {
			postFXStack.Render(colorAttachmentId);
		}
		else if (useIntermediateBuffer) {
			DrawFinal(cameraSettings.finalBlendMode);
			ExecuteBuffer();
		}
		DrawGizmosAfterFX();
		Cleanup();
		Submit();
	}

	bool Cull (float maxShadowDistance) {
		if (camera.TryGetCullingParameters(out ScriptableCullingParameters p)) {
			p.shadowDistance = Mathf.Min(maxShadowDistance, camera.farClipPlane);
			cullingResults = context.Cull(ref p);
			return true;
		}
		return false;
	}

	void Setup () {
		context.SetupCameraProperties(camera);
		CameraClearFlags flags = camera.clearFlags;

		useIntermediateBuffer =
			useColorTexture || useDepthTexture || postFXStack.IsActive;
		if (useIntermediateBuffer) {
			if (flags > CameraClearFlags.Color) {
				flags = CameraClearFlags.Color;
			}
			
			//获得两个独立缓冲区
			
			//颜色缓冲区
			buffer.GetTemporaryRT(
				colorAttachmentId, camera.pixelWidth, camera.pixelHeight,
				0, FilterMode.Bilinear, useHDR ?
					RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default
			);
			
			//深度缓冲区
			buffer.GetTemporaryRT(
				depthAttachmentId, camera.pixelWidth, camera.pixelHeight,
				32, FilterMode.Point, RenderTextureFormat.Depth
			);
			buffer.SetRenderTarget(
				colorAttachmentId,
				RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
				depthAttachmentId,
				RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store
			);
		}

		buffer.ClearRenderTarget(
			flags <= CameraClearFlags.Depth,
			flags == CameraClearFlags.Color,
			flags == CameraClearFlags.Color ?
				camera.backgroundColor.linear : Color.clear
		);
		buffer.BeginSample(SampleName);
		buffer.SetGlobalTexture(colorTextureId, missingTexture);
		buffer.SetGlobalTexture(depthTextureId, missingTexture);
		ExecuteBuffer();
	}

	void Cleanup () {
		lighting.Cleanup();
		if (useIntermediateBuffer) {
			buffer.ReleaseTemporaryRT(colorAttachmentId);
			buffer.ReleaseTemporaryRT(depthAttachmentId);
			if (useColorTexture) {
				buffer.ReleaseTemporaryRT(colorTextureId);
			}
			if (useDepthTexture) {
				buffer.ReleaseTemporaryRT(depthTextureId);
			}
		}
	}

	void Submit () {
		buffer.EndSample(SampleName);
		ExecuteBuffer();
		context.Submit();
	}

	void ExecuteBuffer () {
		context.ExecuteCommandBuffer(buffer);
		buffer.Clear();
	}

	void DrawVisibleGeometry (
		bool useDynamicBatching, bool useGPUInstancing, bool useLightsPerObject,
		int renderingLayerMask
	) {
		PerObjectData lightsPerObjectFlags = useLightsPerObject ?
			PerObjectData.LightData | PerObjectData.LightIndices :
			PerObjectData.None;
		var sortingSettings = new SortingSettings(camera) {
			criteria = SortingCriteria.CommonOpaque
		};
		var drawingSettings = new DrawingSettings(
			unlitShaderTagId, sortingSettings
		) {
			enableDynamicBatching = useDynamicBatching,
			enableInstancing = useGPUInstancing,
			perObjectData =
				PerObjectData.ReflectionProbes |
				PerObjectData.Lightmaps | PerObjectData.ShadowMask |
				PerObjectData.LightProbe | PerObjectData.OcclusionProbe |
				PerObjectData.LightProbeProxyVolume |
				PerObjectData.OcclusionProbeProxyVolume |
				lightsPerObjectFlags
		};
		drawingSettings.SetShaderPassName(1, litShaderTagId);

		var filteringSettings = new FilteringSettings(
			RenderQueueRange.opaque, renderingLayerMask: (uint)renderingLayerMask
		);
		//绘制不透明
		context.DrawRenderers(
			cullingResults, ref drawingSettings, ref filteringSettings
		);
		//绘制天空盒
		context.DrawSkybox(camera);
		
		//拷贝颜色缓冲和深度缓冲
		if (useColorTexture || useDepthTexture) {
			CopyAttachments();
		}

		sortingSettings.criteria = SortingCriteria.CommonTransparent;
		drawingSettings.sortingSettings = sortingSettings;
		filteringSettings.renderQueueRange = RenderQueueRange.transparent;
		
		//绘制透明物体
		context.DrawRenderers(
			cullingResults, ref drawingSettings, ref filteringSettings
		);
	}

	void CopyAttachments () {
		if (useColorTexture) {
			buffer.GetTemporaryRT(
				colorTextureId, camera.pixelWidth, camera.pixelHeight,
				0, FilterMode.Bilinear, useHDR ?
					RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default
			);
			if (copyTextureSupported) {
				buffer.CopyTexture(colorAttachmentId, colorTextureId);
			}
			else {
				Draw(colorAttachmentId, colorTextureId);
			}
		}
		if (useDepthTexture) {
			//复制深度数据
			/*
			 * 将在需要时获取一个临时的重复深度纹理，并将深度附件数据复制到其中。这可以通过在命令缓冲区上使用源纹理和目标纹理调用CopyTexture来完成
			 * 这比通过全屏draw call进行操作要有效得多。另外，请确保在Cleanup中释放额外的深度纹理。
			 */
			buffer.GetTemporaryRT(
				depthTextureId, camera.pixelWidth, camera.pixelHeight,
				32, FilterMode.Point, RenderTextureFormat.Depth
			);
			if (copyTextureSupported) {
				buffer.CopyTexture(depthAttachmentId, depthTextureId);
			}
			else {
				Draw(depthAttachmentId, depthTextureId, true);
			}
		}
		if (!copyTextureSupported) {
			buffer.SetRenderTarget(
				colorAttachmentId,
				RenderBufferLoadAction.Load, RenderBufferStoreAction.Store,
				depthAttachmentId,
				RenderBufferLoadAction.Load, RenderBufferStoreAction.Store
			);
		}
		//重置渲染目标并执行一次缓冲区
		ExecuteBuffer();
	}

	void Draw (
		RenderTargetIdentifier from, RenderTargetIdentifier to, bool isDepth = false
	) {
		buffer.SetGlobalTexture(sourceTextureId, from);
		buffer.SetRenderTarget(
			to, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store
		);
		buffer.DrawProcedural(
			Matrix4x4.identity, material, isDepth ? 1 : 0, MeshTopology.Triangles, 3
		);
	}

	void DrawFinal (CameraSettings.FinalBlendMode finalBlendMode) {
		buffer.SetGlobalFloat(srcBlendId, (float)finalBlendMode.source);
		buffer.SetGlobalFloat(dstBlendId, (float)finalBlendMode.destination);
		buffer.SetGlobalTexture(sourceTextureId, colorAttachmentId);
		buffer.SetRenderTarget(
			BuiltinRenderTextureType.CameraTarget,
			finalBlendMode.destination == BlendMode.Zero ?
				RenderBufferLoadAction.DontCare : RenderBufferLoadAction.Load,
			RenderBufferStoreAction.Store
		);
		buffer.SetViewport(camera.pixelRect);
		buffer.DrawProcedural(
			Matrix4x4.identity, material, 0, MeshTopology.Triangles, 3
		);
		buffer.SetGlobalFloat(srcBlendId, 1f);
		buffer.SetGlobalFloat(dstBlendId, 0f);
	}
}