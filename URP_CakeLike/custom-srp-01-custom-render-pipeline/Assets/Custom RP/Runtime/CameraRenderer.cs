using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer {

	const string bufferName = "Render Camera";

	static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");

    /*
        我们对其进行配置并向其添加命令以供后续的执行。某些任务（例如绘制天空盒）提供了专属方法，
        但其他命令则必须通过单独的命令缓冲区（command buffer）间接执行。我们需要用这样的缓冲区来绘制场景中的其他几何图形。 
    */
   
    //必须创建一个新的CommandBuffer对象实例
    CommandBuffer buffer = new CommandBuffer {
		name = bufferName
	};

	ScriptableRenderContext context;

	Camera camera;

	CullingResults cullingResults;

	public void Render (ScriptableRenderContext context, Camera camera) {
		this.context = context;
		this.camera = camera;

        //如果每个相机都有自己的镜头，那就更清楚了。为此，添加一个仅编辑器能用的PrepareBuffer方法，使缓冲区的名称与摄像机的名称相等。
        PrepareBuffer();

        //渲染场景窗口
        PrepareForSceneWindow();

        //进行裁剪判断，失败就终止
		if (!Cull()) {
			return;
		}

		Setup();

        //绘制所有可见的几何体
        DrawVisibleGeometry();

        //绘制所有不受支持的着色器
        DrawUnsupportedShaders();

        //绘制Gizmos
        DrawGizmos();
		Submit();
	}

	bool Cull () {

        /*
         找出什么可以被剔除需要我们跟踪多个相机设置和矩阵，可以使用ScriptableCullingParameters结构。
         这个结构可以在摄像机上调用TryGetCullingParameters，而不是自己去填充它。它返回是否可以成功检索该参数，因为它可能会获取失败。
         要获得参数数据，我们必须将其作为输出(out)参数提供，方法是在它前面写一个out。在返回成功或失败的单独的Cull方法中执行此操作
         */
        if (camera.TryGetCullingParameters(out ScriptableCullingParameters p)) {

            /*
                实际的裁剪是通过调用上下文上的Cull来完成的，这会产生一个CullingResults结构。
                如果成功的话，可以在清除中执行此操作，并将结果存储在字段中。
                在这种情况下，我们必须将剔除参数作为引用参数传递，方法是在前面写ref。
             */
            cullingResults = context.Cull(ref p);
			return true;
		}
		return false;
	}

	void Setup () {

        //设置矩阵以及其他一些属性 unity_MatrixVP 视图投影矩阵
        context.SetupCameraProperties(camera);

        //清除渲染目标
        /*
            无论我们画了什么，最终都会被渲染到摄像机的渲染目标上，默认情况下，是帧缓冲区，但也可能是渲染纹理。
            但是所有之前已经画过的东西仍然存在，这可能会干扰现在渲染的图像。
            为了保证正确的渲染，我们必须清除渲染目标，以消除其旧的内容。通过调用命令缓冲区上的ClearRenderTarget来完成的
         */
        /*
           如果我们要清除一个不透明的颜色，就要使用到相机的背景色。但是因为我们是在线性颜色空间中绘制，所以我们必须把颜色转换到线性空间，
           所以我们最终需要camera.backgroundColor.linear。在所有其他情况下，颜色都不重要，所以使用Color.clear就足够了。 
        */
        CameraClearFlags flags = camera.clearFlags;
		buffer.ClearRenderTarget(
			flags <= CameraClearFlags.Depth, //是否应该清除深度
            flags == CameraClearFlags.Color, //是否应该清除颜色
            flags == CameraClearFlags.Color ? //用于清除的颜色，显示清除时的颜色
                camera.backgroundColor.linear : Color.clear
		);

        //可以使用命令缓冲区注入给Profiler注入样本，这些样本将同时显示在Profiler和帧调试器中
        buffer.BeginSample(SampleName);

		ExecuteBuffer();
	}

	void Submit () {
        //可以使用命令缓冲区注入给Profiler注入样本，这些样本将同时显示在Profiler和帧调试器中
        buffer.EndSample(SampleName);

		ExecuteBuffer();
		context.Submit(); //向上下文发出的命令都在缓冲区，需要执行submit方法才能提交
    }

	void ExecuteBuffer () {

        //要执行缓冲区，需以缓冲区为参数在上下文上调用ExecuteCommandBuffer
        //这会从缓冲区复制命令但并不会清除它，如果要重用它的话，就必须在之后明确地执行该操作。因为执行和清除总是一起完成的
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
	}

    //首先绘制不透明对象，然后是Skybox，然后才是透明对象
    void DrawVisibleGeometry () {
		var sortingSettings = new SortingSettings(camera) {
			criteria = SortingCriteria.CommonOpaque
		};

        //本教程中我们只支持unlit 的着色器，所以我们必须获取SRPDefaultUnlitPass的着色器标签ID
        //一个新的SortingSettings结构值。将相机传递给SortingSettings的构造函数，它用于确定基于正焦还是基于透视的应用排序
        var drawingSettings = new DrawingSettings(
			unlitShaderTagId, sortingSettings
		);

        //指出哪些 render 队列是允许的,这里只绘制不透明物体
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

        /*
            一旦我们知道什么是可见的，我们就可以继续渲染它们。这是通过调用上下文中的DrawRenderers作为参数来实现的，并告诉它要使用哪个renderers 。
            此外，我们还必须提供绘图设置和筛选设置。这两种都是结构体DrawingSettings和FilteringSettings
         */
        context.DrawRenderers(
			cullingResults, ref drawingSettings, ref filteringSettings
		);

        //绘制天空盒
		context.DrawSkybox(camera);

        //绘制不透明物体的设置
		sortingSettings.criteria = SortingCriteria.CommonTransparent;
		drawingSettings.sortingSettings = sortingSettings;
		filteringSettings.renderQueueRange = RenderQueueRange.transparent;

		context.DrawRenderers(
			cullingResults, ref drawingSettings, ref filteringSettings
		);
	}
}