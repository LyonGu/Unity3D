using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/*
 调用顺序 TODO
    1 BlurRenderFeature Create

    2 BlurRenderFeature AddRenderPasses
    3 GrabPassImpl Configure
    4 GrabPassImpl Execute

    2 3 4 是每帧都会调用
*/

[System.Serializable]
public class BlurRenderFeatureSettings
{
    public RenderPassEvent m_renderPassEvent;
    public Vector2 m_BlurAmount;
}
public class BlurRenderFeature : ScriptableRendererFeature
{

    const string k_BlurShader = "Hidden/Blur";
    private Material m_BlurMaterial;

    private Vector2 currentBlurAmount;

    public BlurRenderFeatureSettings settings;

    private GrabPassImpl m_grabPass;


    //每帧都调用
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        //Debug.Log($"BlurRenderFeature AddRenderPasses=============={Time.frameCount}");
        if (!renderingData.cameraData.isSceneViewCamera) 
        {
            // 非Scene窗口camera
            m_grabPass.SetUp(renderer.cameraColorTarget);// 颜色RT _CameraColorTexture  不透明物体
            renderer.EnqueuePass(m_grabPass);
        }
    }

  
    public override void Create()
    {
        //Debug.Log($"BlurRenderFeature Create=============={Time.frameCount}");
        m_BlurMaterial = CoreUtils.CreateEngineMaterial(Shader.Find(k_BlurShader));
        currentBlurAmount = settings.m_BlurAmount;
        m_grabPass = new GrabPassImpl(m_BlurMaterial, currentBlurAmount);
        m_grabPass.renderPassEvent = settings.m_renderPassEvent;
    }

    void Update()
    {
        if (m_grabPass != null)
        {
            if (currentBlurAmount != settings.m_BlurAmount)
            {
                currentBlurAmount = settings.m_BlurAmount;
                m_grabPass.UpdateBlurAmount(currentBlurAmount);
            }
        }
    }
}

public class GrabPassImpl : ScriptableRenderPass
{
    private Material m_BlurMaterial;
    private Vector2 m_BlurAmount;

    private RenderTextureDescriptor m_OpaqueDesc;  //RenderTexture的描述类
    private RenderTargetIdentifier m_CamerColorTexture;  
    public GrabPassImpl(Material blurMateral, Vector2 blurAmount)
    {
        base.profilingSampler = new ProfilingSampler(nameof(GrabPassImpl));
        m_BlurMaterial = blurMateral;
        m_BlurAmount = blurAmount;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        //Debug.Log($"GrabPassImpl OnCameraSetup=============={Time.frameCount}");
        base.OnCameraSetup(cmd, ref renderingData);
    }
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        //Debug.Log($"GrabPassImpl Execute=============={Time.frameCount}");
        CommandBuffer cmd = CommandBufferPool.Get();
        using (new ProfilingScope(cmd, profilingSampler))
        {
            //降低分辨率
            m_OpaqueDesc.width /= 4;
            m_OpaqueDesc.height /= 4;

            int blurredID = Shader.PropertyToID("_BlurRT1");
            int blurredID2 = Shader.PropertyToID("_BlurRT2");

            //获得两张RT  using过后会自动调用 cmd.ReleaseTemporaryRT?
            cmd.GetTemporaryRT(blurredID, m_OpaqueDesc, FilterMode.Bilinear);
            cmd.GetTemporaryRT(blurredID2, m_OpaqueDesc, FilterMode.Bilinear);

            //颜色RT Blit到临时RT中
            cmd.Blit(m_CamerColorTexture, blurredID);
            //横向纵向做Blur模糊
            cmd.SetGlobalVector("offsets", new Vector4(m_BlurAmount.x / Screen.width, 0, 0, 0));
            cmd.Blit(blurredID, blurredID2, m_BlurMaterial); //未指明pass，默认用第一个
            cmd.SetGlobalVector("offsets", new Vector4(0, m_BlurAmount.y / Screen.height, 0, 0));
            cmd.Blit(blurredID2, blurredID, m_BlurMaterial);
            cmd.SetGlobalVector("offsets", new Vector4(m_BlurAmount.x * 2 / Screen.width, 0, 0, 0));
            cmd.Blit(blurredID, blurredID2, m_BlurMaterial);
            cmd.SetGlobalVector("offsets", new Vector4(0, m_BlurAmount.y * 2 / Screen.height, 0, 0));
            cmd.Blit(blurredID2, blurredID, m_BlurMaterial);
            //最后在把临时RT Blit回颜色RT
            cmd.Blit(blurredID, m_CamerColorTexture);

        }
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }


    //每帧都调用
    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        //Debug.Log($"GrabPassImpl Configure=============={Time.frameCount}");
        cameraTextureDescriptor.msaaSamples = 1;
        m_OpaqueDesc = cameraTextureDescriptor;
    }

    public void SetUp(RenderTargetIdentifier destination)
    {
        m_CamerColorTexture = destination;
    }

    public void UpdateBlurAmount(Vector2 newBlurAmount)
    {
        m_BlurAmount = newBlurAmount;
    }

    public override void OnFinishCameraStackRendering(CommandBuffer cmd)
    {
        //Debug.Log($"GrabPassImpl OnFinishCameraStackRendering=============={Time.frameCount}");
        base.OnFinishCameraStackRendering(cmd);
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {
        //Debug.Log($"GrabPassImpl FrameCleanup=============={Time.frameCount}");
        base.FrameCleanup(cmd);
    }

}
