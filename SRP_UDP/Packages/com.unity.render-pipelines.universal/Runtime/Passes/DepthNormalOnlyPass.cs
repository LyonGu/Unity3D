using System;

namespace UnityEngine.Rendering.Universal.Internal
{
    public class DepthNormalOnlyPass : ScriptableRenderPass
    {
        internal RenderTextureDescriptor normalDescriptor { get; private set; }
        internal RenderTextureDescriptor depthDescriptor { get; private set; }

        private RenderTargetHandle depthHandle { get; set; }
        private RenderTargetHandle normalHandle { get; set; }
        private ShaderTagId m_ShaderTagId = new ShaderTagId("DepthNormals");
        private FilteringSettings m_FilteringSettings;

        // Constants
        private const int k_DepthBufferBits = 32;

        /// <summary>
        /// Create the DepthNormalOnlyPass
        /// </summary>
        public DepthNormalOnlyPass(RenderPassEvent evt, RenderQueueRange renderQueueRange, LayerMask layerMask)
        {
            base.profilingSampler = new ProfilingSampler(nameof(DepthNormalOnlyPass));
            m_FilteringSettings = new FilteringSettings(renderQueueRange, layerMask);
            renderPassEvent = evt;
        }

        /// <summary>
        /// Configure the pass
        /// </summary>
        public void Setup(RenderTextureDescriptor baseDescriptor, RenderTargetHandle depthHandle, RenderTargetHandle normalHandle)
        {
            //设置深度信息渲染目标RT _CameraDepthTexture 
            this.depthHandle = depthHandle;
            baseDescriptor.colorFormat = RenderTextureFormat.Depth;
            baseDescriptor.depthBufferBits = k_DepthBufferBits;
            baseDescriptor.msaaSamples = 1;// 禁用 MSAA
            depthDescriptor = baseDescriptor; //结构体 每次都是拷贝
            
            //设置法线信息渲染目标RT _CameraNormalsTexture
            this.normalHandle = normalHandle;
            baseDescriptor.colorFormat = RenderTextureFormat.RGHalf;
            baseDescriptor.depthBufferBits = 0;
            baseDescriptor.msaaSamples = 1;
            normalDescriptor = baseDescriptor;
        }

        /// <inheritdoc/>
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            //创建 _CameraDepthTexture RT 和  _CameraNormalsTexture RT
            cmd.GetTemporaryRT(normalHandle.id, normalDescriptor, FilterMode.Point);
            cmd.GetTemporaryRT(depthHandle.id, depthDescriptor, FilterMode.Point);
            //配置渲染目标
            //颜色缓冲区==》存储法线的RT
            //深度缓冲区==》存储深度的RT
            ConfigureTarget(
                new RenderTargetIdentifier(normalHandle.Identifier(), 0, CubemapFace.Unknown, -1),
                new RenderTargetIdentifier(depthHandle.Identifier(), 0, CubemapFace.Unknown, -1)
                );
            ConfigureClear(ClearFlag.All, Color.black);
        }

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // NOTE: Do NOT mix ProfilingScope with named CommandBuffers i.e. CommandBufferPool.Get("name").
            // Currently there's an issue which results in mismatched markers.
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.DepthNormalPrepass)))
            {
                //复制绘制命令到context里，因为OnCameraSetup里创建了RT，要把命令复制给context
                //后面context.submit时才会把命令提交给GPU
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                
                //设置一些渲染相关的参数：排序方式，passId，
                //不透明物体排序
                var sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
                
                //只绘制具有"DepthNormals"pass的物体
                //创建一个DrawingSettings结构体 非CameraType.Preview相机会开启GPUInstancing
                var drawSettings = CreateDrawingSettings(m_ShaderTagId, ref renderingData, sortFlags);
                drawSettings.perObjectData = PerObjectData.None;

                ref CameraData cameraData = ref renderingData.cameraData;
                Camera camera = cameraData.camera;
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
            {
                throw new ArgumentNullException("cmd");
            }
            
            //渲染目标如果不是默认帧缓冲需要执行清理操作
            if (depthHandle != RenderTargetHandle.CameraTarget)
            {
                //释放深度图RT和法线图RT
                cmd.ReleaseTemporaryRT(normalHandle.id);
                cmd.ReleaseTemporaryRT(depthHandle.id);
                
                //重置渲染目标为帧缓冲
                normalHandle = RenderTargetHandle.CameraTarget;
                depthHandle = RenderTargetHandle.CameraTarget;
            }
        }
    }
}
