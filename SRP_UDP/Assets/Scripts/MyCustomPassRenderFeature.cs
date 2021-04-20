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
public class MyCustomFeatureSettings
{
    public RenderPassEvent m_renderPassEvent;
}
public class MyCustomPassRenderFeature : ScriptableRendererFeature
{

    const string k_BlurShader = "Unlit/TestOnlyRenderCustomPass";
    private Material m_BlurMaterial;

    public MyCustomFeatureSettings settings;

    private MyCustomPass m_grabPass;


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
        m_grabPass = new MyCustomPass(m_BlurMaterial);
        m_grabPass.renderPassEvent = settings.m_renderPassEvent;
    }

}

public class MyCustomPass : ScriptableRenderPass
{
    private Material m_BlurMaterial;

    private RenderTargetIdentifier m_CamerColorTexture;  
    public MyCustomPass(Material blurMateral)
    {
        base.profilingSampler = new ProfilingSampler(nameof(MyCustomPass));
        m_BlurMaterial = blurMateral;
    }
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        //Debug.Log($"GrabPassImpl Execute=============={Time.frameCount}");
        CommandBuffer cmd = CommandBufferPool.Get();
        using (new ProfilingScope(cmd, profilingSampler))
        {
            
            cmd.Blit(m_CamerColorTexture, this.colorAttachment, m_BlurMaterial);

            

        }
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }


    public void SetUp(RenderTargetIdentifier destination)
    {
        m_CamerColorTexture = destination;
    }

}
