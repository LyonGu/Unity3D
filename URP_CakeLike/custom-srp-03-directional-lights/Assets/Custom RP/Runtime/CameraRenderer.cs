﻿using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer {

	const string bufferName = "Render Camera";

	//这里的SRPDefaultUnlit和CustomLit都是LightMode标签
	//SRPDefaultUnlit是内置的tag
	/*
	 * Tags {
				"LightMode" = "CustomLit"
			}
	 * 
	 */
	static ShaderTagId
		unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit"),
		litShaderTagId = new ShaderTagId("CustomLit");

	CommandBuffer buffer = new CommandBuffer {
		name = bufferName
	};

	ScriptableRenderContext context;

	Camera camera;

	CullingResults cullingResults;

	Lighting lighting = new Lighting();

	public void Render (
		ScriptableRenderContext context, Camera camera,
		bool useDynamicBatching, bool useGPUInstancing
	) {
		this.context = context;
		this.camera = camera;

		PrepareBuffer();
		PrepareForSceneWindow();
		if (!Cull()) {
			return;
		}

		Setup();
		lighting.Setup(context, cullingResults);
		DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
		DrawUnsupportedShaders();
		DrawGizmos();
		Submit();
	}

	bool Cull () {
		if (camera.TryGetCullingParameters(out ScriptableCullingParameters p)) {
			cullingResults = context.Cull(ref p);
			return true;
		}
		return false;
	}

	void Setup () {
		context.SetupCameraProperties(camera);
		CameraClearFlags flags = camera.clearFlags;
		//buffer里有一条clear命令，需要调用context.ExecuteCommandBuffer(buffer);才能执行
		buffer.ClearRenderTarget(
			flags <= CameraClearFlags.Depth,
			flags == CameraClearFlags.Color,
			flags == CameraClearFlags.Color ?
				camera.backgroundColor.linear : Color.clear
		);
		buffer.BeginSample(SampleName);
		ExecuteBuffer();
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

	void DrawVisibleGeometry (bool useDynamicBatching, bool useGPUInstancing) {
		var sortingSettings = new SortingSettings(camera) {
			criteria = SortingCriteria.CommonOpaque
		};
		//unlitShaderTagId 为 0号pass
		var drawingSettings = new DrawingSettings(
			unlitShaderTagId, sortingSettings
		) {
			enableDynamicBatching = useDynamicBatching,
			enableInstancing = useGPUInstancing
		};
		
		//1号pass
		drawingSettings.SetShaderPassName(1, litShaderTagId);

		var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

		context.DrawRenderers(
			cullingResults, ref drawingSettings, ref filteringSettings
		);

		context.DrawSkybox(camera);

		sortingSettings.criteria = SortingCriteria.CommonTransparent;
		drawingSettings.sortingSettings = sortingSettings;
		filteringSettings.renderQueueRange = RenderQueueRange.transparent;

		context.DrawRenderers(
			cullingResults, ref drawingSettings, ref filteringSettings
		);
	}
}