using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

public class MyPipeline : RenderPipeline
{
    private CullingResults cull;
    private CommandBuffer buffer;
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        //把command传入了buffer 实际起效需要我们通过submit函数submit 这些commands
        //context.DrawSkybox(cameras[0]);
        //context.Submit();
        //base.Render(context, cameras);
        if (buffer == null)
        {
            buffer = new CommandBuffer();
        }
        foreach (var camera in cameras)
        {
            //Render(context, camera);
            Render1(context, camera);
        }
    }


    void Render(ScriptableRenderContext context, Camera camera)
    {

        context.SetupCameraProperties(camera);

        //创建一个commandBuffer
        var buffer = new CommandBuffer()
        {
            name = "CustomComBuffer"
        };

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


        buffer.SetViewMatrix(camera.worldToCameraMatrix);
        //ExecuteCommandBuffer函数将command传入context的内部，等待submit后执行  
        context.ExecuteCommandBuffer(buffer);

        //command buffer会消耗unity native层的内存资源，当我们不再需要它的时候要立刻释放掉。Release函数可以完成这个功能。
        buffer.Clear();

        //得到裁剪的配置信息
        ScriptableCullingParameters cullingParameters;
        camera.TryGetCullingParameters(out cullingParameters);
        if (cullingParameters == null) return;
        cullingParameters.isOrthographic = false;

        //得到裁剪的配置信息后，通弄过context.Cull函数可以得到最终的裁剪结果
        CullingResults cull = context.Cull(ref cullingParameters);

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
        context.SetupCameraProperties(camera);

        //创建一个commandBuffer
        var buffer = new CommandBuffer()
        {
            name = "CustomComBuffer"
        };

        
        CameraClearFlags clearflags = camera.clearFlags;
        bool isClearDepth = (clearflags & CameraClearFlags.Depth) != 0;
        bool isClearColor = (clearflags & CameraClearFlags.Color) != 0;
        buffer.ClearRenderTarget(isClearDepth, isClearColor, camera.backgroundColor);
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();

        //得到裁剪的配置信息
        ScriptableCullingParameters cullingParameters;
        camera.TryGetCullingParameters(out cullingParameters);
        if (cullingParameters == null) return;
        cullingParameters.isOrthographic = false;

        //得到裁剪的配置信息后，通弄过context.Cull函数可以得到最终的裁剪结果
        CullingResults cull = context.Cull(ref cullingParameters);

        //渲染设置
        SortingSettings sorting = new SortingSettings(camera)
        {
            criteria = SortingCriteria.OptimizeStateChanges
        };
        ShaderTagId shaderTagId = new ShaderTagId("SRPDefaultUnlit"); // 使用LightMode的类型
        var drawSetting = new DrawingSettings(shaderTagId, sorting);

        //渲染过滤设置，这里设置成所有的都渲染
        //这边是指定渲染的种类(对应shader中的Rendertype)和相关Layer的设置(-1表示全部layer)
        var filterSetting = new FilteringSettings(RenderQueueRange.all, -1);
    

        ////相当于把渲染command放入context
        context.DrawRenderers(cull, ref drawSetting, ref filterSetting);

        //shaderTagId = new ShaderTagId("Always"); // 使用LightMode的类型
        //drawSetting = new DrawingSettings(shaderTagId, sorting);

        //context.DrawRenderers(cull, ref drawSetting, ref filterSetting);

        //相当于把渲染command放入context
        context.DrawSkybox(camera);
        context.Submit();
    }
}
