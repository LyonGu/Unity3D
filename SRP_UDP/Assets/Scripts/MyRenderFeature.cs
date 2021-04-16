using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class Settings
{
    public Mesh mesh;
    public Material material;
    public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;
}
public class MyRenderFeature : ScriptableRendererFeature
{

    public Settings _settings = new Settings();
    private MyRendererPass _rendererPass;

   
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_rendererPass);
    }

    public override void Create()
    {
        _rendererPass = new MyRendererPass(_settings);
    }

}



public class MyRendererPass : ScriptableRenderPass
{

    private Settings m_Setting;
    public MyRendererPass(Settings settings)
    {
        base.profilingSampler = new ProfilingSampler(nameof(MyRendererPass));
        m_Setting = settings;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get();

        //添加ProfilingScope 在FrameDeBugger中方便查看
        using (new ProfilingScope(cmd, profilingSampler))
        {
            //cmd.draw ... 调用绘制接口
            cmd.DrawMesh(m_Setting.mesh, Matrix4x4.identity, m_Setting.material);

            //cmd.DrawMeshInstanced
            //cmd.DrawMeshInstancedIndirect
            //cmd.DrawMeshInstancedProcedural
            //cmd.DrawOcclusionMesh
            //cmd.DrawProceduralIndirect
            //cmd.DrawRenderer
        }

        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }
}
