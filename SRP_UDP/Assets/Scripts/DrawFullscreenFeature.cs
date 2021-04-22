using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public enum BufferType
{
    CameraColor,
    Custom
}
public class DrawFullscreenFeature : ScriptableRendererFeature
{

    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        public Material blitMaterial = null;
        public int blitMaterialPassIndex = -1;
        public BufferType sourceType = BufferType.CameraColor;
        public BufferType destinationType = BufferType.CameraColor;
        public string sourceTextureId = "_SourceTexture";
        public string destinationTextureId = "_DestinationTexture";
    }

    public Settings settings = new Settings();
    DrawFullscreenPass blitPass;

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.blitMaterial == null)
        {
            Debug.LogWarningFormat("Missing Blit Material. {0} blit pass will not execute. Check for missing reference in the assigned renderer.", GetType().Name);
            return;
        }
        //同步配置给pass
        blitPass.renderPassEvent = settings.renderPassEvent;
        blitPass.settings = settings;
        renderer.EnqueuePass(blitPass);
    }

    public override void Create()
    {
        blitPass = new DrawFullscreenPass(name);
    }
}


public class DrawFullscreenPass : ScriptableRenderPass
{
    public FilterMode filterMode { get; set; }
    public DrawFullscreenFeature.Settings settings;
    RenderTargetIdentifier source;
    RenderTargetIdentifier destination;
    int temporaryRTId = Shader.PropertyToID("_TempRT");  //创建一张临时RT需要的参数
    int sourceId;
    int destinationId;
    bool isSourceAndDestinationSameTarget;

    string m_ProfilerTag;

    public DrawFullscreenPass(string tag)
    {
        m_ProfilerTag = tag;
    }

    // UniversalRenderPipeline.RenderSingleCamera--》ScriptableRenderer.Excute -->ScriptableRenderer.InternalStartRendering-->每个pass的OnCameraSetup
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        RenderTextureDescriptor blitTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor; //RT描述文件
        blitTargetDescriptor.depthBufferBits = 0;
        if (settings.sourceType == settings.destinationType && (settings.sourceType == BufferType.CameraColor || settings.sourceTextureId == settings.destinationTextureId))
        {
            isSourceAndDestinationSameTarget = true;
        }

        //获取renderer对象
        var renderer = renderingData.cameraData.renderer;
        if (settings.sourceType == BufferType.CameraColor)
        {
            sourceId = -1;
            source = renderer.cameraColorTarget; //_CameraColorTexture
        }
        else
        {
            sourceId = Shader.PropertyToID(settings.sourceTextureId); //
            cmd.GetTemporaryRT(sourceId, blitTargetDescriptor, filterMode); //创建一张临时RT
            source = new RenderTargetIdentifier(sourceId);
        }

        if (isSourceAndDestinationSameTarget)
        {
            destinationId = temporaryRTId;
            cmd.GetTemporaryRT(destinationId, blitTargetDescriptor, filterMode);
            destination = new RenderTargetIdentifier(destinationId);
        }
        else if (settings.destinationType == BufferType.CameraColor)
        {
            destinationId = -1;
            destination = renderer.cameraColorTarget;
        }
        else
        {
            destinationId = Shader.PropertyToID(settings.destinationTextureId);
            cmd.GetTemporaryRT(destinationId, blitTargetDescriptor, filterMode);
            destination = new RenderTargetIdentifier(destinationId);
        }


    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
        if (isSourceAndDestinationSameTarget)
        {
            //destination为一张临时RT
            Blit(cmd, source, destination, settings.blitMaterial, settings.blitMaterialPassIndex);
            Blit(cmd, destination, source); //RT不做任何处理复制过去
        }
        else
        {
            Blit(cmd, source, destination, settings.blitMaterial, settings.blitMaterialPassIndex);
        }
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }


    // UniversalRenderPipeline.RenderSingleCamera--》ScriptableRenderer.Excute -->ScriptableRenderer.InternalFinishRendering-->每个pass的FrameCleanup
    public override void FrameCleanup(CommandBuffer cmd)
    {
        //销毁临时RT
        if (destinationId != -1)
            cmd.ReleaseTemporaryRT(destinationId);

        if (source == destination && sourceId != -1)
            cmd.ReleaseTemporaryRT(sourceId);
    }
}
