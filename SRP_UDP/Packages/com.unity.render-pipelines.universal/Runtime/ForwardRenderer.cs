using UnityEngine.Rendering.Universal.Internal;
using System.Reflection;
using System.ComponentModel;

namespace UnityEngine.Rendering.Universal
{

    // 正向渲染和延迟渲染
    /// <summary>
    /// Rendering modes for Universal renderer.
    /// </summary>
    public enum RenderingMode
    {
        /// <summary>Render all objects and lighting in one pass, with a hard limit on the number of lights that can be applied on an object.</summary>
        Forward,
        /// <summary>Render all objects first in a g-buffer pass, then apply all lighting in a separate pass using deferred shading.</summary>
        Deferred
    };

    /// <summary>
    /// Default renderer for Universal RP.
    /// This renderer is supported on all Universal RP supported platforms.
    /// It uses a classic forward rendering strategy with per-object light culling.
    /// </summary>
    public sealed class ForwardRenderer : ScriptableRenderer
    {
        const int k_DepthStencilBufferBits = 32;

        private static class Profiling
        {
            private const string k_Name = nameof(ForwardRenderer);
            public static readonly ProfilingSampler createCameraRenderTarget = new ProfilingSampler($"{k_Name}.{nameof(CreateCameraRenderTarget)}");
        }

        // Rendering mode setup from UI.
        internal RenderingMode renderingMode { get { return RenderingMode.Forward;  } }
        // Actual rendering mode, which may be different (ex: wireframe rendering, harware not capable of deferred rendering).
        internal RenderingMode actualRenderingMode { get { return GL.wireframe || m_DeferredLights == null || !m_DeferredLights.IsRuntimeSupportedThisFrame()  ? RenderingMode.Forward : this.renderingMode; } }
        internal bool accurateGbufferNormals { get { return m_DeferredLights != null ? m_DeferredLights.AccurateGbufferNormals : false; } }
        
        /*一堆Pass*/
        
        ColorGradingLutPass m_ColorGradingLutPass;
        DepthOnlyPass m_DepthPrepass;  //渲染深度的pass  ==》 RT: _CameraDepthTexture
        DepthNormalOnlyPass m_DepthNormalPrepass; //渲染深度+法线的pass ==》 RT: _CameraDepthTexture 和 _CameraNormalsTexture
        MainLightShadowCasterPass m_MainLightShadowCasterPass; //主光生成shadowMap Pass
        AdditionalLightsShadowCasterPass m_AdditionalLightsShadowCasterPass; //副光生成shadowMap Pass
        GBufferPass m_GBufferPass; //延迟渲染专用：储存一系列光照数据
        CopyDepthPass m_GBufferCopyDepthPass; //延迟渲染专用：拷贝深度信息， 目标RT: _CameraDepthTexture
        TileDepthRangePass m_TileDepthRangePass;
        TileDepthRangePass m_TileDepthRangeExtraPass; // TODO use subpass API to hide this pass
        DeferredPass m_DeferredPass;
        DrawObjectsPass m_RenderOpaqueForwardOnlyPass;
        DrawObjectsPass m_RenderOpaqueForwardPass;  //不透明物体前向渲染
        DrawSkyboxPass m_DrawSkyboxPass;
        CopyDepthPass m_CopyDepthPass; //这个pass 会把深度信息从_CameraDepthAttachment Copy到 _CameraDepthTexture上
        CopyColorPass m_CopyColorPass; //这个pass 会把颜色信息从_CameraColorTexture Copy到 _CameraOpaqueTexture上，这个时候只有不透明物体信息
        TransparentSettingsPass m_TransparentSettingsPass;
        DrawObjectsPass m_RenderTransparentForwardPass;  //透明物体前向渲染
        InvokeOnRenderObjectCallbackPass m_OnRenderObjectCallbackPass;
        PostProcessPass m_PostProcessPass;  //后期pass
        PostProcessPass m_FinalPostProcessPass; //后期pass
        FinalBlitPass m_FinalBlitPass; //FinalBlitPass ==》 最后输出到屏幕上
        CapturePass m_CapturePass; //抓屏Pass
#if ENABLE_VR && ENABLE_XR_MODULE
        XROcclusionMeshPass m_XROcclusionMeshPass;
        CopyDepthPass m_XRCopyDepthPass;
#endif
#if UNITY_EDITOR
        SceneViewDepthCopyPass m_SceneViewDepthCopyPass;  //Scene窗口深度拷贝pass
#endif
        //RT handle 创建RT使用
        RenderTargetHandle m_ActiveCameraColorAttachment; //激活目标ColorTexture，渲染颜色目标
        RenderTargetHandle m_ActiveCameraDepthAttachment; //激活目标DepthTexture，渲染深度目标
        RenderTargetHandle m_CameraColorAttachment; // RT:_CameraColorTexture
        RenderTargetHandle m_CameraDepthAttachment; // RT:_CameraDepthAttachment
        RenderTargetHandle m_DepthTexture; //RT:_CameraDepthTexture   m_DepthPrepass和m_DepthNormalPrepass使用
        RenderTargetHandle m_NormalsTexture; //RT:_CameraNormalsTexture   m_DepthNormalPrepass使用
        RenderTargetHandle[] m_GBufferHandles;
        RenderTargetHandle m_OpaqueColor; //透明物体颜色RT
        RenderTargetHandle m_AfterPostProcessColor; //后效RT
        RenderTargetHandle m_ColorGradingLut; //颜色分级lut RT
        // For tiled-deferred shading.
        RenderTargetHandle m_DepthInfoTexture;
        RenderTargetHandle m_TileDepthInfoTexture;

        //前向渲染光照信息
        ForwardLights m_ForwardLights;

        //延迟渲染
        DeferredLights m_DeferredLights;
#pragma warning disable 414
        RenderingMode m_RenderingMode;
#pragma warning restore 414
        StencilState m_DefaultStencilState;

        Material m_BlitMaterial;
        Material m_CopyDepthMaterial;
        Material m_SamplingMaterial;
        Material m_ScreenspaceShadowsMaterial; //屏幕空间阴影
        Material m_TileDepthInfoMaterial;
        Material m_TileDeferredMaterial;
        Material m_StencilDeferredMaterial;

        public ForwardRenderer(ForwardRendererData data) : base(data)
        {
#if ENABLE_VR && ENABLE_XR_MODULE
            UniversalRenderPipeline.m_XRSystem.InitializeXRSystemData(data.xrSystemData);
#endif
            //创建一堆材质信息
            m_BlitMaterial = CoreUtils.CreateEngineMaterial(data.shaders.blitPS); //"Shaders/Utils/Blit.shader"  
            m_CopyDepthMaterial = CoreUtils.CreateEngineMaterial(data.shaders.copyDepthPS); //"Shaders/Utils/CopyDepth.shader"
            m_SamplingMaterial = CoreUtils.CreateEngineMaterial(data.shaders.samplingPS); //"Shaders/Utils/Sampling.shader" 降采样
            m_ScreenspaceShadowsMaterial = CoreUtils.CreateEngineMaterial(data.shaders.screenSpaceShadowPS); //"Shaders/Utils/ScreenSpaceShadows.shader")
            //m_TileDepthInfoMaterial = CoreUtils.CreateEngineMaterial(data.shaders.tileDepthInfoPS);
            //m_TileDeferredMaterial = CoreUtils.CreateEngineMaterial(data.shaders.tileDeferredPS);
            m_StencilDeferredMaterial = CoreUtils.CreateEngineMaterial(data.shaders.stencilDeferredPS);
            
            //是否重载模板测试信息
            StencilStateData stencilData = data.defaultStencilState;
            m_DefaultStencilState = StencilState.defaultValue;
            m_DefaultStencilState.enabled = stencilData.overrideStencilState;
            m_DefaultStencilState.SetCompareFunction(stencilData.stencilCompareFunction); //比较函数 UnityEngin.Rendering.CompareFunction
            m_DefaultStencilState.SetPassOperation(stencilData.passOperation); //模板测试通过处理 UnityEngin.Rendering.StencilOp
            m_DefaultStencilState.SetFailOperation(stencilData.failOperation); //模板测试未通过处理
            m_DefaultStencilState.SetZFailOperation(stencilData.zFailOperation); //模板测试通过 深度测试未通过处理

            //光照信息处理类
            m_ForwardLights = new ForwardLights();
            //m_DeferredLights.LightCulling = data.lightCulling;
            this.m_RenderingMode = RenderingMode.Forward; //默认前向渲染

            //定义一堆pass，使用事件排序
            // Note: Since all custom render passes inject first and we have stable sort,
            // we inject the builtin passes in the before events.
            //主光阴影ShadowCaster，附光阴影ShadowCaster
            m_MainLightShadowCasterPass = new MainLightShadowCasterPass(RenderPassEvent.BeforeRenderingShadows); //50
            m_AdditionalLightsShadowCasterPass = new AdditionalLightsShadowCasterPass(RenderPassEvent.BeforeRenderingShadows); //50
#if ENABLE_VR && ENABLE_XR_MODULE
            m_XROcclusionMeshPass = new XROcclusionMeshPass(RenderPassEvent.BeforeRenderingOpaques);
            // Schedule XR copydepth right after m_FinalBlitPass(AfterRendering + 1)
            m_XRCopyDepthPass = new CopyDepthPass(RenderPassEvent.AfterRendering + 2, m_CopyDepthMaterial);
#endif
            //pre深度pass  150
            m_DepthPrepass = new DepthOnlyPass(RenderPassEvent.BeforeRenderingPrepasses, RenderQueueRange.opaque, data.opaqueLayerMask);
            
            //深度+法线pass 150
            m_DepthNormalPrepass = new DepthNormalOnlyPass(RenderPassEvent.BeforeRenderingPrepasses, RenderQueueRange.opaque, data.opaqueLayerMask);
            //颜色分级Lut pass 150
            m_ColorGradingLutPass = new ColorGradingLutPass(RenderPassEvent.BeforeRenderingPrepasses, data.postProcessData);

            if (this.renderingMode == RenderingMode.Deferred)
            {
                //后续看吧 TODO
                m_DeferredLights = new DeferredLights(m_TileDepthInfoMaterial, m_TileDeferredMaterial, m_StencilDeferredMaterial);
                m_DeferredLights.AccurateGbufferNormals = data.accurateGbufferNormals;
                //m_DeferredLights.TiledDeferredShading = data.tiledDeferredShading;
                m_DeferredLights.TiledDeferredShading = false;
                UniversalRenderPipelineAsset urpAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;

                m_GBufferPass = new GBufferPass(RenderPassEvent.BeforeRenderingOpaques, RenderQueueRange.opaque, data.opaqueLayerMask, m_DefaultStencilState, stencilData.stencilReference, m_DeferredLights);
                // Forward-only pass only runs if deferred renderer is enabled.
                // It allows specific materials to be rendered in a forward-like pass.
                // We render both gbuffer pass and forward-only pass before the deferred lighting pass so we can minimize copies of depth buffer and
                // benefits from some depth rejection.
                // - If a material can be rendered either forward or deferred, then it should declare a UniversalForward and a UniversalGBuffer pass.
                // - If a material cannot be lit in deferred (unlit, bakedLit, special material such as hair, skin shader), then it should declare UniversalForwardOnly pass
                // - Legacy materials have unamed pass, which is implicitely renamed as SRPDefaultUnlit. In that case, they are considered forward-only too.
                // TO declare a material with unnamed pass and UniversalForward/UniversalForwardOnly pass is an ERROR, as the material will be rendered twice.
                StencilState forwardOnlyStencilState = DeferredLights.OverwriteStencil(m_DefaultStencilState, (int)StencilUsage.MaterialMask);
                ShaderTagId[] forwardOnlyShaderTagIds = new ShaderTagId[] {
                    new ShaderTagId("UniversalForwardOnly"),
                    new ShaderTagId("SRPDefaultUnlit"), // Legacy shaders (do not have a gbuffer pass) are considered forward-only for backward compatibility
                    new ShaderTagId("LightweightForward") // Legacy shaders (do not have a gbuffer pass) are considered forward-only for backward compatibility
                };
                int forwardOnlyStencilRef = stencilData.stencilReference | (int)StencilUsage.MaterialUnlit;
                m_RenderOpaqueForwardOnlyPass = new DrawObjectsPass("Render Opaques Forward Only", forwardOnlyShaderTagIds, true, RenderPassEvent.BeforeRenderingOpaques + 1, RenderQueueRange.opaque, data.opaqueLayerMask, forwardOnlyStencilState, forwardOnlyStencilRef);
                m_GBufferCopyDepthPass = new CopyDepthPass(RenderPassEvent.BeforeRenderingOpaques + 2, m_CopyDepthMaterial);
                m_TileDepthRangePass = new TileDepthRangePass(RenderPassEvent.BeforeRenderingOpaques + 3, m_DeferredLights, 0);
                m_TileDepthRangeExtraPass = new TileDepthRangePass(RenderPassEvent.BeforeRenderingOpaques + 4, m_DeferredLights, 1);
                m_DeferredPass = new DeferredPass(RenderPassEvent.BeforeRenderingOpaques + 5, m_DeferredLights);
            }


            //不透明物体pass 250
            // Always create this pass even in deferred because we use it for wireframe rendering in the Editor or offscreen depth texture rendering.
            m_RenderOpaqueForwardPass = new DrawObjectsPass(URPProfileId.DrawOpaqueObjects, true,
                RenderPassEvent.BeforeRenderingOpaques, RenderQueueRange.opaque, data.opaqueLayerMask,
                m_DefaultStencilState, stencilData.stencilReference);
            
            //CopyDepthPass 400
            m_CopyDepthPass = new CopyDepthPass(RenderPassEvent.AfterRenderingSkybox, m_CopyDepthMaterial);
            
            //天空盒绘制Pass 350
            m_DrawSkyboxPass = new DrawSkyboxPass(RenderPassEvent.BeforeRenderingSkybox);
            
            //CopyColorPass 400
            m_CopyColorPass = new CopyColorPass(RenderPassEvent.AfterRenderingSkybox, m_SamplingMaterial, m_BlitMaterial);
#if ADAPTIVE_PERFORMANCE_2_1_0_OR_NEWER
            if (!UniversalRenderPipeline.asset.useAdaptivePerformance || AdaptivePerformance.AdaptivePerformanceRenderSettings.SkipTransparentObjects == false)
#endif
            {
                //透明物体设置渲染pass 450
                m_TransparentSettingsPass = new TransparentSettingsPass(RenderPassEvent.BeforeRenderingTransparents, data.shadowTransparentReceive);
                
                //透明物体渲染pass 450
                m_RenderTransparentForwardPass = new DrawObjectsPass(URPProfileId.DrawTransparentObjects, false, RenderPassEvent.BeforeRenderingTransparents, RenderQueueRange.transparent, data.transparentLayerMask, m_DefaultStencilState, stencilData.stencilReference);
            }
            
            //无效物体渲染Pass，callback  550
            m_OnRenderObjectCallbackPass = new InvokeOnRenderObjectCallbackPass(RenderPassEvent.BeforeRenderingPostProcessing);
            
            //后效pass 550
            m_PostProcessPass = new PostProcessPass(RenderPassEvent.BeforeRenderingPostProcessing, data.postProcessData, m_BlitMaterial);
            
            //最终的pass 1000+1
            m_FinalPostProcessPass = new PostProcessPass(RenderPassEvent.AfterRendering + 1, data.postProcessData, m_BlitMaterial);
            
            //截屏pass 1000
            m_CapturePass = new CapturePass(RenderPassEvent.AfterRendering);
            
            //FinalBlitPass 最后输出到屏幕的pass  1000+1
            m_FinalBlitPass = new FinalBlitPass(RenderPassEvent.AfterRendering + 1, m_BlitMaterial);

#if UNITY_EDITOR
            m_SceneViewDepthCopyPass = new SceneViewDepthCopyPass(RenderPassEvent.AfterRendering + 9, m_CopyDepthMaterial);
#endif

            //定义各种RT名称
            // RenderTexture format depends on camera and pipeline (HDR, non HDR, etc)
            // Samples (MSAA) depend on camera and pipeline
            m_CameraColorAttachment.Init("_CameraColorTexture");
            m_CameraDepthAttachment.Init("_CameraDepthAttachment");
            m_DepthTexture.Init("_CameraDepthTexture");
            m_NormalsTexture.Init("_CameraNormalsTexture");
            if (this.renderingMode == RenderingMode.Deferred)
            {
                m_GBufferHandles = new RenderTargetHandle[(int)DeferredLights.GBufferHandles.Count];
                m_GBufferHandles[(int)DeferredLights.GBufferHandles.DepthAsColor].Init("_GBufferDepthAsColor");
                m_GBufferHandles[(int)DeferredLights.GBufferHandles.Albedo].Init("_GBuffer0");
                m_GBufferHandles[(int)DeferredLights.GBufferHandles.SpecularMetallic].Init("_GBuffer1");
                m_GBufferHandles[(int)DeferredLights.GBufferHandles.NormalSmoothness].Init("_GBuffer2");
                m_GBufferHandles[(int)DeferredLights.GBufferHandles.Lighting] = new RenderTargetHandle();
                m_GBufferHandles[(int)DeferredLights.GBufferHandles.ShadowMask].Init("_GBuffer4");
            }
            m_OpaqueColor.Init("_CameraOpaqueTexture");
            m_AfterPostProcessColor.Init("_AfterPostProcessTexture");
            m_ColorGradingLut.Init("_InternalGradingLut");
            m_DepthInfoTexture.Init("_DepthInfoTexture");
            m_TileDepthInfoTexture.Init("_TileDepthInfoTexture");

            supportedRenderingFeatures = new RenderingFeatures()
            {
                cameraStacking = true,
            };

            if (this.renderingMode == RenderingMode.Deferred)
            {
                unsupportedGraphicsDeviceTypes = new GraphicsDeviceType[] {
                    GraphicsDeviceType.OpenGLCore,
                    GraphicsDeviceType.OpenGLES2,
                    GraphicsDeviceType.OpenGLES3
                };
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            // always dispose unmanaged resources
            m_PostProcessPass.Cleanup();
            m_FinalPostProcessPass.Cleanup();
            m_ColorGradingLutPass.Cleanup();

            CoreUtils.Destroy(m_BlitMaterial);
            CoreUtils.Destroy(m_CopyDepthMaterial);
            CoreUtils.Destroy(m_SamplingMaterial);
            CoreUtils.Destroy(m_ScreenspaceShadowsMaterial);
            CoreUtils.Destroy(m_TileDepthInfoMaterial);
            CoreUtils.Destroy(m_TileDeferredMaterial);
            CoreUtils.Destroy(m_StencilDeferredMaterial);
        }

        //根据配置确定是否加入对应的Pass参与渲染  UniversalRenderPipeline.RenderSingleCamera 会调用这个方法
        /// <inheritdoc />
        public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
        {
#if ADAPTIVE_PERFORMANCE_2_1_0_OR_NEWER
            bool needTransparencyPass = !UniversalRenderPipeline.asset.useAdaptivePerformance || !AdaptivePerformance.AdaptivePerformanceRenderSettings.SkipTransparentObjects;
#endif
            //获取相机对象
            Camera camera = renderingData.cameraData.camera;
            ref CameraData cameraData = ref renderingData.cameraData;
            
            //RT的渲染器描述对象 可以只用它来创建rt :屏幕大小，HDR，MSAA，sRGB。。。
            RenderTextureDescriptor cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;

            // Special path for depth only offscreen cameras. Only write opaques + transparents.
            //cameraData.targetTexture 其实就是camera的targetTexture属性，
            //cameraData.targetTexture不为空，且对应的格式是深度纹理
            bool isOffscreenDepthTexture = cameraData.targetTexture != null && cameraData.targetTexture.format == RenderTextureFormat.Depth;
            if (isOffscreenDepthTexture)
            {
                //这块逻辑走的情况比较少
                
                //设置相机的渲染目标（颜色缓冲区目标对象和深度缓冲区目标对象）都为默认帧缓冲，就是屏幕
                ConfigureCameraTarget(BuiltinRenderTextureType.CameraTarget, BuiltinRenderTextureType.CameraTarget);

                //加入 RenderFeature
                AddRenderPasses(ref renderingData);
                
                //把pass加入队列，
                //如果是渲染深度相机：RenderFeature 不透明物体pass 天空盒pass 透明物体pass
                EnqueuePass(m_RenderOpaqueForwardPass); //渲染不透明物体pass

                // TODO: Do we need to inject transparents and skybox when rendering depth only camera? They don't write to depth.
                EnqueuePass(m_DrawSkyboxPass); //渲染天空盒pass
#if ADAPTIVE_PERFORMANCE_2_1_0_OR_NEWER
                if (!needTransparencyPass)
                    return;
#endif
                EnqueuePass(m_RenderTransparentForwardPass); //渲染透明物体pass
                return;
            }

            if (m_DeferredLights != null)
                m_DeferredLights.ResolveMixedLightingMode(ref renderingData);

            // Assign the camera color target early in case it is needed during AddRenderPasses.
            bool isPreviewCamera = cameraData.isPreviewCamera; //preview相机
            
            //判断是否需要创建颜色纹理 ColorTexture，有自定义feature以及不是preveiew相机
            var createColorTexture = rendererFeatures.Count != 0 && !isPreviewCamera;
            if (createColorTexture)
            {
                //配置ColorTexture渲染目标，其实就是一张RT，名字叫“_CameraColorTexture”
                m_ActiveCameraColorAttachment = m_CameraColorAttachment;
                var activeColorRenderTargetId = m_ActiveCameraColorAttachment.Identifier();
#if ENABLE_VR && ENABLE_XR_MODULE
                if (cameraData.xr.enabled) activeColorRenderTargetId = new RenderTargetIdentifier(activeColorRenderTargetId, 0, CubemapFace.Unknown, -1);
#endif
                //设置相机的颜色缓冲区目标对象 为一张rt，名字叫“_CameraColorTexture”
                ConfigureCameraColorTarget(activeColorRenderTargetId);
            }

            // Add render passes and gather the input requirements
            isCameraColorTargetValid = true;

            //RenderFeature Pass 调用每个激活featrure的 AddRenderPasses
            AddRenderPasses(ref renderingData);
            isCameraColorTargetValid = false;
            
            //有什么用？？
            RenderPassInputSummary renderPassInputs = GetRenderPassInputs(ref renderingData); 

            // Should apply post-processing after rendering this camera?
            //正在渲染的相机上是否开启后效,只要相机上开了就行
            bool applyPostProcessing = cameraData.postProcessEnabled;

            // There's at least a camera in the camera stack that applies post-processing
            //只要有一个相机开启了后效就为true，包括 base Camera和 overlay Camera
            bool anyPostProcessing = renderingData.postProcessingEnabled;

            // TODO: We could cache and generate the LUT before rendering the stack
            bool generateColorGradingLUT = cameraData.postProcessEnabled; //是否生成LUT 表  后期开启了就会生成LUT表
            bool isSceneViewCamera = cameraData.isSceneViewCamera; //Scene窗口相机
            
            //再次判断是否需要深度图， cameraData.requiresDepthTexture
            /*
             * if(配置文件勾选了开启获取深度)
             * {
             *     return true;
             * }
             * else
             * {
             *    if(相机为场景相机)
             *     {
             *        return true;
             *     }
             *     else
             *     {
             *         if(相机未开启后效)
             *             return false;
             *         if(相机的抗锯齿模式 == AntialiasingMode.SubpixelMorphologicalAntiAliasing)
             *             return true;
             *         if(开启后效里有运动模糊和景深效果)
             *             return true;
             *         return false
             *     }
             * }
             */
            
            //判断是否需要深度图
            bool requiresDepthTexture = cameraData.requiresDepthTexture || renderPassInputs.requiresDepthTexture || this.actualRenderingMode == RenderingMode.Deferred;

            //主光源ShadowMap ShadowCasterPass.SetUp ==> 根据返回值确定是否开启屏幕空间阴影
            bool mainLightShadows = m_MainLightShadowCasterPass.Setup(ref renderingData);

            //副光源ShadowMap ==> 根据返回值确定是否开启屏幕空间阴影
            bool additionalLightShadows = m_AdditionalLightsShadowCasterPass.Setup(ref renderingData);
            
            //一般返回false
            bool transparentsNeedSettingsPass = m_TransparentSettingsPass.Setup(ref renderingData);

            // Depth prepass is generated in the following cases:
            // - If game or offscreen camera requires it we check if we can copy the depth from the rendering opaques pass and use that instead.
            // - Scene or preview cameras always require a depth texture. We do a depth pre-pass to simplify it and it shouldn't matter much for editor.
            // - Render passes require it
            
            //这命名真是醉了，其实就是表示是不是需要产生一张深度图RT，有一个为true最后结果就为true
            bool requiresDepthPrepass = requiresDepthTexture && !CanCopyDepth(ref renderingData.cameraData);
            requiresDepthPrepass |= isSceneViewCamera;
            requiresDepthPrepass |= isPreviewCamera;
            requiresDepthPrepass |= renderPassInputs.requiresDepthPrepass; //一般为false
            requiresDepthPrepass |= renderPassInputs.requiresNormalsTexture; //一般为false

            // The copying of depth should normally happen after rendering opaques.
            // But if we only require it for post processing or the scene camera then we do it after rendering transparent objects
            //需要深度贴图==》 直接在渲染完不透明物体后渲染
            //不需要深度贴图 + (当前相机开启了后效或者当前相机是场景相机) ==》直接在渲染完透明物体后渲染
            m_CopyDepthPass.renderPassEvent = (!requiresDepthTexture && (applyPostProcessing || isSceneViewCamera)) ? RenderPassEvent.AfterRenderingTransparents : RenderPassEvent.AfterRenderingOpaques;

            //再次判断是否需要ColorTexture（绘制了不透明物体和天空盒的RT）
            bool IntermediateColorTexture = RequiresIntermediateColorTexture(ref cameraData); //是否产生中间RT，一般返回true
            createColorTexture |= IntermediateColorTexture; 
            createColorTexture |= renderPassInputs.requiresColorTexture;
            createColorTexture &= !isPreviewCamera;

            // If camera requires depth and there's no depth pre-pass we create a depth texture that can be read later by effect requiring it.
            // When deferred renderer is enabled, we must always create a depth texture and CANNOT use BuiltinRenderTextureType.CameraTarget. This is to get
            // around a bug where during gbuffer pass (MRT pass), the camera depth attachment is correctly bound, but during
            // deferred pass ("camera color" + "camera depth"), the implicit depth surface of "camera color" is used instead of "camera depth",
            // because BuiltinRenderTextureType.CameraTarget for depth means there is no explicit depth attachment...
            
            //判断是否产生一张深度RT
            bool createDepthTexture = cameraData.requiresDepthTexture && !requiresDepthPrepass;
            
            //cameraData.resolveFinalTarget 当前相机是否需要最后绘制到屏幕上，如果当前相机的camerastack里有激活的相机就不需要绘制到屏幕
            createDepthTexture |= (cameraData.renderType == CameraRenderType.Base && !cameraData.resolveFinalTarget);
            // Deferred renderer always need to access depth buffer.
            createDepthTexture |= this.actualRenderingMode == RenderingMode.Deferred;
#if ENABLE_VR && ENABLE_XR_MODULE
            if (cameraData.xr.enabled)
            {
                // URP can't handle msaa/size mismatch between depth RT and color RT(for now we create intermediate textures to ensure they match)
                createDepthTexture |= createColorTexture;
                createColorTexture = createDepthTexture;
            }
#endif

#if UNITY_ANDROID || UNITY_WEBGL
            if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Vulkan)
            {
                // GLES can not use render texture's depth buffer with the color buffer of the backbuffer
                // in such case we create a color texture for it too.
                createColorTexture |= createDepthTexture;
            }
#endif

            // Configure all settings require to start a new camera stack (base camera only)
            if (cameraData.renderType == CameraRenderType.Base)
            {
                RenderTargetHandle cameraTargetHandle = RenderTargetHandle.GetCameraTarget(cameraData.xr);//FrameBuffer 帧缓冲

                m_ActiveCameraColorAttachment = (createColorTexture) ? m_CameraColorAttachment : cameraTargetHandle;
                m_ActiveCameraDepthAttachment = (createDepthTexture) ? m_CameraDepthAttachment : cameraTargetHandle;

                //是否产生中间RT
                bool intermediateRenderTexture = createColorTexture || createDepthTexture;

                // Doesn't create texture for Overlay cameras as they are already overlaying on top of created textures.
                if (intermediateRenderTexture)
                {
                    //生成一张_CameraColorTexture 临时RT
                    //生成一张_CameraDepthAttachment 临时RT
                    //里面会有commandbuffer提交给context
                    CreateCameraRenderTarget(context, ref cameraTargetDescriptor, createColorTexture, createDepthTexture);
                }
                    
            }
            else
            {
                //URP 原本是将overlay的摄像机颜色和深度写入两张RT中，最后FInalBlit到FrameBuffer中
                //if (Display.main.requiresSrgbBlitToBackbuffer)
                //{
                //    m_ActiveCameraColorAttachment = m_CameraColorAttachment;  //_CameraColorTexture
                //    m_ActiveCameraDepthAttachment = m_CameraDepthAttachment;  //_CameraDepthTexture
                //}
                //else
                //{
                //    //如果硬件支持SRGB转换，直接写入FrameBuffer
                //    m_ActiveCameraColorAttachment = RenderTargetHandle.CameraTarget;
                //    m_ActiveCameraDepthAttachment = RenderTargetHandle.CameraTarget;
                //}
                
                //源码就是这个
                
                //overlay相机直接设置激活rt目标
                m_ActiveCameraColorAttachment = m_CameraColorAttachment;  //_CameraColorTexture
                m_ActiveCameraDepthAttachment = m_CameraDepthAttachment;  //_CameraDepthTexture


            }

            // Assign camera targets (color and depth)
            {
                var activeColorRenderTargetId = m_ActiveCameraColorAttachment.Identifier();
                var activeDepthRenderTargetId = m_ActiveCameraDepthAttachment.Identifier();

#if ENABLE_VR && ENABLE_XR_MODULE
                if (cameraData.xr.enabled)
                {
                    activeColorRenderTargetId = new RenderTargetIdentifier(activeColorRenderTargetId, 0, CubemapFace.Unknown, -1);
                    activeDepthRenderTargetId = new RenderTargetIdentifier(activeDepthRenderTargetId, 0, CubemapFace.Unknown, -1);
                }
#endif
                //配置摄像机的渲染目标 m_CameraColorTarget 和 m_CameraDepthTarget
                //默认情况下，是帧缓冲区，但也可能是渲染纹理
                ConfigureCameraTarget(activeColorRenderTargetId, activeDepthRenderTargetId);
            }

            //后期处理完是否还有pass需要执行
            bool hasPassesAfterPostProcessing = activeRenderPassQueue.Find(x => x.renderPassEvent == RenderPassEvent.AfterRendering) != null;
            
            //根据逻辑判断是否把对应的pass加入队列，并且调用pass的SetUp方法
            //主光源ShadowMappass加入队列
            if (mainLightShadows)
                EnqueuePass(m_MainLightShadowCasterPass);

            //副光源ShadowMap pass加入队列
            if (additionalLightShadows)
                EnqueuePass(m_AdditionalLightsShadowCasterPass);

            if (requiresDepthPrepass)
            {
                if (renderPassInputs.requiresNormalsTexture)
                {
                    //需要法线图，会同时生成深度信息  m_DepthNormalPrepass加入对列
                    m_DepthNormalPrepass.Setup(cameraTargetDescriptor, m_DepthTexture, m_NormalsTexture);
                    EnqueuePass(m_DepthNormalPrepass);
                }
                else
                {
                    //只生成深度图  DepthPrePass加入对列 生成_CameraDepthTexture 
                    m_DepthPrepass.Setup(cameraTargetDescriptor, m_DepthTexture);
                    EnqueuePass(m_DepthPrepass);
                }
            }

            if (generateColorGradingLUT)
            {
                //LUT 查找表生成 后期开启了就会生成LUT表
                m_ColorGradingLutPass.Setup(m_ColorGradingLut);
                EnqueuePass(m_ColorGradingLutPass);
            }

#if ENABLE_VR && ENABLE_XR_MODULE
            if (cameraData.xr.hasValidOcclusionMesh)
                EnqueuePass(m_XROcclusionMeshPass);
#endif

            if (this.actualRenderingMode == RenderingMode.Deferred)
                EnqueueDeferred(ref renderingData, requiresDepthPrepass, mainLightShadows, additionalLightShadows);
            else
                EnqueuePass(m_RenderOpaqueForwardPass);// 不透明物体渲染队列

            Skybox cameraSkybox;
            cameraData.camera.TryGetComponent<Skybox>(out cameraSkybox);
            bool isOverlayCamera = cameraData.renderType == CameraRenderType.Overlay;
            if (camera.clearFlags == CameraClearFlags.Skybox && (RenderSettings.skybox != null || cameraSkybox?.material != null) && !isOverlayCamera)
                EnqueuePass(m_DrawSkyboxPass); //天空盒渲染
            
            //复制深度纹理
            // If a depth texture was created we necessarily need to copy it, otherwise we could have render it to a renderbuffer.
            // If deferred rendering path was selected, it has already made a copy.
            bool requiresDepthCopyPass = !requiresDepthPrepass
                                         && renderingData.cameraData.requiresDepthTexture
                                         && createDepthTexture
                                         && this.actualRenderingMode != RenderingMode.Deferred;
            if (requiresDepthCopyPass)
            {
                //CopyDepthPass  
                m_CopyDepthPass.Setup(m_ActiveCameraDepthAttachment, m_DepthTexture);
                EnqueuePass(m_CopyDepthPass);
            }

            // For Base Cameras: Set the depth texture to the far Z if we do not have a depth prepass or copy depth
            if (cameraData.renderType == CameraRenderType.Base && !requiresDepthPrepass && !requiresDepthCopyPass)
            {
                //如果不生成深度图就会往shader里赋值一个属性名为 _CameraDepthTexture，shader里可以直接使用
                Shader.SetGlobalTexture(m_DepthTexture.id, SystemInfo.usesReversedZBuffer ? Texture2D.blackTexture : Texture2D.whiteTexture);
            }

            if (renderingData.cameraData.requiresOpaqueTexture || renderPassInputs.requiresColorTexture)
            {
                //CopyColor Pass 绘制颜色RT  _CameraOpaqueTexture，只要不透明颜色数据
                // TODO: Downsampling method should be store in the renderer instead of in the asset.
                // We need to migrate this data to renderer. For now, we query the method in the active asset.
                Downsampling downsamplingMethod = UniversalRenderPipeline.asset.opaqueDownsampling;
                //Copy Color Pass ： 生成一张_CameraOpaqueColor的RT  downsamplingMethod降采样信息  _CameraOpaqueColor上只有不透明物体的信息
                //m_CopyColorPass这个pass的事件是AfterRenderingSkybox
                //m_RenderTransparentForwardPass 透明pass的事件是BeforeRenderingTransparents
                //m_CopyColorPass会优先 m_RenderTransparentForwardPass渲染，pass内部会排序，越小越先渲染
                m_CopyColorPass.Setup(m_ActiveCameraColorAttachment.Identifier(), m_OpaqueColor, downsamplingMethod);
                EnqueuePass(m_CopyColorPass);
            }
#if ADAPTIVE_PERFORMANCE_2_1_0_OR_NEWER
            if (needTransparencyPass)
#endif
            {
                if (transparentsNeedSettingsPass)
                {
                    //一般这里不走
                    EnqueuePass(m_TransparentSettingsPass);
                }

                //半透明渲染队列
                EnqueuePass(m_RenderTransparentForwardPass);
            }

            //ObjectPass 渲染事件回调Pass
            EnqueuePass(m_OnRenderObjectCallbackPass);
            
            //判断是否是最后一个渲染目标，当前相机是否要输出到屏幕 
            //当前相机的stack为空，或者 stack里有相机 但都没有激活 cameraData.resolveFinalTarget值为true
            bool lastCameraInTheStack = cameraData.resolveFinalTarget; 
            bool hasCaptureActions = renderingData.cameraData.captureActions != null && lastCameraInTheStack;

            //判断是否执行最后后效处理：当前相机开启后效&&最后一个渲染目标&&抗锯齿为FAXX
            bool isFAXX = renderingData.cameraData.antialiasing == AntialiasingMode.FastApproximateAntialiasing;
            bool applyFinalPostProcessing = anyPostProcessing && lastCameraInTheStack && isFAXX;

            //后处理后如果还有Pass就继续绘制，不直接绘制到屏幕上
            // When post-processing is enabled we can use the stack to resolve rendering to camera target (screen or RT).
            // However when there are render passes executing after post we avoid resolving to screen so rendering continues (before sRGBConvertion etc)
            
            //hasCaptureActions 一般为false
            //hasPassesAfterPostProcessing 一般为false
            //applyFinalPostProcessing: stack里最后一个激活相机 && isFAXX && 所有相机里有开启后效的相机
            bool resolvePostProcessingToCameraTarget = !hasCaptureActions && !hasPassesAfterPostProcessing && !applyFinalPostProcessing;
            
            //resolvePostProcessingToCameraTarget这个变量指给lastCameraInTheStack为true时才会使用，其实可以放到if里面
            if (lastCameraInTheStack)
            {
                //最后一个渲染目标: statck里相机没有激活 或者 Stack里最后一个激活相机 才会走这段逻辑
         
                
                // Post-processing will resolve to final target. No need for final blit pass.
                //applyPostProcessing为true 表示当前相机开启了后效
                if (applyPostProcessing)
                {
                    //m_AfterPostProcessColor ==> _AfterPostProcessTexture
                    var destination = resolvePostProcessingToCameraTarget ? RenderTargetHandle.CameraTarget : m_AfterPostProcessColor;

                    // if resolving to screen we need to be able to perform sRGBConvertion in post-processing if necessary
                    bool doSRGBConvertion = resolvePostProcessingToCameraTarget;
                    //处理的原始图其实就是m_CameraColorAttachment==> "_CameraColorTexture"
                    m_PostProcessPass.Setup(cameraTargetDescriptor, m_ActiveCameraColorAttachment, destination, m_ActiveCameraDepthAttachment, m_ColorGradingLut, applyFinalPostProcessing, doSRGBConvertion);
                    EnqueuePass(m_PostProcessPass); //后效pass入队列
                }


                // if we applied post-processing for this camera it means current active texture is m_AfterPostProcessColor
                //m_ActiveCameraColorAttachment 可以理解为相机的颜色缓冲区
                var sourceForFinalPass = (applyPostProcessing) ? m_AfterPostProcessColor : m_ActiveCameraColorAttachment;

                // Do FXAA or any other final post-processing effect that might need to run after AA.
                if (applyFinalPostProcessing)
                {
                    //最后一个相机也开启了后效，加入m_FinalPostProcessPass
                    m_FinalPostProcessPass.SetupFinalPass(sourceForFinalPass);
                    EnqueuePass(m_FinalPostProcessPass);
                }

                if (renderingData.cameraData.captureActions != null)
                {
                    //截屏pass
                    m_CapturePass.Setup(sourceForFinalPass);
                    EnqueuePass(m_CapturePass);
                }
                
                //判断是否要最后执行一次 final blit，满足条件之一即可
                // applyFinalPostProcessing: 最后一个相机也开启了后效
                // applyPostProcessing: 当前相机是否开启了后效，
                // hasPassesAfterPostProcessing: 一般为false
                // m_ActiveCameraColorAttachment对象为默认的帧缓冲
                
                // if post-processing then we already resolved to camera target while doing post.
                // Also only do final blit if camera is not rendering to RT.
                bool isEquare = m_ActiveCameraColorAttachment == RenderTargetHandle.GetCameraTarget(cameraData.xr);
                bool cameraTargetResolved =
                    // final PP always blit to camera target
                    applyFinalPostProcessing ||
                    // no final PP but we have PP stack. In that case it blit unless there are render pass after PP
                    (applyPostProcessing && !hasPassesAfterPostProcessing) ||
                    // offscreen camera rendering to a texture, we don't need a blit pass to resolve to screen 绘制到屏幕
                    isEquare;
          
                // We need final blit to resolve to screen
                if (!cameraTargetResolved)
                {
                    //如果硬件不支持SRGB转换，直接启动FinalBlit，就需要FinalBlitPass，shader里面有个宏开关打开后会进行SRGB转换
                    //if (Display.main.requiresSrgbBlitToBackbuffer)
                    {
                        //FinalBlitPass
                        m_FinalBlitPass.Setup(cameraTargetDescriptor, sourceForFinalPass);
                        EnqueuePass(m_FinalBlitPass);
                    }
                    
                }

#if ENABLE_VR && ENABLE_XR_MODULE
                bool depthTargetResolved =
                    // active depth is depth target, we don't need a blit pass to resolve
                    m_ActiveCameraDepthAttachment == RenderTargetHandle.GetCameraTarget(cameraData.xr);

                if (!depthTargetResolved && cameraData.xr.copyDepth)
                {
                    m_XRCopyDepthPass.Setup(m_ActiveCameraDepthAttachment, RenderTargetHandle.GetCameraTarget(cameraData.xr));
                    EnqueuePass(m_XRCopyDepthPass);
                }
#endif
            }

            // stay in RT so we resume rendering on stack after post-processing
            else if (applyPostProcessing)
            {
                //**** baseCamera会走这段逻辑
                //applyPostProcessing为true 表示当前相机开启了后效
                
                //if (Display.main.requiresSrgbBlitToBackbuffer)
                //{
                //    //如果硬件不支持SRGB转换，在 _AfterPostProcessColor 上画
                //    m_PostProcessPass.Setup(cameraTargetDescriptor, m_ActiveCameraColorAttachment, m_AfterPostProcessColor, m_ActiveCameraDepthAttachment, m_ColorGradingLut, false, false);
                //    EnqueuePass(m_PostProcessPass);
                //}
                //else
                //{
                //    //如果硬件支持SRGB转换，在 FrameBuffer 上画
                //    m_PostProcessPass.Setup(cameraTargetDescriptor, m_ActiveCameraColorAttachment, RenderTargetHandle.CameraTarget, m_ActiveCameraDepthAttachment, m_ColorGradingLut, false, false);
                //    EnqueuePass(m_PostProcessPass);
                //}

                m_PostProcessPass.Setup(cameraTargetDescriptor, m_ActiveCameraColorAttachment, m_AfterPostProcessColor, m_ActiveCameraDepthAttachment, m_ColorGradingLut, false, false);
                EnqueuePass(m_PostProcessPass); //后效的pass加入对列

            }

#if UNITY_EDITOR
            if (isSceneViewCamera)
            {
                // Scene view camera should always resolve target (not stacked)
                Assertions.Assert.IsTrue(lastCameraInTheStack, "Editor camera must resolve target upon finish rendering.");
                m_SceneViewDepthCopyPass.Setup(m_DepthTexture);
                EnqueuePass(m_SceneViewDepthCopyPass);
            }
#endif
        }

        /// <inheritdoc />
        public override void SetupLights(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            m_ForwardLights.Setup(context, ref renderingData);

            // Perform per-tile light culling on CPU  延迟渲染TODO
            if (this.actualRenderingMode == RenderingMode.Deferred)
                m_DeferredLights.SetupLights(context, ref renderingData);
        }

        // 摄像机裁剪
        /// <inheritdoc />
        public override void SetupCullingParameters(ref ScriptableCullingParameters cullingParameters,
            ref CameraData cameraData)
        {
            // TODO: PerObjectCulling also affect reflection probes. Enabling it for now.
            // if (asset.additionalLightsRenderingMode == LightRenderingMode.Disabled ||
            //     asset.maxAdditionalLightsCount == 0)
            // {
            //     cullingParameters.cullingOptions |= CullingOptions.DisablePerObjectCulling;
            // }

            // We disable shadow casters if both shadow casting modes are turned off
            // or the shadow distance has been turned down to zero
            bool isShadowCastingDisabled = !UniversalRenderPipeline.asset.supportsMainLightShadows && !UniversalRenderPipeline.asset.supportsAdditionalLightShadows;
            bool isShadowDistanceZero = Mathf.Approximately(cameraData.maxShadowDistance, 0.0f);
            if (isShadowCastingDisabled || isShadowDistanceZero)
            {
                cullingParameters.cullingOptions &= ~CullingOptions.ShadowCasters;
            }

            if (this.actualRenderingMode == RenderingMode.Deferred)
                cullingParameters.maximumVisibleLights = 0xFFFF;
            else
            {
                // We set the number of maximum visible lights allowed and we add one for the mainlight...
                cullingParameters.maximumVisibleLights = UniversalRenderPipeline.maxVisibleAdditionalLights + 1;
            }
            cullingParameters.shadowDistance = cameraData.maxShadowDistance;
        }

        //清理操作
        /// <inheritdoc />
        public override void FinishRendering(CommandBuffer cmd)
        {
            if (m_ActiveCameraColorAttachment != RenderTargetHandle.CameraTarget)
            {
                cmd.ReleaseTemporaryRT(m_ActiveCameraColorAttachment.id);
                m_ActiveCameraColorAttachment = RenderTargetHandle.CameraTarget;
            }

            if (m_ActiveCameraDepthAttachment != RenderTargetHandle.CameraTarget)
            {
                cmd.ReleaseTemporaryRT(m_ActiveCameraDepthAttachment.id);
                m_ActiveCameraDepthAttachment = RenderTargetHandle.CameraTarget;
            }
        }

        void EnqueueDeferred(ref RenderingData renderingData, bool hasDepthPrepass, bool applyMainShadow, bool applyAdditionalShadow)
        {
            // the last slice is the lighting buffer created in DeferredRenderer.cs
            m_GBufferHandles[(int)DeferredLights.GBufferHandles.Lighting] = m_ActiveCameraColorAttachment;

            m_DeferredLights.Setup(
                ref renderingData,
                applyAdditionalShadow ? m_AdditionalLightsShadowCasterPass : null,
                hasDepthPrepass,
                renderingData.cameraData.renderType == CameraRenderType.Overlay,
                m_DepthTexture,
                m_DepthInfoTexture,
                m_TileDepthInfoTexture,
                m_ActiveCameraDepthAttachment, m_GBufferHandles
            );

            EnqueuePass(m_GBufferPass);

            EnqueuePass(m_RenderOpaqueForwardOnlyPass);

            //Must copy depth for deferred shading: TODO wait for API fix to bind depth texture as read-only resource.
            if (!hasDepthPrepass)
            {
                m_GBufferCopyDepthPass.Setup(m_CameraDepthAttachment, m_DepthTexture);
                EnqueuePass(m_GBufferCopyDepthPass);
            }

            // Note: DeferredRender.Setup is called by UniversalRenderPipeline.RenderSingleCamera (overrides ScriptableRenderer.Setup).
            // At this point, we do not know if m_DeferredLights.m_Tilers[x].m_Tiles actually contain any indices of lights intersecting tiles (If there are no lights intersecting tiles, we could skip several following passes) : this information is computed in DeferredRender.SetupLights, which is called later by UniversalRenderPipeline.RenderSingleCamera (via ScriptableRenderer.Execute).
            // However HasTileLights uses m_HasTileVisLights which is calculated by CheckHasTileLights from all visibleLights. visibleLights is the list of lights that have passed camera culling, so we know they are in front of the camera. So we can assume m_DeferredLights.m_Tilers[x].m_Tiles will not be empty in that case.
            // m_DeferredLights.m_Tilers[x].m_Tiles could be empty if we implemented an algorithm accessing scene depth information on the CPU side, but this (access depth from CPU) will probably not happen.
            if (m_DeferredLights.HasTileLights())
            {
                // Compute for each tile a 32bits bitmask in which a raised bit means "this 1/32th depth slice contains geometry that could intersect with lights".
                // Per-tile bitmasks are obtained by merging together the per-pixel bitmasks computed for each individual pixel of the tile.
                EnqueuePass(m_TileDepthRangePass);

                // On some platform, splitting the bitmasks computation into two passes:
                //   1/ Compute bitmasks for individual or small blocks of pixels
                //   2/ merge those individual bitmasks into per-tile bitmasks
                // provides better performance that doing it in a single above pass.
                if (m_DeferredLights.HasTileDepthRangeExtraPass())
                    EnqueuePass(m_TileDepthRangeExtraPass);
            }

            EnqueuePass(m_DeferredPass);
        }

        private struct RenderPassInputSummary
        {
            internal bool requiresDepthTexture;
            internal bool requiresDepthPrepass;
            internal bool requiresNormalsTexture;
            internal bool requiresColorTexture;
        }

        private RenderPassInputSummary GetRenderPassInputs(ref RenderingData renderingData)
        {
            //activeRenderPassQueue 渲染对列 一堆pass对象
            RenderPassInputSummary inputSummary = new RenderPassInputSummary();
            for (int i = 0; i < activeRenderPassQueue.Count; ++i)
            {
                ScriptableRenderPass pass = activeRenderPassQueue[i];

                // 是否需要深度图 _CameraDepthTexture
                //pass.input & ScriptableRenderPassInput.Depth   xxx & 1 ==> 取二进制的末位 
                bool needsDepth   = (pass.input & ScriptableRenderPassInput.Depth) != ScriptableRenderPassInput.None; 
                bool needsNormals = (pass.input & ScriptableRenderPassInput.Normal) != ScriptableRenderPassInput.None;

                //  _CameraColorTexture
                bool needsColor   = (pass.input & ScriptableRenderPassInput.Color) != ScriptableRenderPassInput.None;
                bool eventBeforeOpaque = pass.renderPassEvent <= RenderPassEvent.BeforeRenderingOpaques;

                inputSummary.requiresDepthTexture   |= needsDepth;
                inputSummary.requiresDepthPrepass   |= needsNormals || needsDepth && eventBeforeOpaque; //Eealy-Z
                inputSummary.requiresNormalsTexture |= needsNormals;
                inputSummary.requiresColorTexture   |= needsColor;
            }

            return inputSummary;
        }

        void CreateCameraRenderTarget(ScriptableRenderContext context, ref RenderTextureDescriptor descriptor, bool createColor, bool createDepth)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, Profiling.createCameraRenderTarget))
            {
                if (createColor)
                {
                    //创建颜色RT
                    // RenderTargetHandle.CameraTarget 直接代表FrameBuffer
                    bool useDepthRenderBuffer = m_ActiveCameraDepthAttachment == RenderTargetHandle.CameraTarget;
                    var colorDescriptor = descriptor; //复制了一份rt描述文件数据
                    colorDescriptor.useMipMap = false;
                    colorDescriptor.autoGenerateMips = false;
                    colorDescriptor.depthBufferBits = (useDepthRenderBuffer) ? k_DepthStencilBufferBits : 0;
                    cmd.GetTemporaryRT(m_ActiveCameraColorAttachment.id, colorDescriptor, FilterMode.Bilinear);
                }

                if (createDepth)
                {
                    //创建深度RT
                    var depthDescriptor = descriptor; //复制了一份rt描述文件数据
                    depthDescriptor.useMipMap = false;
                    depthDescriptor.autoGenerateMips = false;
#if ENABLE_VR && ENABLE_XR_MODULE
                    // XRTODO: Enabled this line for non-XR pass? URP copy depth pass is already capable of handling MSAA.
                    depthDescriptor.bindMS = depthDescriptor.msaaSamples > 1 && !SystemInfo.supportsMultisampleAutoResolve && (SystemInfo.supportsMultisampledTextures != 0);
#endif
                    depthDescriptor.colorFormat = RenderTextureFormat.Depth;
                    depthDescriptor.depthBufferBits = k_DepthStencilBufferBits;
                    cmd.GetTemporaryRT(m_ActiveCameraDepthAttachment.id, depthDescriptor, FilterMode.Point);
                }
            }
            //把commandbuffer的命令复制给context
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        bool PlatformRequiresExplicitMsaaResolve()
        {
            // On Metal/iOS the MSAA resolve is done implicitly as part of the renderpass, so we do not need an extra intermediate pass for the explicit autoresolve.
            // TODO: should also be valid on Metal MacOS/Editor, but currently not working as expected. Remove the "mobile only" requirement once trunk has a fix.

            return !SystemInfo.supportsMultisampleAutoResolve &&
                   !(SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal && Application.isMobilePlatform);
        }

        /// <summary>
        /// Checks if the pipeline needs to create a intermediate render texture.
        /// </summary>
        /// <param name="cameraData">CameraData contains all relevant render target information for the camera.</param>
        /// <seealso cref="CameraData"/>
        /// <returns>Return true if pipeline needs to render to a intermediate render texture.</returns>
        bool RequiresIntermediateColorTexture(ref CameraData cameraData)
        {
            // When rendering a camera stack we always create an intermediate render texture to composite camera results.
            // We create it upon rendering the Base camera.
            if (cameraData.renderType == CameraRenderType.Base && !cameraData.resolveFinalTarget)
                return true;

            // Always force rendering into intermediate color texture if deferred rendering mode is selected.
            // Reason: without intermediate color texture, the target camera texture is y-flipped.
            // However, the target camera texture is bound during gbuffer pass and deferred pass.
            // Gbuffer pass will not be y-flipped because it is MRT (see ScriptableRenderContext implementation),
            // while deferred pass will be y-flipped, which breaks rendering.
            // This incurs an extra blit into at the end of rendering.
            if (this.actualRenderingMode == RenderingMode.Deferred)
                return true;

            bool isSceneViewCamera = cameraData.isSceneViewCamera;
            var cameraTargetDescriptor = cameraData.cameraTargetDescriptor;
            int msaaSamples = cameraTargetDescriptor.msaaSamples;
            bool isScaledRender = !Mathf.Approximately(cameraData.renderScale, 1.0f);
            bool isCompatibleBackbufferTextureDimension = cameraTargetDescriptor.dimension == TextureDimension.Tex2D;
            bool requiresExplicitMsaaResolve = msaaSamples > 1 && PlatformRequiresExplicitMsaaResolve();
            bool isOffscreenRender = cameraData.targetTexture != null && !isSceneViewCamera;
            bool isCapturing = cameraData.captureActions != null;

#if ENABLE_VR && ENABLE_XR_MODULE
            if (cameraData.xr.enabled)
                isCompatibleBackbufferTextureDimension = cameraData.xr.renderTargetDesc.dimension == cameraTargetDescriptor.dimension;
#endif

            bool requiresBlitForOffscreenCamera = cameraData.postProcessEnabled || cameraData.requiresOpaqueTexture || requiresExplicitMsaaResolve || !cameraData.isDefaultViewport;
            if (isOffscreenRender)
                return requiresBlitForOffscreenCamera;

            return requiresBlitForOffscreenCamera || isSceneViewCamera || isScaledRender || cameraData.isHdrEnabled ||
                   !isCompatibleBackbufferTextureDimension || isCapturing || cameraData.requireSrgbConversion;
        }

        bool CanCopyDepth(ref CameraData cameraData)
        {
            //只要开启了MSAA就返回false
            bool msaaEnabledForCamera = cameraData.cameraTargetDescriptor.msaaSamples > 1;
            bool supportsTextureCopy = SystemInfo.copyTextureSupport != CopyTextureSupport.None;
            bool supportsDepthTarget = RenderingUtils.SupportsRenderTextureFormat(RenderTextureFormat.Depth);
            bool supportsDepthCopy = !msaaEnabledForCamera && (supportsDepthTarget || supportsTextureCopy);

            // TODO:  We don't have support to highp Texture2DMS currently and this breaks depth precision.
            // currently disabling it until shader changes kick in.
            //bool msaaDepthResolve = msaaEnabledForCamera && SystemInfo.supportsMultisampledTextures != 0;
            bool msaaDepthResolve = false;
            return supportsDepthCopy || msaaDepthResolve;
        }
    }
}
