using System;
using Unity.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Rendering.Universal;
#endif
using UnityEngine.Scripting.APIUpdating;
using Lightmapping = UnityEngine.Experimental.GlobalIllumination.Lightmapping;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling;

namespace UnityEngine.Rendering.LWRP
{
    [Obsolete("LWRP -> Universal (UnityUpgradable) -> UnityEngine.Rendering.Universal.UniversalRenderPipeline", true)]
    public class LightweightRenderPipeline
    {
        public LightweightRenderPipeline(LightweightRenderPipelineAsset asset)
        {
        }
    }
}

namespace UnityEngine.Rendering.Universal
{
    //继承RenderPipeline 实现render方法
    public sealed partial class UniversalRenderPipeline : RenderPipeline
    {
        public const string k_ShaderTagName = "UniversalPipeline";

        private static class Profiling
        {
            private static Dictionary<int, ProfilingSampler> s_HashSamplerCache = new Dictionary<int, ProfilingSampler>();
            public static readonly ProfilingSampler unknownSampler = new ProfilingSampler("Unknown");

            // Specialization for camera loop to avoid allocations.
            public static ProfilingSampler TryGetOrAddCameraSampler(Camera camera)
            {
                #if UNIVERSAL_PROFILING_NO_ALLOC
                    return unknownSampler;
                #else
                    ProfilingSampler ps = null;
                    int cameraId = camera.GetHashCode();
                    bool exists = s_HashSamplerCache.TryGetValue(cameraId, out ps);
                    if (!exists)
                    {
                        // NOTE: camera.name allocates!
                        ps = new ProfilingSampler( $"{nameof(UniversalRenderPipeline)}.{nameof(RenderSingleCamera)}: {camera.name}");
                        s_HashSamplerCache.Add(cameraId, ps);
                    }
                    return ps;
                #endif
            }

            public static class Pipeline
            {
                // TODO: Would be better to add Profiling name hooks into RenderPipeline.cs, requires changes outside of Universal.
#if UNITY_2021_1_OR_NEWER
                public static readonly ProfilingSampler beginContextRendering  = new ProfilingSampler($"{nameof(RenderPipeline)}.{nameof(BeginContextRendering)}");
                public static readonly ProfilingSampler endContextRendering    = new ProfilingSampler($"{nameof(RenderPipeline)}.{nameof(EndContextRendering)}");
#else
                public static readonly ProfilingSampler beginFrameRendering  = new ProfilingSampler($"{nameof(RenderPipeline)}.{nameof(BeginFrameRendering)}");
                public static readonly ProfilingSampler endFrameRendering    = new ProfilingSampler($"{nameof(RenderPipeline)}.{nameof(EndFrameRendering)}");
#endif
                public static readonly ProfilingSampler beginCameraRendering = new ProfilingSampler($"{nameof(RenderPipeline)}.{nameof(BeginCameraRendering)}");
                public static readonly ProfilingSampler endCameraRendering   = new ProfilingSampler($"{nameof(RenderPipeline)}.{nameof(EndCameraRendering)}");

                const string k_Name = nameof(UniversalRenderPipeline);
                public static readonly ProfilingSampler initializeCameraData           = new ProfilingSampler($"{k_Name}.{nameof(InitializeCameraData)}");
                public static readonly ProfilingSampler initializeStackedCameraData    = new ProfilingSampler($"{k_Name}.{nameof(InitializeStackedCameraData)}");
                public static readonly ProfilingSampler initializeAdditionalCameraData = new ProfilingSampler($"{k_Name}.{nameof(InitializeAdditionalCameraData)}");
                public static readonly ProfilingSampler initializeRenderingData        = new ProfilingSampler($"{k_Name}.{nameof(InitializeRenderingData)}");
                public static readonly ProfilingSampler initializeShadowData           = new ProfilingSampler($"{k_Name}.{nameof(InitializeShadowData)}");
                public static readonly ProfilingSampler initializeLightData            = new ProfilingSampler($"{k_Name}.{nameof(InitializeLightData)}");
                public static readonly ProfilingSampler getPerObjectLightFlags         = new ProfilingSampler($"{k_Name}.{nameof(GetPerObjectLightFlags)}");
                public static readonly ProfilingSampler getMainLightIndex              = new ProfilingSampler($"{k_Name}.{nameof(GetMainLightIndex)}");
                public static readonly ProfilingSampler setupPerFrameShaderConstants   = new ProfilingSampler($"{k_Name}.{nameof(SetupPerFrameShaderConstants)}");

                public static class Renderer
                {
                    const string k_Name = nameof(ScriptableRenderer);
                    public static readonly ProfilingSampler setupCullingParameters = new ProfilingSampler($"{k_Name}.{nameof(ScriptableRenderer.SetupCullingParameters)}");
                    public static readonly ProfilingSampler setup                  = new ProfilingSampler($"{k_Name}.{nameof(ScriptableRenderer.Setup)}");
                };

                public static class Context
                {
                    const string k_Name = nameof(Context);
                    public static readonly ProfilingSampler submit = new ProfilingSampler($"{k_Name}.{nameof(ScriptableRenderContext.Submit)}");
                };

                public static class XR
                {
                    public static readonly ProfilingSampler mirrorView = new ProfilingSampler("XR Mirror View");
                };
            };
        }

#if ENABLE_VR && ENABLE_XR_MODULE
        internal static XRSystem m_XRSystem = new XRSystem();
#endif

        public static float maxShadowBias
        {
            get => 10.0f;
        }

        public static float minRenderScale
        {
            get => 0.1f;
        }

        public static float maxRenderScale
        {
            get => 2.0f;
        }

        // Amount of Lights that can be shaded per object (in the for loop in the shader)
        public static int maxPerObjectLights
        {
            // No support to bitfield mask and int[] in gles2. Can't index fast more than 4 lights.
            // Check Lighting.hlsl for more details.
            get => (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2) ? 4 : 8;
        }

        // These limits have to match same limits in Input.hlsl
        const int k_MaxVisibleAdditionalLightsMobileShaderLevelLessThan45 = 16;
        const int k_MaxVisibleAdditionalLightsMobile    = 32;
        const int k_MaxVisibleAdditionalLightsNonMobile = 256;
        public static int maxVisibleAdditionalLights
        {
            get
            {
                bool isMobile = Application.isMobilePlatform;
                if (isMobile && (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2 || (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 && Graphics.minOpenGLESVersion <= OpenGLESVersion.OpenGLES30)))
                    return k_MaxVisibleAdditionalLightsMobileShaderLevelLessThan45;

                // GLES can be selected as platform on Windows (not a mobile platform) but uniform buffer size so we must use a low light count.
                return (isMobile || SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore || SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2 || SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3)
                        ? k_MaxVisibleAdditionalLightsMobile : k_MaxVisibleAdditionalLightsNonMobile;
            }
        }
        
        //UniversalRenderPipelineAsset的CreatePipeline方法会创建一个UniversalRenderPipeline实例，调用构造方法
        public UniversalRenderPipeline(UniversalRenderPipelineAsset asset)
        {
            //仅仅Editor下生效
            SetSupportedRenderingFeatures();
            
            //抗锯齿设置 MSAA
            // In QualitySettings.antiAliasing disabled state uses value 0, where in URP 1
            int qualitySettingsMsaaSampleCount = QualitySettings.antiAliasing > 0 ? QualitySettings.antiAliasing : 1;
            bool msaaSampleCountNeedsUpdate = qualitySettingsMsaaSampleCount != asset.msaaSampleCount;

            // Let engine know we have MSAA on for cases where we support MSAA backbuffer
            if (msaaSampleCountNeedsUpdate)
            {
                //项目启动就是设置MSAA的样式
                QualitySettings.antiAliasing = asset.msaaSampleCount;
#if ENABLE_VR && ENABLE_XR_MODULE
                XRSystem.UpdateMSAALevel(asset.msaaSampleCount);
#endif
            }

#if ENABLE_VR && ENABLE_XR_MODULE
            XRSystem.UpdateRenderScale(asset.renderScale);
#endif
            // For compatibility reasons we also match old LightweightPipeline tag.
            Shader.globalRenderPipeline = "UniversalPipeline,LightweightPipeline";

            Lightmapping.SetDelegate(lightsDelegate);

            CameraCaptureBridge.enabled = true;

            RenderingUtils.ClearSystemInfoCache();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            Shader.globalRenderPipeline = "";
            SupportedRenderingFeatures.active = new SupportedRenderingFeatures();
            ShaderData.instance.Dispose();
            DeferredShaderData.instance.Dispose();

#if ENABLE_VR && ENABLE_XR_MODULE
            m_XRSystem?.Dispose();
#endif

#if UNITY_EDITOR
            SceneViewDrawMode.ResetDrawMode();
#endif
            Lightmapping.ResetDelegate();
            CameraCaptureBridge.enabled = false;
        }

#if UNITY_2021_1_OR_NEWER
        protected override void Render(ScriptableRenderContext renderContext,  Camera[] cameras)
        {
            Render(renderContext, new List<Camera>(cameras));
        }
#endif

#if UNITY_2021_1_OR_NEWER
        protected override void Render(ScriptableRenderContext renderContext, List<Camera> cameras)
#else

        //URP渲染管线的入口
        protected override void Render(ScriptableRenderContext renderContext, Camera[] cameras)
#endif
        {
            // TODO: Would be better to add Profiling name hooks into RenderPipelineManager.
            // C#8 feature, only in >= 2020.2
            using var profScope = new ProfilingScope(null, ProfilingSampler.Get(URPProfileId.UniversalRenderTotal));

#if UNITY_2021_1_OR_NEWER
            using (new ProfilingScope(null, Profiling.Pipeline.beginContextRendering))
            {
                BeginContextRendering(renderContext, cameras);
            }
#else
            using(new ProfilingScope(null, Profiling.Pipeline.beginFrameRendering))
            {
                //发送一个渲染开始事件，发送RenderPipelineManager.beginFrameRendering事件
                //PixelPerfectCamera的OnEnable方法里注册，这个脚本好像一般不会添加
                BeginFrameRendering(renderContext, cameras);
            }
#endif
            //是否使用线性空间
            GraphicsSettings.lightsUseLinearIntensity = (QualitySettings.activeColorSpace == ColorSpace.Linear);

            //是否开启SRP Batcher
            GraphicsSettings.useScriptableRenderPipelineBatching = asset.useSRPBatcher;
            
            //设置shader变量值
            //主要设置了未开启环境反射时的默认颜色、阴影颜色
            SetupPerFrameShaderConstants();
#if ENABLE_VR && ENABLE_XR_MODULE
            // Update XR MSAA level per frame.
            XRSystem.UpdateMSAALevel(asset.msaaSampleCount);
#endif

            //通过相机深度进行排序
            SortCameras(cameras);
#if UNITY_2021_1_OR_NEWER
            for (int i = 0; i < cameras.Count; ++i)
#else
            //逐相机渲染
            for (int i = 0; i < cameras.Length; ++i)
#endif
            {
                var camera = cameras[i];
                //Game窗口相机
                if (IsGameCamera(camera))
                {
                    //每个相机渲染的总入口
                    RenderCameraStack(renderContext, camera);
                }
                else
                {
                    //scene窗口相机
                    using (new ProfilingScope(null, Profiling.Pipeline.beginCameraRendering))
                    {
                        BeginCameraRendering(renderContext, camera);
                    }
#if VISUAL_EFFECT_GRAPH_0_0_1_OR_NEWER
                //It should be called before culling to prepare material. When there isn't any VisualEffect component, this method has no effect.
                VFX.VFXManager.PrepareCamera(camera);
#endif
                    UpdateVolumeFramework(camera, null);

                    RenderSingleCamera(renderContext, camera);

                    using (new ProfilingScope(null, Profiling.Pipeline.endCameraRendering))
                    {
                        EndCameraRendering(renderContext, camera);
                    }
                }
            }
#if UNITY_2021_1_OR_NEWER
            using (new ProfilingScope(null, Profiling.Pipeline.endContextRendering))
            {
                EndContextRendering(renderContext, cameras);
            }
#else
            using(new ProfilingScope(null, Profiling.Pipeline.endFrameRendering))
            {
                EndFrameRendering(renderContext, cameras);
            }
#endif
        }

        //每个相机单独的渲染入口
        /// <summary>
        /// Standalone camera rendering. Use this to render procedural cameras.
        /// This method doesn't call <c>BeginCameraRendering</c> and <c>EndCameraRendering</c> callbacks.
        /// </summary>
        /// <param name="context">Render context used to record commands during execution.</param>
        /// <param name="camera">Camera to render.</param>
        /// <seealso cref="ScriptableRenderContext"/>
        public static void RenderSingleCamera(ScriptableRenderContext context, Camera camera)
        {
            UniversalAdditionalCameraData additionalCameraData = null;
            if (IsGameCamera(camera))
                camera.gameObject.TryGetComponent(out additionalCameraData);

            if (additionalCameraData != null && additionalCameraData.renderType != CameraRenderType.Base)
            {
                Debug.LogWarning("Only Base cameras can be rendered with standalone RenderSingleCamera. Camera will be skipped.");
                return;
            }

            InitializeCameraData(camera, additionalCameraData, true, out var cameraData);
#if ADAPTIVE_PERFORMANCE_2_0_0_OR_NEWER
            if (asset.useAdaptivePerformance)
                ApplyAdaptivePerformance(ref cameraData);
#endif
            RenderSingleCamera(context, cameraData, cameraData.postProcessEnabled);
        }

        static bool TryGetCullingParameters(CameraData cameraData, out ScriptableCullingParameters cullingParams)
        {
#if ENABLE_VR && ENABLE_XR_MODULE
            if (cameraData.xr.enabled)
            {
                cullingParams = cameraData.xr.cullingParams;

                // Sync the FOV on the camera to match the projection from the XR device
                if (!cameraData.camera.usePhysicalProperties)
                    cameraData.camera.fieldOfView = Mathf.Rad2Deg * Mathf.Atan(1.0f / cullingParams.stereoProjectionMatrix.m11) * 2.0f;

                return true;
            }
#endif
            //最后还是调用camera的裁剪方法
            /*
       找出什么可以被剔除需要我们跟踪多个相机设置和矩阵，可以使用ScriptableCullingParameters结构。
       这个结构可以在摄像机上调用TryGetCullingParameters，而不是自己去填充它。它返回是否可以成功检索该参数，因为它可能会获取失败。
       要获得参数数据，我们必须将其作为输出(out)参数提供，方法是在它前面写一个out。在返回成功或失败的单独的Cull方法中执行此操作
       */
            return cameraData.camera.TryGetCullingParameters(false, out cullingParams);
        }

        /// <summary>
        /// Renders a single camera. This method will do culling, setup and execution of the renderer.
        /// </summary>
        /// <param name="context">Render context used to record commands during execution.</param>
        /// <param name="cameraData">Camera rendering data. This might contain data inherited from a base camera.</param>
        /// <param name="anyPostProcessingEnabled">True if at least one camera has post-processing enabled in the stack, false otherwise.</param>
        static void RenderSingleCamera(ScriptableRenderContext context, CameraData cameraData, bool anyPostProcessingEnabled)
        {
            Camera camera = cameraData.camera; //摄像机对象
            var renderer = cameraData.renderer;//渲染器对象
            if (renderer == null)
            {
                Debug.LogWarning(string.Format("Trying to render {0} with an invalid renderer. Camera rendering will be skipped.", camera.name));
                return;
            }

            //调用裁剪接口 CPU层先裁剪一遍 避免多余的点进行空间位置转换
            //返回失败会直接退出，成功的话会调用context.Cull(ref cullingParameters)进行真正裁剪
            if (!TryGetCullingParameters(cameraData, out var cullingParameters))
                return;

            ScriptableRenderer.current = renderer;
            bool isSceneViewCamera = cameraData.isSceneViewCamera;

            // NOTE: Do NOT mix ProfilingScope with named CommandBuffers i.e. CommandBufferPool.Get("name").
            // Currently there's an issue which results in mismatched markers.
            // The named CommandBuffer will close its "profiling scope" on execution.
            // That will orphan ProfilingScope markers as the named CommandBuffer markers are their parents.
            // Resulting in following pattern:
            // exec(cmd.start, scope.start, cmd.end) and exec(cmd.start, scope.end, cmd.end)
            //从对象池获取一个CommandBuffer对象
            CommandBuffer cmd = CommandBufferPool.Get();
            ProfilingSampler sampler = Profiling.TryGetOrAddCameraSampler(camera);
            using (new ProfilingScope(cmd, sampler)) // Enqueues a "BeginSample" command into the CommandBuffer cmd
            {
                //重置RT附件，颜色RT，深度RT，
                //设置颜色缓冲区目标和深度缓冲区目标为屏幕
                renderer.Clear(cameraData.renderType);

                using (new ProfilingScope( cmd, Profiling.Pipeline.Renderer.setupCullingParameters))
                {
                    //override 方法 设置裁剪参数 每个Render可以自己实现, 一般是调用forwardRender的SetupCullingParameters方法
                    renderer.SetupCullingParameters(ref cullingParameters, ref cameraData);
                }
                
                //将commanderbuffer中所有的命令都发给context，其实是一个复制的过程
                context.ExecuteCommandBuffer(cmd); // Send all the commands enqueued so far in the CommandBuffer cmd, to the ScriptableRenderContext context
                //发送完清除，可复用对象
                cmd.Clear();

#if UNITY_EDITOR
                // Emit scene view UI
                if (isSceneViewCamera)
                {
                    ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
                }
#endif
                //进行裁剪，得到裁剪结果
                /*
               实际的裁剪是通过调用上下文上的Cull来完成的，这会产生一个CullingResults结构。
               如果成功的话，可以在清除中执行此操作，并将结果存储在字段中。
               在这种情况下，我们必须将剔除参数作为引用参数传递，方法是在前面写ref。
            */
                var cullResults = context.Cull(ref cullingParameters);
                //填充RenderingData结构体，这个结构体包含渲染相关的所有参数，裁剪信息，相机数据，光照阴影数据，后处理等很多
                InitializeRenderingData(asset, ref cameraData, ref cullResults, anyPostProcessingEnabled, out var renderingData);

#if ADAPTIVE_PERFORMANCE_2_0_0_OR_NEWER
                if (asset.useAdaptivePerformance)
                    ApplyAdaptivePerformance(ref renderingData);
#endif

                using (new ProfilingScope(cmd, Profiling.Pipeline.Renderer.setup))
                {
                    //抽象方法 每个Render自己实现 一般是调用forwardRender的Setup方法
                    renderer.Setup(context, ref renderingData);
                }

                // Timing scope inside 这里是直接调用基类ScriptableRenderer的Execute方法，一般都不重载
                renderer.Execute(context, ref renderingData);

            } // When ProfilingSample goes out of scope, an "EndSample" command is enqueued into CommandBuffer cmd

            cameraData.xr.EndCamera(cmd, cameraData);
            //再次复制commandbuffer里的命令给context
            context.ExecuteCommandBuffer(cmd); // Sends to ScriptableRenderContext all the commands enqueued since cmd.Clear, i.e the "EndSample" command
            CommandBufferPool.Release(cmd);

            using (new ProfilingScope(cmd, Profiling.Pipeline.Context.submit))
            {
                //真正执行command的地方，把之前的设置统一执行一遍, 提交给GPU
                context.Submit(); // Actually execute the commands that we previously sent to the ScriptableRenderContext context
            }

            ScriptableRenderer.current = null;
        }

        /// <summary>
        // Renders a camera stack. This method calls RenderSingleCamera for each valid camera in the stack.
        // The last camera resolves the final target to screen.
        /// </summary>
        /// <param name="context">Render context used to record commands during execution.</param>
        /// <param name="camera">Camera to render.</param>
        static void RenderCameraStack(ScriptableRenderContext context, Camera baseCamera)
        {
            using var profScope = new ProfilingScope(null, ProfilingSampler.Get(URPProfileId.RenderCameraStack));
            
            //读取UniversalAdditionalCameraData数据 可以理解为UniversalAdditionalCameraData其实就是camera的一个分身数据代表,
            //有camera的数据也有自己数据
            //UniversalAdditionalCameraDatas 组件上有一个相机的stack
            baseCamera.TryGetComponent<UniversalAdditionalCameraData>(out var baseCameraAdditionalData);
            
            // 当渲染baseCamera时会把rendered stacked里的overlayer camera渲染了，这里就不用再次渲染了直接返回
            // Overlay cameras will be rendered stacked while rendering base cameras
            if (baseCameraAdditionalData != null && baseCameraAdditionalData.renderType == CameraRenderType.Overlay)
                return;

            // renderer contains a stack if it has additional data and the renderer supports stacking
            //m_Renderers使用的是Quality面板里设置的渲染资产文件对应的渲染器列表,
            var renderer = baseCameraAdditionalData?.scriptableRenderer; //当前相机使用的渲染器对象
            bool supportsCameraStacking = renderer != null && renderer.supportedRenderingFeatures.cameraStacking;
            List<Camera> cameraStack = (supportsCameraStacking) ? baseCameraAdditionalData?.cameraStack : null;
            
            //baseCamera是否开启后效
            bool anyPostProcessingEnabled = baseCameraAdditionalData != null && baseCameraAdditionalData.renderPostProcessing;
            
            
            //确定最后一个激活的摄像机，来把最后的渲染数据解析到屏幕上（其实就是用最后一个激活相机渲染到屏幕）
            // We need to know the last active camera in the stack to be able to resolve
            // rendering to screen when rendering it. The last camera in the stack is not
            // necessarily the last active one as it users might disable it.
            int lastActiveOverlayCameraIndex = -1;
            if (cameraStack != null)
            {
                //baseCamera使用的渲染器对象类型，这里为ForwardRenderer
                var baseCameraRendererType = baseCameraAdditionalData?.scriptableRenderer.GetType();
                
                //是否需要刷新CameraStack
                bool shouldUpdateCameraStack = false;

                for (int i = 0; i < cameraStack.Count; ++i)
                {
                    Camera currCamera = cameraStack[i];
                    if (currCamera == null)
                    {
                        //有空数据是需要刷新stack
                        shouldUpdateCameraStack = true;
                        continue;
                    }

                    if (currCamera.isActiveAndEnabled)
                    {
                        currCamera.TryGetComponent<UniversalAdditionalCameraData>(out var data);

                        if (data == null || data.renderType != CameraRenderType.Overlay)
                        {
                            Debug.LogWarning(string.Format("Stack can only contain Overlay cameras. {0} will skip rendering.", currCamera.name));
                            continue;
                        }
                        //CameraStack里的相机必须是overlay类型
                        var currCameraRendererType = data?.scriptableRenderer.GetType();
                        if (currCameraRendererType != baseCameraRendererType)
                        {
                            //overlay相机和base相机好的渲染器对象类型不一致会判断是否要跳过
                            var renderer2DType = typeof(Experimental.Rendering.Universal.Renderer2D);
                            if (currCameraRendererType != renderer2DType && baseCameraRendererType != renderer2DType)
                            {
                                Debug.LogWarning(string.Format("Only cameras with compatible renderer types can be stacked. {0} will skip rendering", currCamera.name));
                                continue;
                            }
                        }
                        //按位或运算符(|)：0 | 0 = 0, 0 | 1 = 1, 1 | 0 = 1, 1 | 1 = 1  ==> 两个都为0才为0，否则为1
                        //anyPostProcessingEnabled为布尔值，true为1 false为0
                        //意思其实就是只要有一个相机开启来后效最后就会暂时开启执行后效的逻辑，后面还有判断
                        anyPostProcessingEnabled |= data.renderPostProcessing;
                        
                        //确定最后一个可激活相机在stack里的下标
                        lastActiveOverlayCameraIndex = i;
                    }
                }
                if(shouldUpdateCameraStack)
                {
                    //更新baseCamera的CameraStack信息
                    baseCameraAdditionalData.UpdateCameraStack();
                }
            }

            // Post-processing not supported in GLES2.
            // 按位与运算符(&) ： 0 & 0 = 0, 0 & 1 = 0, 1 & 0 = 0, 1 & 1 = 1。==> 两个都为1才为1，否则为0
            anyPostProcessingEnabled &= SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES2;
            
            //lastActiveOverlayCameraIndex为-1时，表示basecamera的stack里有相机需要渲染
            bool isStackedRendering = lastActiveOverlayCameraIndex != -1;
            
            //对volume系统的支持，核心是调用VolumeManager的Update方法 TODO
            // Update volumeframework before initializing additional camera data  更新后期
            UpdateVolumeFramework(baseCamera, baseCameraAdditionalData);

            //给相机初始化数据CameraData，使用baseCameraAdditionalData填充CameraData结构体，这里传入的是baseCamera
            //isStackedRendering标识stack里有相机需要渲染
            /*
             *    //cameraData初始化了一些基本数据： InitializeStackedCameraData
             *         targetTexture，cameraType，volumeLayerMask，抗锯齿，HDR，屏幕大小数据，renderScale，RT描述器对象
                 
                 //cameraData初始化了一些额外数据：InitializeAdditionalCameraData
                        camera对象，renderType，是否清除深度，是否开启后效（后面还有判断），最大阴影距离，是否需要深度贴图，是否需要颜色贴图，渲染器对象，
                        是否需要最后绘制到屏幕上，矩阵信息
             */
            InitializeCameraData(baseCamera, baseCameraAdditionalData, !isStackedRendering, out var baseCameraData);

#if ENABLE_VR && ENABLE_XR_MODULE
            var originalTargetDesc = baseCameraData.cameraTargetDescriptor;
            var xrActive = false;
            var xrPasses = m_XRSystem.SetupFrame(baseCameraData);
            foreach (XRPass xrPass in xrPasses)
            {
                baseCameraData.xr = xrPass;

                // XRTODO: remove isStereoEnabled in 2021.x
#pragma warning disable 0618
                baseCameraData.isStereoEnabled = xrPass.enabled;
#pragma warning restore 0618

                if (baseCameraData.xr.enabled)
                {
                    xrActive = true;
                    // Helper function for updating cameraData with xrPass Data
                    m_XRSystem.UpdateCameraData(ref baseCameraData, baseCameraData.xr);
                }
#endif
                using(new ProfilingScope(null, Profiling.Pipeline.beginCameraRendering))
                {
                    //发送渲染开始事件，如果事件注册了就会有回调信息
                    BeginCameraRendering(context, baseCamera);
                }
#if VISUAL_EFFECT_GRAPH_0_0_1_OR_NEWER
                //It should be called before culling to prepare material. When there isn't any VisualEffect component, this method has no effect.
                VFX.VFXManager.PrepareCamera(baseCamera);
#endif
#if ADAPTIVE_PERFORMANCE_2_0_0_OR_NEWER
                if (asset.useAdaptivePerformance)
                    ApplyAdaptivePerformance(ref baseCameraData);
#endif
                //每个相机真正的渲染
                //anyPostProcessingEnabled 只要有一个相机开启了后效这个值就为true
                RenderSingleCamera(context, baseCameraData, anyPostProcessingEnabled);
                using (new ProfilingScope(null, Profiling.Pipeline.endCameraRendering))
                {
                    EndCameraRendering(context, baseCamera);
                }

                
                if (isStackedRendering)
                {
                    //Stack里的overlay相机开始渲染, 走一遍跟base相机一样的流程
                    for (int i = 0; i < cameraStack.Count; ++i)
                    {
                        var currCamera = cameraStack[i];
                        if (!currCamera.isActiveAndEnabled)
                            continue;
                        //拿到每个overlay相机的UniversalAdditionalCameraData数据，其实就是对应camera的一个分身数据
                        currCamera.TryGetComponent<UniversalAdditionalCameraData>(out var currCameraData);
                        // Camera is overlay and enabled
                        if (currCameraData != null)
                        {
                            //copy 一份baseCamera数据
                            // Copy base settings from base camera data and initialize initialize remaining specific settings for this camera type.
                            CameraData overlayCameraData = baseCameraData;
                            
                            //判断是否是最后一个激活相机
                            bool lastCamera = i == lastActiveOverlayCameraIndex;

                            using (new ProfilingScope(null, Profiling.Pipeline.beginCameraRendering))
                            {
                                BeginCameraRendering(context, currCamera);
                            }
#if VISUAL_EFFECT_GRAPH_0_0_1_OR_NEWER
                            //It should be called before culling to prepare material. When there isn't any VisualEffect component, this method has no effect.
                            VFX.VFXManager.PrepareCamera(currCamera);
#endif
                            //调用overlay相机的VolumeManager的Update方法，后效方法
                            UpdateVolumeFramework(currCamera, currCameraData);
                            //仅仅只初始化了一些额外数据
                            InitializeAdditionalCameraData(currCamera, currCameraData, lastCamera, ref overlayCameraData);
#if ENABLE_VR && ENABLE_XR_MODULE
                            if (baseCameraData.xr.enabled)
                                m_XRSystem.UpdateFromCamera(ref overlayCameraData.xr, overlayCameraData);
#endif
                            //overlay相机 填充RenderingData结构体，这个结构体包含渲染相关的所有参数，裁剪信息，相机数据，光照阴影数据，后处理等很多
                            RenderSingleCamera(context, overlayCameraData, anyPostProcessingEnabled);

                            using (new ProfilingScope(null, Profiling.Pipeline.endCameraRendering))
                            {
                                EndCameraRendering(context, currCamera);
                            }
                        }
                    }
                }

#if ENABLE_VR && ENABLE_XR_MODULE
                if (baseCameraData.xr.enabled)
                    baseCameraData.cameraTargetDescriptor = originalTargetDesc;
            }

            if (xrActive)
            {
                CommandBuffer cmd = CommandBufferPool.Get();
                using (new ProfilingScope(cmd, Profiling.Pipeline.XR.mirrorView))
                {
                    m_XRSystem.RenderMirrorView(cmd, baseCamera);
                }

                context.ExecuteCommandBuffer(cmd);
                context.Submit();
                CommandBufferPool.Release(cmd);
            }

            m_XRSystem.ReleaseFrame();
#endif
        }

        static void UpdateVolumeFramework(Camera camera, UniversalAdditionalCameraData additionalCameraData)
        {
            using var profScope = new ProfilingScope(null, ProfilingSampler.Get(URPProfileId.UpdateVolumeFramework));

            // Default values when there's no additional camera data available
            LayerMask layerMask = 1; // "Default"
            Transform trigger = camera.transform;

            if (additionalCameraData != null)
            {
                layerMask = additionalCameraData.volumeLayerMask;
                trigger = additionalCameraData.volumeTrigger != null
                    ? additionalCameraData.volumeTrigger
                    : trigger;
            }
            else if (camera.cameraType == CameraType.SceneView)
            {
                // Try to mirror the MainCamera volume layer mask for the scene view - do not mirror the target
                var mainCamera = Camera.main;
                UniversalAdditionalCameraData mainAdditionalCameraData = null;

                if (mainCamera != null && mainCamera.TryGetComponent(out mainAdditionalCameraData))
                    layerMask = mainAdditionalCameraData.volumeLayerMask;

                trigger = mainAdditionalCameraData != null && mainAdditionalCameraData.volumeTrigger != null ? mainAdditionalCameraData.volumeTrigger : trigger;
            }

            VolumeManager.instance.Update(trigger, layerMask);
        }

        static bool CheckPostProcessForDepth(in CameraData cameraData)
        {
            if (!cameraData.postProcessEnabled)
                return false;

            if (cameraData.antialiasing == AntialiasingMode.SubpixelMorphologicalAntiAliasing)
                return true;

            var stack = VolumeManager.instance.stack;

            //如果开启了景深和运动模糊后期效果，会自动生成深度图
            if (stack.GetComponent<DepthOfField>().IsActive())
                return true;

            if (stack.GetComponent<MotionBlur>().IsActive())
                return true;

            return false;
        }

        static void SetSupportedRenderingFeatures()
        {
#if UNITY_EDITOR
            SupportedRenderingFeatures.active = new SupportedRenderingFeatures()
            {
                reflectionProbeModes = SupportedRenderingFeatures.ReflectionProbeModes.None,
                defaultMixedLightingModes = SupportedRenderingFeatures.LightmapMixedBakeModes.Subtractive,
                mixedLightingModes = SupportedRenderingFeatures.LightmapMixedBakeModes.Subtractive | SupportedRenderingFeatures.LightmapMixedBakeModes.IndirectOnly | SupportedRenderingFeatures.LightmapMixedBakeModes.Shadowmask,
                lightmapBakeTypes = LightmapBakeType.Baked | LightmapBakeType.Mixed,
                lightmapsModes = LightmapsMode.CombinedDirectional | LightmapsMode.NonDirectional,
                lightProbeProxyVolumes = false,
                motionVectors = false,
                receiveShadows = false,
                reflectionProbes = true,
                particleSystemInstancing = true
            };
            SceneViewDrawMode.SetupDrawMode();
#endif
        }
        
        //resolveFinalTarget表示当前相机是否是需要最后绘制到屏幕上
        static void InitializeCameraData(Camera camera, UniversalAdditionalCameraData additionalCameraData, bool resolveFinalTarget, out CameraData cameraData)
        {
            using var profScope = new ProfilingScope(null, Profiling.Pipeline.initializeCameraData);

            cameraData = new CameraData();
            
            //每个相机都有自己的CameraData对象  additionalCameraData可以理解成为camera组件的一个副身 跟camera有一样的功能
            
            //cameraData初始化了一些基本数据：targetTexture，cameraType，volumeLayerMask，抗锯齿，HDR，屏幕大小数据，renderScale，RT描述器对象
            InitializeStackedCameraData(camera, additionalCameraData, ref cameraData);
            
            //cameraData初始化了一些额外数据：感觉这个信息比较重要
            //camera对象，renderType，是否清除深度，是否开启后效（后面还有判断），最大阴影距离，是否需要深度贴图，是否需要颜色贴图，渲染器对象，
            //是否需要最后绘制到屏幕上，矩阵信息
            InitializeAdditionalCameraData(camera, additionalCameraData, resolveFinalTarget, ref cameraData);
        }

        /// <summary>
        /// Initialize camera data settings common for all cameras in the stack. Overlay cameras will inherit
        /// settings from base camera.
        /// </summary>
        /// <param name="baseCamera">Base camera to inherit settings from.</param>
        /// <param name="baseAdditionalCameraData">Component that contains additional base camera data.</param>
        /// <param name="cameraData">Camera data to initialize setttings.</param>
        static void InitializeStackedCameraData(Camera baseCamera, UniversalAdditionalCameraData baseAdditionalCameraData, ref CameraData cameraData)
        {
            using var profScope = new ProfilingScope(null, Profiling.Pipeline.initializeStackedCameraData);

            var settings = asset;
            
            //复制camera的targetTexture
            cameraData.targetTexture = baseCamera.targetTexture;
            
            //复制camera的cameraType, 这个是枚举CameraType：  Game = 1, SceneView=2 Preview = 4,VR = 8,Reflection = 16
            cameraData.cameraType = baseCamera.cameraType;
            bool isSceneViewCamera = cameraData.isSceneViewCamera;

            ///////////////////////////////////////////////////////////////////
            // Environment and Post-processing settings                       /
            ///////////////////////////////////////////////////////////////////
            if (isSceneViewCamera)
            {
                //场景相机
                cameraData.volumeLayerMask = 1; // "Default"
                cameraData.volumeTrigger = null;
                cameraData.isStopNaNEnabled = false;
                cameraData.isDitheringEnabled = false;
                cameraData.antialiasing = AntialiasingMode.None;
                cameraData.antialiasingQuality = AntialiasingQuality.High;
#if ENABLE_VR && ENABLE_XR_MODULE
                cameraData.xrRendering = false;
#endif
            }
            else if (baseAdditionalCameraData != null)
            {
                // 基本都走这
                cameraData.volumeLayerMask = baseAdditionalCameraData.volumeLayerMask;
                cameraData.volumeTrigger = baseAdditionalCameraData.volumeTrigger == null ? baseCamera.transform : baseAdditionalCameraData.volumeTrigger;
                cameraData.isStopNaNEnabled = baseAdditionalCameraData.stopNaN && SystemInfo.graphicsShaderLevel >= 35;
                cameraData.isDitheringEnabled = baseAdditionalCameraData.dithering; //抖动？
                cameraData.antialiasing = baseAdditionalCameraData.antialiasing; //抗锯齿
                cameraData.antialiasingQuality = baseAdditionalCameraData.antialiasingQuality;  //抗锯齿质量
#if ENABLE_VR && ENABLE_XR_MODULE
                cameraData.xrRendering = baseAdditionalCameraData.allowXRRendering && m_XRSystem.RefreshXrSdk();
#endif
            }
            else
            {
                cameraData.volumeLayerMask = 1; // "Default"
                cameraData.volumeTrigger = null;
                cameraData.isStopNaNEnabled = false;
                cameraData.isDitheringEnabled = false;
                cameraData.antialiasing = AntialiasingMode.None;
                cameraData.antialiasingQuality = AntialiasingQuality.High;
#if ENABLE_VR && ENABLE_XR_MODULE
                cameraData.xrRendering = m_XRSystem.RefreshXrSdk();
#endif
            }

            ///////////////////////////////////////////////////////////////////
            // Settings that control output of the camera                     /
            ///////////////////////////////////////////////////////////////////

            var renderer = baseAdditionalCameraData?.scriptableRenderer;
            //渲染器对象的Features是否支持MSAA
            bool rendererSupportsMSAA = renderer != null && renderer.supportedRenderingFeatures.msaa;
            
            //设置msaa的超采样数
            int msaaSamples = 1;
            if (baseCamera.allowMSAA && settings.msaaSampleCount > 1 && rendererSupportsMSAA)
                msaaSamples = (baseCamera.targetTexture != null) ? baseCamera.targetTexture.antiAliasing : settings.msaaSampleCount;
#if ENABLE_VR && ENABLE_XR_MODULE
            // Use XR's MSAA if camera is XR camera. XR MSAA needs special handle here because it is not per Camera.
            // Multiple cameras could render into the same XR display and they should share the same MSAA level.
            if (cameraData.xrRendering)
                msaaSamples = XRSystem.GetMSAALevel();
#endif
            //复制是否支持HDR，需要相机和渲染资产文件同时开启
            cameraData.isHdrEnabled = baseCamera.allowHDR && settings.supportsHDR;
            
            //复制camera屏幕大小数据
            Rect cameraRect = baseCamera.rect;
            cameraData.pixelRect = baseCamera.pixelRect;
            cameraData.pixelWidth = baseCamera.pixelWidth; //屏幕宽
            cameraData.pixelHeight = baseCamera.pixelHeight; //屏幕高
            cameraData.aspectRatio = (float)cameraData.pixelWidth / (float)cameraData.pixelHeight;
            //是否是默认视口（看到全屏）
            cameraData.isDefaultViewport = (!(Math.Abs(cameraRect.x) > 0.0f || Math.Abs(cameraRect.y) > 0.0f ||
                Math.Abs(cameraRect.width) < 1.0f || Math.Abs(cameraRect.height) < 1.0f));
            
            //复制摄像机的可变分辨率缩放比例，
            // Discard variations lesser than kRenderScaleThreshold.
            // Scale is only enabled for gameview.
            const float kRenderScaleThreshold = 0.05f;
            cameraData.renderScale = (Mathf.Abs(1.0f - settings.renderScale) < kRenderScaleThreshold) ? 1.0f : settings.renderScale;

#if ENABLE_VR && ENABLE_XR_MODULE
            cameraData.xr = m_XRSystem.emptyPass;
            XRSystem.UpdateRenderScale(cameraData.renderScale);
#else
            cameraData.xr = XRPass.emptyPass;
#endif
            //渲染顺序设置 SortingCriteria How to sort objects during rendering
            var commonOpaqueFlags = SortingCriteria.CommonOpaque; // 按不透明物体进行排序
            //这几种类型只要有一种返回就为true，意味着noFrontToBackOpaqueFlags为true
            var noFrontToBackOpaqueFlags = SortingCriteria.SortingLayer | SortingCriteria.RenderQueue | SortingCriteria.OptimizeStateChanges | SortingCriteria.CanvasOrder;
            bool hasHSRGPU = SystemInfo.hasHiddenSurfaceRemovalOnGPU; //一般为false
            //是否要跳过从前往后排序
            bool canSkipFrontToBackSorting = (baseCamera.opaqueSortMode == OpaqueSortMode.Default && hasHSRGPU) || baseCamera.opaqueSortMode == OpaqueSortMode.NoDistanceSort;
            
            //设置不透明物体的渲染对象排序方式，一般都为commonOpaqueFlags，从前往后排
            cameraData.defaultOpaqueSortFlags = canSkipFrontToBackSorting ? noFrontToBackOpaqueFlags : commonOpaqueFlags;
            cameraData.captureActions = CameraCaptureBridge.GetCaptureActions(baseCamera);

            bool needsAlphaChannel = Graphics.preserveFramebufferAlpha; //预乘alpha
            //设置RT的渲染器描述对象，后面可以只用它来创建rt：屏幕大小，HDR，MSAA，sRGB。。。
            cameraData.cameraTargetDescriptor = CreateRenderTextureDescriptor(baseCamera, cameraData.renderScale,
                cameraData.isHdrEnabled, msaaSamples, needsAlphaChannel);
        }

        /// <summary>
        /// Initialize settings that can be different for each camera in the stack.
        /// </summary>
        /// <param name="camera">Camera to initialize settings from.</param>
        /// <param name="additionalCameraData">Additional camera data component to initialize settings from.</param>
        /// <param name="resolveFinalTarget">True if this is the last camera in the stack and rendering should resolve to camera target.</param>
        /// <param name="cameraData">Settings to be initilized.</param>
        static void InitializeAdditionalCameraData(Camera camera, UniversalAdditionalCameraData additionalCameraData, bool resolveFinalTarget, ref CameraData cameraData)
        {
            using var profScope = new ProfilingScope(null, Profiling.Pipeline.initializeAdditionalCameraData);

            var settings = asset;
            
            //复制camera对象
            cameraData.camera = camera;
            
            //阴影最大距离数据maxShadowDistance，shadowMask模式，距离之内使用实时阴影，超过最大距离使用烘焙阴影
            bool anyShadowsEnabled = settings.supportsMainLightShadows || settings.supportsAdditionalLightShadows;
            cameraData.maxShadowDistance = Mathf.Min(settings.shadowDistance, camera.farClipPlane);
            cameraData.maxShadowDistance = (anyShadowsEnabled && cameraData.maxShadowDistance >= camera.nearClipPlane) ? cameraData.maxShadowDistance : 0.0f;

            bool isSceneViewCamera = cameraData.isSceneViewCamera;
            if (isSceneViewCamera)
            {
                cameraData.renderType = CameraRenderType.Base;
                cameraData.clearDepth = true;
                cameraData.postProcessEnabled = CoreUtils.ArePostProcessesEnabled(camera);
                cameraData.requiresDepthTexture = settings.supportsCameraDepthTexture; //是否需要深度贴图
                cameraData.requiresOpaqueTexture = settings.supportsCameraOpaqueTexture; //是否需要颜色贴图（不透明我物体）
                cameraData.renderer = asset.scriptableRenderer; //render对象
            }
            else if (additionalCameraData != null)
            {
                //基本都走这，
                //renderType（base还是overlay）, 是否清除深度，是否开启后效（后面还有判断），最大阴影距离，是否需要深度贴图，是否需要颜色贴图，渲染器对象
                cameraData.renderType = additionalCameraData.renderType; //取的其实是camera组件上的信息 additionalCameraData可以理解为camera组件的副身
                cameraData.clearDepth = (additionalCameraData.renderType != CameraRenderType.Base) ? additionalCameraData.clearDepth : true;
                cameraData.postProcessEnabled = additionalCameraData.renderPostProcessing; //相机上是否开启后效
                cameraData.maxShadowDistance = (additionalCameraData.renderShadows) ? cameraData.maxShadowDistance : 0.0f;
                cameraData.requiresDepthTexture = additionalCameraData.requiresDepthTexture;
                cameraData.requiresOpaqueTexture = additionalCameraData.requiresColorTexture;
                cameraData.renderer = additionalCameraData.scriptableRenderer;
            }
            else
            {
                cameraData.renderType = CameraRenderType.Base;
                cameraData.clearDepth = true;
                cameraData.postProcessEnabled = false;
                cameraData.requiresDepthTexture = settings.supportsCameraDepthTexture;
                cameraData.requiresOpaqueTexture = settings.supportsCameraOpaqueTexture;
                cameraData.renderer = asset.scriptableRenderer;
            }
            
            //再次判断是否需要开启后效，GLes2不开启
            // Disables post if GLes2
            cameraData.postProcessEnabled &= SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES2;
            
            //再次判断是否需要深度图，
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
            
            //左右两边只要有一个为true就为true
            cameraData.requiresDepthTexture |= isSceneViewCamera || CheckPostProcessForDepth(cameraData);
            
            //当前相机是否需要最后绘制到屏幕上，如果当前相机的camerastack里有激活的相机就不需要绘制到屏幕
            cameraData.resolveFinalTarget = resolveFinalTarget;

            // Disable depth and color copy. We should add it in the renderer instead to avoid performance pitfalls
            // of camera stacking breaking render pass execution implicitly.
            bool isOverlayCamera = (cameraData.renderType == CameraRenderType.Overlay);
            if (isOverlayCamera)
            {
                //overlayer相机不需要生成DepthTexture和OpaqueTexture
                cameraData.requiresDepthTexture = false;
                cameraData.requiresOpaqueTexture = false;
            }

            Matrix4x4 projectionMatrix = camera.projectionMatrix; //相机的投影矩阵

            // Overlay cameras inherit viewport from base.
            // If the viewport is different between them we might need to patch the projection to adjust aspect ratio
            // matrix to prevent squishing when rendering objects in overlay cameras.
            if (isOverlayCamera && !camera.orthographic && cameraData.pixelRect != camera.pixelRect)
            {
                // m00 = (cotangent / aspect), therefore m00 * aspect gives us cotangent.
                float cotangent = camera.projectionMatrix.m00 * camera.aspect;

                // Get new m00 by dividing by base camera aspectRatio.
                float newCotangent = cotangent / cameraData.aspectRatio;
                projectionMatrix.m00 = newCotangent;
            }

            //每个相机设置自己的VP矩阵
            cameraData.SetViewAndProjectionMatrix(camera.worldToCameraMatrix, projectionMatrix);
        }

        static void InitializeRenderingData(UniversalRenderPipelineAsset settings, ref CameraData cameraData, ref CullingResults cullResults,
            bool anyPostProcessingEnabled, out RenderingData renderingData)
        {
            using var profScope = new ProfilingScope(null, Profiling.Pipeline.initializeRenderingData);

            var visibleLights = cullResults.visibleLights;

            int mainLightIndex = GetMainLightIndex(settings, visibleLights);
            bool mainLightCastShadows = false;
            bool additionalLightsCastShadows = false;

            if (cameraData.maxShadowDistance > 0.0f)
            {
                mainLightCastShadows = (mainLightIndex != -1 && visibleLights[mainLightIndex].light != null &&
                                        visibleLights[mainLightIndex].light.shadows != LightShadows.None);

                // If additional lights are shaded per-pixel they cannot cast shadows
                if (settings.additionalLightsRenderingMode == LightRenderingMode.PerPixel)
                {
                    for (int i = 0; i < visibleLights.Length; ++i)
                    {
                        if (i == mainLightIndex)
                            continue;

                        Light light = visibleLights[i].light;

                        // UniversalRP doesn't support additional directional lights or point light shadows yet
                        if (visibleLights[i].lightType == LightType.Spot && light != null && light.shadows != LightShadows.None)
                        {
                            additionalLightsCastShadows = true;
                            break;
                        }
                    }
                }
            }

            //填充renderingData的内容  光照 阴影 后效
            renderingData.cullResults = cullResults;
            renderingData.cameraData = cameraData;
            InitializeLightData(settings, visibleLights, mainLightIndex, out renderingData.lightData);
            InitializeShadowData(settings, visibleLights, mainLightCastShadows, additionalLightsCastShadows && !renderingData.lightData.shadeAdditionalLightsPerVertex, out renderingData.shadowData);
            InitializePostProcessingData(settings, out renderingData.postProcessingData);
            renderingData.supportsDynamicBatching = settings.supportsDynamicBatching; //是否支持动态合批
            renderingData.perObjectData = GetPerObjectLightFlags(renderingData.lightData.additionalLightsCount);
            renderingData.postProcessingEnabled = anyPostProcessingEnabled;
        }

        static void InitializeShadowData(UniversalRenderPipelineAsset settings, NativeArray<VisibleLight> visibleLights, bool mainLightCastShadows, bool additionalLightsCastShadows, out ShadowData shadowData)
        {
            using var profScope = new ProfilingScope(null, Profiling.Pipeline.initializeShadowData);

            m_ShadowBiasData.Clear();

            for (int i = 0; i < visibleLights.Length; ++i)
            {
                Light light = visibleLights[i].light;
                UniversalAdditionalLightData data = null;
                if (light != null)
                {
                    light.gameObject.TryGetComponent(out data);
                }

                if (data && !data.usePipelineSettings)
                    m_ShadowBiasData.Add(new Vector4(light.shadowBias, light.shadowNormalBias, 0.0f, 0.0f));
                else
                    m_ShadowBiasData.Add(new Vector4(settings.shadowDepthBias, settings.shadowNormalBias, 0.0f, 0.0f));
            }

            shadowData.bias = m_ShadowBiasData;
            shadowData.supportsMainLightShadows = SystemInfo.supportsShadows && settings.supportsMainLightShadows && mainLightCastShadows;

            // We no longer use screen space shadows in URP.
            // This change allows us to have particles & transparent objects receive shadows.
            shadowData.requiresScreenSpaceShadowResolve = false;

            shadowData.mainLightShadowCascadesCount = settings.shadowCascadeCount;
            shadowData.mainLightShadowmapWidth = settings.mainLightShadowmapResolution;
            shadowData.mainLightShadowmapHeight = settings.mainLightShadowmapResolution;

            switch (shadowData.mainLightShadowCascadesCount)
            {
                case 1:
                    shadowData.mainLightShadowCascadesSplit = new Vector3(1.0f, 0.0f, 0.0f);
                    break;

                case 2:
                    shadowData.mainLightShadowCascadesSplit = new Vector3(settings.cascade2Split, 1.0f, 0.0f);
                    break;

                case 3:
                    shadowData.mainLightShadowCascadesSplit = new Vector3(settings.cascade3Split.x, settings.cascade3Split.y, 0.0f);
                    break;

                default:
                    shadowData.mainLightShadowCascadesSplit = settings.cascade4Split;
                    break;
            }

            shadowData.supportsAdditionalLightShadows = SystemInfo.supportsShadows && settings.supportsAdditionalLightShadows && additionalLightsCastShadows;
            shadowData.additionalLightsShadowmapWidth = shadowData.additionalLightsShadowmapHeight = settings.additionalLightsShadowmapResolution;
            shadowData.supportsSoftShadows = settings.supportsSoftShadows && (shadowData.supportsMainLightShadows || shadowData.supportsAdditionalLightShadows);
            shadowData.shadowmapDepthBufferBits = 16;
        }

        static void InitializePostProcessingData(UniversalRenderPipelineAsset settings, out PostProcessingData postProcessingData)
        {
            postProcessingData.gradingMode = settings.supportsHDR
                ? settings.colorGradingMode
                : ColorGradingMode.LowDynamicRange;

            postProcessingData.lutSize = settings.colorGradingLutSize;
        }

        static void InitializeLightData(UniversalRenderPipelineAsset settings, NativeArray<VisibleLight> visibleLights, int mainLightIndex, out LightData lightData)
        {
            using var profScope = new ProfilingScope(null, Profiling.Pipeline.initializeLightData);

            //UniversalRenderPipeline.maxPerObjectLights属性限定了不同Level每个物体接受灯光数量的上限
            int maxPerObjectAdditionalLights = UniversalRenderPipeline.maxPerObjectLights;
            int maxVisibleAdditionalLights = UniversalRenderPipeline.maxVisibleAdditionalLights;

            lightData.mainLightIndex = mainLightIndex;

            if (settings.additionalLightsRenderingMode != LightRenderingMode.Disabled)
            {
                lightData.additionalLightsCount =
                    Math.Min((mainLightIndex != -1) ? visibleLights.Length - 1 : visibleLights.Length,
                        maxVisibleAdditionalLights);
                lightData.maxPerObjectAdditionalLightsCount = Math.Min(settings.maxAdditionalLightsCount, maxPerObjectAdditionalLights);
            }
            else
            {
                lightData.additionalLightsCount = 0;
                lightData.maxPerObjectAdditionalLightsCount = 0;
            }

            lightData.shadeAdditionalLightsPerVertex = settings.additionalLightsRenderingMode == LightRenderingMode.PerVertex;
            lightData.visibleLights = visibleLights;
            lightData.supportsMixedLighting = settings.supportsMixedLighting;
        }

        static PerObjectData GetPerObjectLightFlags(int additionalLightsCount)
        {
            using var profScope = new ProfilingScope(null, Profiling.Pipeline.getPerObjectLightFlags);
            
            //反射探针，光照贴图，光照探针，ShadowMask，OcclusionProbe
            //逐物体的灯光数据(LightData)
            var configuration = PerObjectData.ReflectionProbes | PerObjectData.Lightmaps | PerObjectData.LightProbe | PerObjectData.LightData | PerObjectData.OcclusionProbe | PerObjectData.ShadowMask;

            if (additionalLightsCount > 0)
            {
                configuration |= PerObjectData.LightData;

                // In this case we also need per-object indices (unity_LightIndices)
                if (!RenderingUtils.useStructuredBuffer)
                    configuration |= PerObjectData.LightIndices;
            }

            return configuration;
        }

        // Main Light is always a directional light
        static int GetMainLightIndex(UniversalRenderPipelineAsset settings, NativeArray<VisibleLight> visibleLights)
        {
            using var profScope = new ProfilingScope(null, Profiling.Pipeline.getMainLightIndex);

            int totalVisibleLights = visibleLights.Length;

            if (totalVisibleLights == 0 || settings.mainLightRenderingMode != LightRenderingMode.PerPixel)
                return -1;

            Light sunLight = RenderSettings.sun;
            int brightestDirectionalLightIndex = -1;
            float brightestLightIntensity = 0.0f;
            for (int i = 0; i < totalVisibleLights; ++i)
            {
                VisibleLight currVisibleLight = visibleLights[i];
                Light currLight = currVisibleLight.light;

                // Particle system lights have the light property as null. We sort lights so all particles lights
                // come last. Therefore, if first light is particle light then all lights are particle lights.
                // In this case we either have no main light or already found it.
                if (currLight == null)
                    break;

                if (currVisibleLight.lightType == LightType.Directional)
                {
                    // Sun source needs be a directional light
                    if (currLight == sunLight)
                        return i;

                    // In case no sun light is present we will return the brightest directional light
                    if (currLight.intensity > brightestLightIntensity)
                    {
                        brightestLightIntensity = currLight.intensity;
                        brightestDirectionalLightIndex = i;
                    }
                }
            }

            return brightestDirectionalLightIndex;
        }

        static void SetupPerFrameShaderConstants()
        {
            //设置了未开启环境反射时的默认颜色、阴影颜色

            using var profScope = new ProfilingScope(null, Profiling.Pipeline.setupPerFrameShaderConstants);
            
            //当高光反射被关闭的时候，使用一个颜色值来代替间接高光反射率值 specular
            //_GlossyEnvironmentColor
            // When glossy reflections are OFF in the shader we set a constant color to use as indirect specular
            SphericalHarmonicsL2 ambientSH = RenderSettings.ambientProbe;
            Color linearGlossyEnvColor = new Color(ambientSH[0, 0], ambientSH[1, 0], ambientSH[2, 0]) * RenderSettings.reflectionIntensity;
            Color glossyEnvColor = CoreUtils.ConvertLinearToActiveColorSpace(linearGlossyEnvColor);
            Shader.SetGlobalVector(ShaderPropertyId.glossyEnvironmentColor, glossyEnvColor);

            // Ambient 环境光
            Shader.SetGlobalVector(ShaderPropertyId.ambientSkyColor, CoreUtils.ConvertSRGBToActiveColorSpace(RenderSettings.ambientSkyColor));
            Shader.SetGlobalVector(ShaderPropertyId.ambientEquatorColor, CoreUtils.ConvertSRGBToActiveColorSpace(RenderSettings.ambientEquatorColor));
            Shader.SetGlobalVector(ShaderPropertyId.ambientGroundColor, CoreUtils.ConvertSRGBToActiveColorSpace(RenderSettings.ambientGroundColor));

            // Used when subtractive mode is selected 
            //灯光的lightmode为Mixed，在lighting面板里设置subtractive模式
            Shader.SetGlobalVector(ShaderPropertyId.subtractiveShadowColor, CoreUtils.ConvertSRGBToActiveColorSpace(RenderSettings.subtractiveShadowColor));

            // Required for 2D Unlit Shadergraph master node as it doesn't currently support hidden properties.
            Shader.SetGlobalColor(ShaderPropertyId.rendererColor, Color.white);
        }

#if ADAPTIVE_PERFORMANCE_2_0_0_OR_NEWER
        static void ApplyAdaptivePerformance(ref CameraData cameraData)
        {
            var noFrontToBackOpaqueFlags = SortingCriteria.SortingLayer | SortingCriteria.RenderQueue | SortingCriteria.OptimizeStateChanges | SortingCriteria.CanvasOrder;
            if (AdaptivePerformance.AdaptivePerformanceRenderSettings.SkipFrontToBackSorting)
                cameraData.defaultOpaqueSortFlags = noFrontToBackOpaqueFlags;

            var MaxShadowDistanceMultiplier = AdaptivePerformance.AdaptivePerformanceRenderSettings.MaxShadowDistanceMultiplier;
            cameraData.maxShadowDistance *= MaxShadowDistanceMultiplier;

            var RenderScaleMultiplier = AdaptivePerformance.AdaptivePerformanceRenderSettings.RenderScaleMultiplier;
            cameraData.renderScale *= RenderScaleMultiplier;

            // TODO
            if (!cameraData.xr.enabled)
            {
                cameraData.cameraTargetDescriptor.width = (int)(cameraData.camera.pixelWidth * cameraData.renderScale);
                cameraData.cameraTargetDescriptor.height = (int)(cameraData.camera.pixelHeight * cameraData.renderScale);
            }

            var antialiasingQualityIndex = (int)cameraData.antialiasingQuality - AdaptivePerformance.AdaptivePerformanceRenderSettings.AntiAliasingQualityBias;
            if (antialiasingQualityIndex < 0)
                cameraData.antialiasing = AntialiasingMode.None;
            cameraData.antialiasingQuality = (AntialiasingQuality)Mathf.Clamp(antialiasingQualityIndex, (int)AntialiasingQuality.Low, (int)AntialiasingQuality.High);
        }
        static void ApplyAdaptivePerformance(ref RenderingData renderingData)
        {
            if (AdaptivePerformance.AdaptivePerformanceRenderSettings.SkipDynamicBatching)
                renderingData.supportsDynamicBatching = false;

            var MainLightShadowmapResolutionMultiplier = AdaptivePerformance.AdaptivePerformanceRenderSettings.MainLightShadowmapResolutionMultiplier;
            renderingData.shadowData.mainLightShadowmapWidth = (int)(renderingData.shadowData.mainLightShadowmapWidth * MainLightShadowmapResolutionMultiplier);
            renderingData.shadowData.mainLightShadowmapHeight = (int)(renderingData.shadowData.mainLightShadowmapHeight * MainLightShadowmapResolutionMultiplier);

            var MainLightShadowCascadesCountBias = AdaptivePerformance.AdaptivePerformanceRenderSettings.MainLightShadowCascadesCountBias;
            renderingData.shadowData.mainLightShadowCascadesCount = Mathf.Clamp(renderingData.shadowData.mainLightShadowCascadesCount - MainLightShadowCascadesCountBias, 0, 4);

            var shadowQualityIndex = AdaptivePerformance.AdaptivePerformanceRenderSettings.ShadowQualityBias;
            for (int i = 0; i < shadowQualityIndex; i++)
            {
                if (renderingData.shadowData.supportsSoftShadows)
                {
                    renderingData.shadowData.supportsSoftShadows = false;
                    continue;
                }

                if (renderingData.shadowData.supportsAdditionalLightShadows)
                {
                    renderingData.shadowData.supportsAdditionalLightShadows = false;
                    continue;
                }

                if (renderingData.shadowData.supportsMainLightShadows)
                {
                    renderingData.shadowData.supportsMainLightShadows = false;
                    continue;
                }

                break;
            }

            if (AdaptivePerformance.AdaptivePerformanceRenderSettings.LutBias >= 1 && renderingData.postProcessingData.lutSize == 32)
                renderingData.postProcessingData.lutSize = 16;
        }
#endif
    }
}
