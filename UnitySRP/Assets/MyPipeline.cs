using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Conditional = System.Diagnostics.ConditionalAttribute;

public class MyPipeline : RenderPipeline
{
  
    private CullingResults cull;
    private CommandBuffer buffer;
    private Material errorMaterial;

    private bool _dynamicBatching = false;
    private bool _instancing = false;

    public MyPipeline(bool dynamicBatching, bool instancing)
    {
        _dynamicBatching = dynamicBatching;
        _instancing = instancing;
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        //把command传入了buffer 实际起效需要我们通过submit函数submit 这些commands
        //context.DrawSkybox(cameras[0]);
        //context.Submit();
        //base.Render(context, cameras);
        if (buffer == null)
        {
            buffer = new CommandBuffer()
            {
                name = "Render Camera"
            };
        }

        foreach (var camera in cameras)
        {
            //Render(context, camera);
            Render1(context, camera);
        }
    }


    void Render(ScriptableRenderContext context, Camera camera)
    {
        //得到裁剪的配置信息
        ScriptableCullingParameters cullingParameters;
        camera.TryGetCullingParameters(out cullingParameters);
        if (cullingParameters == null) return;
        cullingParameters.isOrthographic = false;

        //得到裁剪的配置信息后，通弄过context.Cull函数可以得到最终的裁剪结果
        cull = context.Cull(ref cullingParameters);

        context.SetupCameraProperties(camera);



        /*  
            执行一个空的command buffer没有意义，我们加入command buffer是为了清除render target去保证之前渲染的东西不会影响到目前。
            我们向buffer中加入一个clear command通过调用ClearRenderTarget函数，
            它包括三个参数，第一个参数是否清除depth ，第二个参数是否清除color，第三个参数是将缓冲区清除成什么颜色。
            buffer.ClearRenderTarget(true, true, Color.clear);
         */

        CameraClearFlags clearflags = camera.clearFlags;
        bool isClearDepth = (clearflags & CameraClearFlags.Depth) != 0;
        bool isClearColor = (clearflags & CameraClearFlags.Color) != 0;
        buffer.ClearRenderTarget(isClearDepth, isClearColor, camera.backgroundColor);

        
        //ExecuteCommandBuffer函数将command传入context的内部，等待submit后执行  
        context.ExecuteCommandBuffer(buffer);

        //command buffer会消耗unity native层的内存资源，当我们不再需要它的时候要立刻释放掉。Release函数可以完成这个功能。
        buffer.Clear();

        

        //渲染设置
        SortingSettings sorting = new SortingSettings(camera) {
            criteria = SortingCriteria.OptimizeStateChanges
        };
        ShaderTagId shaderTagId = new ShaderTagId("ForwardBase"); // 使用LightMode的类型
        var drawSetting = new DrawingSettings(shaderTagId, sorting);

        //渲染过滤设置，这里设置成所有的都渲染
        //这边是指定渲染的种类(对应shader中的Rendertype)和相关Layer的设置(-1表示全部layer)
        var filterSetting = new FilteringSettings(RenderQueueRange.all, -1);
        ////var filterSetting = new FilteringSettings(RenderQueueRange.all) {
        ////    layerMask = 1 << LayerMask.GetMask("opaque2")  //感觉没有任何用
        ////};

        ////相当于把渲染command放入context
        context.DrawRenderers(cull, ref drawSetting, ref filterSetting);

        shaderTagId = new ShaderTagId("Always"); // 使用LightMode的类型
        drawSetting = new DrawingSettings(shaderTagId, sorting);

        context.DrawRenderers(cull, ref drawSetting, ref filterSetting);

        //相当于把渲染command放入context
        context.DrawSkybox(camera);
        context.Submit();
    }




    void Render1(ScriptableRenderContext context, Camera camera)
    {
        //得到裁剪的配置信息
        ScriptableCullingParameters cullingParameters;
        camera.TryGetCullingParameters(out cullingParameters);
        if (cullingParameters == null) return;
        cullingParameters.isOrthographic = false;


        /*
            我们不用做任何处理，就可以在game视图中正确显示UI，
            这些Unity已经为我们做好了。但是想在Scene视图中看到UI，
            我们需要调用ScriptableRenderContext.EmitWorldGeometryForSceneView函数。
            但是要注意的是调用这个函数会导致UI在game视图中再渲染一遍，
            所以我们要根据CameraType.SceneView区分是否是在scene视图中，
            并且用#if UNITY_EDITOR来保证只在Editor模式下编译这段代码。
         */
#if UNITY_EDITOR
        if (camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
        }
#endif

        //得到裁剪的配置信息后，通弄过context.Cull函数可以得到最终的裁剪结果
        cull = context.Cull(ref cullingParameters);

        context.SetupCameraProperties(camera);


        
        CameraClearFlags clearflags = camera.clearFlags;
        bool isClearDepth = (clearflags & CameraClearFlags.Depth) != 0;
        bool isClearColor = (clearflags & CameraClearFlags.Color) != 0;
        buffer.BeginSample("Render Camera");
        buffer.ClearRenderTarget(isClearDepth, isClearColor, camera.backgroundColor);
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();

        //渲染设置
        SortingSettings sorting = new SortingSettings(camera);
        ShaderTagId shaderTagId = new ShaderTagId("SRPDefaultUnlit"); // 使用LightMode的类型

        //通过设置SortFlags.CommonOpaque，可以从前向后的顺序渲染不透明物体  //有问题，渲染顺序不是每次都对 z值为 1 3 5
        sorting.criteria = SortingCriteria.CommonOpaque;

        var drawSetting = new DrawingSettings(shaderTagId, sorting);
        drawSetting.enableDynamicBatching = _dynamicBatching;
        drawSetting.enableInstancing = _instancing;


        //渲染过滤设置，这里设置成所有的都渲染
        //这边是指定渲染的种类(对应shader中的Rendertype)和相关Layer的设置(-1表示全部layer)
        var filterSetting = new FilteringSettings(RenderQueueRange.all, -1);
        filterSetting.renderQueueRange = RenderQueueRange.opaque;
        ////相当于把渲染command放入context
        context.DrawRenderers(cull, ref drawSetting, ref filterSetting);



        //相当于把渲染command放入context
        context.DrawSkybox(camera);

        //为了渲染透明物体，我们要将渲染顺序改回从后到前
        sorting.criteria = SortingCriteria.CommonTransparent;
        drawSetting = new DrawingSettings(shaderTagId, sorting);
        filterSetting.renderQueueRange = RenderQueueRange.transparent;
        context.DrawRenderers(cull, ref drawSetting, ref filterSetting);

        DrawDefaultPipeline(context, camera);
        buffer.EndSample("Render Camera");
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
        context.Submit();
    }

    [Conditional("UNITY_EDITOR")]
    void DrawDefaultPipeline(ScriptableRenderContext context, Camera camera)
    {
        if (errorMaterial == null)
        {
            Shader errorShader = Shader.Find("Hidden/InternalErrorShader");
            errorMaterial = new Material(errorShader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
        }
        SortingSettings sorting = new SortingSettings(camera);
        ShaderTagId shaderTagId = new ShaderTagId("ForwardBase");
        var drawSetting = new DrawingSettings(shaderTagId, sorting);
        drawSetting.overrideMaterial = errorMaterial;
        drawSetting.overrideMaterialPassIndex = 0;
        drawSetting.SetShaderPassName(1, new ShaderTagId("PrepassBase"));
        drawSetting.SetShaderPassName(2, new ShaderTagId("Always"));
        drawSetting.SetShaderPassName(3, new ShaderTagId("Vertex"));
        drawSetting.SetShaderPassName(4, new ShaderTagId("VertexLMRGBM"));
        drawSetting.SetShaderPassName(5, new ShaderTagId("VertexLM"));
        var filterSetting = new FilteringSettings(RenderQueueRange.all, -1);
        context.DrawRenderers(cull, ref drawSetting, ref filterSetting);
    }
}
