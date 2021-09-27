namespace UnityEngine.Rendering.Universal.Internal
{
    /// <summary>
    /// Copy the given color target to the current camera target
    ///
    /// You can use this pass to copy the result of rendering to
    /// the camera target. The pass takes the screen viewport into
    /// consideration.
    /// </summary>
    public class FinalBlitPass : ScriptableRenderPass
    {
        RenderTargetHandle m_Source;
        Material m_BlitMaterial;

        public FinalBlitPass(RenderPassEvent evt, Material blitMaterial)
        {
            base.profilingSampler = new ProfilingSampler(nameof(FinalBlitPass));

            m_BlitMaterial = blitMaterial;
            renderPassEvent = evt;
        }

        /// <summary>
        /// Configure the pass
        /// </summary>
        /// <param name="baseDescriptor"></param>
        /// <param name="colorHandle"></param>
        public void Setup(RenderTextureDescriptor baseDescriptor, RenderTargetHandle colorHandle)
        {
            //    当前相机开启了后效，colorHandle为 Render的m_AfterPostProcessColor， RT _AfterPostProcessTexture
            //    当前相机未开启了后效，colorHandle为  Render的m_ActiveCameraColorAttachment，可能是RT _CameraColorTexture，也可能是默认帧缓冲
            m_Source = colorHandle;
        }

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (m_BlitMaterial == null)
            {
                Debug.LogErrorFormat("Missing {0}. {1} render pass will not execute. Check for missing reference in the renderer resources.", m_BlitMaterial, GetType().Name);
                return;
            }

            // Note: We need to get the cameraData.targetTexture as this will get the targetTexture of the camera stack.
            // Overlay cameras need to output to the target described in the base camera while doing camera stack.
            ref CameraData cameraData = ref renderingData.cameraData;
            //cameraTarget为targetTexture，或者 默认帧缓冲
            RenderTargetIdentifier cameraTarget = (cameraData.targetTexture != null) ? new RenderTargetIdentifier(cameraData.targetTexture) : BuiltinRenderTextureType.CameraTarget;

            bool isSceneViewCamera = cameraData.isSceneViewCamera;
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.FinalBlit)))
            {


                /*
                 FinalBlit一方面是把贴图拷贝到FrameBuffer中，另外还需要做SRGB转换功能
                 由于在PBR中我们使用的多是线性空间，线性空间的颜色不尽兴伽玛校正是不能进行显示的
                 现在有些硬件（高端手机都支持，低端手机不支持）都支持SRGB转换，这样就可以直接将线性空间的颜色直接给FrameBuffer
                 */

                /*
                   为了兼容所有手机FinalBlit时，需要判断硬件是否支持SRGB转换，这段代码的含义就是当硬件不支持SRGB
                   转换时，在Shader中开启ShaderKeywordStrings.LinearToSRGBConversion这个宏，shader使用的是Blit.shader,
                   会在拷贝的同时进行一次伽马矫正
                 */
                CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.LinearToSRGBConversion,
                    cameraData.requireSrgbConversion);
                
                //shader的全局属性 _SourceTex
                cmd.SetGlobalTexture(ShaderPropertyId.sourceTex, m_Source.Identifier());

#if ENABLE_VR && ENABLE_XR_MODULE
                if (cameraData.xr.enabled)
                {
                    int depthSlice = cameraData.xr.singlePassEnabled ? -1 : cameraData.xr.GetTextureArraySlice();
                    cameraTarget =
                        new RenderTargetIdentifier(cameraData.xr.renderTarget, 0, CubemapFace.Unknown, depthSlice);

                    CoreUtils.SetRenderTarget(
                        cmd,
                        cameraTarget,
                        RenderBufferLoadAction.Load,
                        RenderBufferStoreAction.Store,
                        ClearFlag.None,
                        Color.black);

                    cmd.SetViewport(cameraData.pixelRect);

                    // We y-flip if
                    // 1) we are bliting from render texture to back buffer(UV starts at bottom) and
                    // 2) renderTexture starts UV at top
                    bool yflip = !cameraData.xr.renderTargetIsRenderTexture && SystemInfo.graphicsUVStartsAtTop;
                    Vector4 scaleBias = yflip ? new Vector4(1, -1, 0, 1) : new Vector4(1, 1, 0, 0);
                    cmd.SetGlobalVector(ShaderPropertyId.scaleBias, scaleBias);

                    cmd.DrawProcedural(Matrix4x4.identity, m_BlitMaterial, 0, MeshTopology.Quads, 4);
                }
                else
#endif
                /*
                 RenderBufferLoadAction : 表示GUP渲染时对当前目标像素加载的操作, 当给目标纹理绘制的时候，目标纹理时有颜色的
                 RenderBufferStoreAction：表示GUP渲染结束后对当前目标像素保存的操作
                 */
                if (isSceneViewCamera || cameraData.isDefaultViewport)
                {
                    //场景相机或者全屏视口，设置帧缓冲为颜色和深度数据的渲染目标
                    //BuiltinRenderTextureType.CameraTarget 代表 Target texture of currently rendering camera
                    //如果camera的targetTexure没有设置，就是默认帧缓冲，设置了就是相机的TargetTexure
                    
                    // This set render target is necessary so we change the LOAD state to DontCare.
                    cmd.SetRenderTarget(BuiltinRenderTextureType.CameraTarget,
                        RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, // color
                        RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare); // depth
                    //执行对应的pass，从m_Source输出到cameraTarget
                    //Add a "blit into a render texture" command.
                    //cameraTarget 大部分情况是BuiltinRenderTextureType.CameraTarget
                    cmd.Blit(m_Source.Identifier(), cameraTarget, m_BlitMaterial);
                }
                else
                {
                    // TODO: Final blit pass should always blit to backbuffer. The first time we do we don't need to Load contents to tile.
                    // We need to keep in the pipeline of first render pass to each render target to properly set load/store actions.
                    // meanwhile we set to load so split screen case works.
                    //设置颜色缓冲区目标为cameraTarget
                    CoreUtils.SetRenderTarget(
                        cmd,
                        cameraTarget,
                        RenderBufferLoadAction.Load,
                        RenderBufferStoreAction.Store,
                        ClearFlag.None,
                        Color.black);

                    Camera camera = cameraData.camera;
                    cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
                    cmd.SetViewport(cameraData.pixelRect);
                    cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, m_BlitMaterial);
                    cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
                }
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
