namespace UnityEngine.Rendering.Universal
{
    /// <summary>
    /// Let customizable actions inject commands to capture the camera output.
    ///
    /// You can use this pass to inject capture commands into a command buffer
    /// with the goal of having camera capture happening in external code.
    /// </summary>
    internal class CapturePass : ScriptableRenderPass
    {
        RenderTargetHandle m_CameraColorHandle;
        const string m_ProfilerTag = "Capture Pass";
        private static readonly ProfilingSampler m_ProfilingSampler = new ProfilingSampler(m_ProfilerTag);
        public CapturePass(RenderPassEvent evt)
        {
            base.profilingSampler = new ProfilingSampler(nameof(CapturePass));
            renderPassEvent = evt;
        }

        /// <summary>
        /// Configure the pass
        /// </summary>
        /// <param name="actions"></param>
        public void Setup(RenderTargetHandle colorHandle)
        {
            //当前相机开启了后效，colorHandle为 Render的m_AfterPostProcessColor， RT _AfterPostProcessTexture
            //当前相机未开启了后效，colorHandle为 Render的m_ActiveCameraColorAttachment，可能是RT _CameraColorTexture，也可能是默认帧缓冲
            m_CameraColorHandle = colorHandle;
        }

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmdBuf = CommandBufferPool.Get();
            using (new ProfilingScope(cmdBuf, m_ProfilingSampler))
            {
                
                var colorAttachmentIdentifier = m_CameraColorHandle.Identifier();
                var captureActions = renderingData.cameraData.captureActions;
                //传到定义好的回调里处理, 参数为颜色附件rt以及commanderbuffer对象
                for (captureActions.Reset(); captureActions.MoveNext();)
                    captureActions.Current(colorAttachmentIdentifier, cmdBuf);
            }
            
            //回调处理完再次把命令复制到context里
            context.ExecuteCommandBuffer(cmdBuf);
            CommandBufferPool.Release(cmdBuf);
        }
    }
}
