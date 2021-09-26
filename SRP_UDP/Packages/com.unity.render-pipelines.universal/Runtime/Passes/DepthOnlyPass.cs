using System;

namespace UnityEngine.Rendering.Universal.Internal
{
    /// <summary>
    /// Render all objects that have a 'DepthOnly' pass into the given depth buffer.
    ///
    /// You can use this pass to prime a depth buffer for subsequent rendering.
    /// Use it as a z-prepass, or use it to generate a depth buffer.
    /// </summary>
    public class DepthOnlyPass : ScriptableRenderPass
    {
        int kDepthBufferBits = 32;

        private RenderTargetHandle depthAttachmentHandle { get; set; }
        internal RenderTextureDescriptor descriptor { get; private set; }

        FilteringSettings m_FilteringSettings;
        ShaderTagId m_ShaderTagId = new ShaderTagId("DepthOnly");

        /// <summary>
        /// Create the DepthOnlyPass
        /// </summary>
        public DepthOnlyPass(RenderPassEvent evt, RenderQueueRange renderQueueRange, LayerMask layerMask)
        {
            base.profilingSampler = new ProfilingSampler(nameof(DepthOnlyPass));
            m_FilteringSettings = new FilteringSettings(renderQueueRange, layerMask);
            renderPassEvent = evt;
        }

        /// <summary>
        /// Configure the pass
        /// </summary>
        public void Setup(
            RenderTextureDescriptor baseDescriptor,
            RenderTargetHandle depthAttachmentHandle)
        {
            /*
             RenderTextureDescriptor 结构，里面记录的是对于RT的一些描述信息，depthBufferBits默认给的32bit。
             RenderTargetHandle结构主要记录了一个shader property id
             */
            this.depthAttachmentHandle = depthAttachmentHandle;
            
            //先设置管线的color为depth，因为后面OnCameraSetup是直接把深度RT设置为颜色缓冲目标
            baseDescriptor.colorFormat = RenderTextureFormat.Depth; 
            //32位 深度通道
            baseDescriptor.depthBufferBits = kDepthBufferBits;

            // Depth-Only pass don't use MSAA 禁用MSAA
            baseDescriptor.msaaSamples = 1;
            descriptor = baseDescriptor;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            //创建一个rt记录深度值，“_CameraDepthTexture”
            cmd.GetTemporaryRT(depthAttachmentHandle.id, descriptor, FilterMode.Point);
            
            //设置颜色缓冲渲染目标，绘制到_CameraDepthTexture RT上
            ConfigureTarget(new RenderTargetIdentifier(depthAttachmentHandle.Identifier(), 0, CubemapFace.Unknown, -1));
            
            //清除所有信息（颜色+深度+模板）
            ConfigureClear(ClearFlag.All, Color.black);
        }

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // NOTE: Do NOT mix ProfilingScope with named CommandBuffers i.e. CommandBufferPool.Get("name").
            // Currently there's an issue which results in mismatched markers.
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.DepthPrepass)))
            {
                //复制绘制命令到context里
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                
                //设置一些渲染相关的参数：排序方式，passId，
                //不透明物体排序
                var sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
                //只绘制具有"DepthOnly"pass的物体
                //创建一个DrawingSettings结构体 非CameraType.Preview相机会开启GPUInstancing
                var drawSettings = CreateDrawingSettings(m_ShaderTagId, ref renderingData, sortFlags);
                drawSettings.perObjectData = PerObjectData.None;
                
                //绘制几何体：其实这里也只是把命令添加到队列里，需要 context.Submit()后才会真正绘制
                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref m_FilteringSettings);

            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        /// <inheritdoc/>
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if (cmd == null)
                throw new ArgumentNullException("cmd");
            
            //渲染目标如果不是默认帧缓冲需要执行清理操作
            if (depthAttachmentHandle != RenderTargetHandle.CameraTarget)
            {
                cmd.ReleaseTemporaryRT(depthAttachmentHandle.id);
                depthAttachmentHandle = RenderTargetHandle.CameraTarget;
            }
        }
    }
}
