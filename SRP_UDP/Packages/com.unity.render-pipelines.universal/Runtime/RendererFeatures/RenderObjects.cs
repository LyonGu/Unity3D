using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.Experimental.Rendering.Universal
{
    [MovedFrom("UnityEngine.Experimental.Rendering.LWRP")]public enum RenderQueueType
    {
        Opaque,
        Transparent,
    }

    [ExcludeFromPreset]
    [MovedFrom("UnityEngine.Experimental.Rendering.LWRP")]public class RenderObjects : ScriptableRendererFeature
    {
        [System.Serializable]
        public class RenderObjectsSettings
        {
            public string passTag = "RenderObjectsFeature";
            public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;

            public FilterSettings filterSettings = new FilterSettings();

            public Material overrideMaterial = null;
            public int overrideMaterialPassIndex = 0;

            public bool overrideDepthState = false;
            public CompareFunction depthCompareFunction = CompareFunction.LessEqual;
            public bool enableWrite = true;

            public StencilStateData stencilSettings = new StencilStateData();

            public CustomCameraSettings cameraSettings = new CustomCameraSettings();
        }

        [System.Serializable]
        public class FilterSettings
        {
            // TODO: expose opaque, transparent, all ranges as drop down
            public RenderQueueType RenderQueueType;
            public LayerMask LayerMask;
            public string[] PassNames;

            public FilterSettings()
            {
                RenderQueueType = RenderQueueType.Opaque;
                LayerMask = 0;
            }
        }

        [System.Serializable]
        public class CustomCameraSettings
        {
            public bool overrideCamera = false;
            public bool restoreCamera = true;
            public Vector4 offset;
            public float cameraFieldOfView = 60.0f;
        }

        public RenderObjectsSettings settings = new RenderObjectsSettings();

        RenderObjectsPass renderObjectsPass;

        public override void Create()
        {
            //settings为序列化对象，面板上的配置
            FilterSettings filter = settings.filterSettings;

            // Render Objects pass doesn't support events before rendering prepasses.
            // The camera is not setup before this point and all rendering is monoscopic.
            // Events before BeforeRenderingPrepasses should be used for input texture passes (shadow map, LUT, etc) that doesn't depend on the camera.
            // These events are filtering in the UI, but we still should prevent users from changing it from code or
            // by changing the serialized data.
            //限制最小事件
            if (settings.Event < RenderPassEvent.BeforeRenderingPrepasses)
                settings.Event = RenderPassEvent.BeforeRenderingPrepasses;
            
            //创建对应的Pass
            //filter.RenderQueueType 代表只渲染哪个队列，目前只有透明和不透明两种选择
            //filter.LayerMask 过滤层
            //filter.PassNames 绘制时使用的的pass，这里传入的是shader里"LightMode"对应的名字 Tags {"LightMode" = "xxxx"}
            //settings.cameraSettings 代表相机设置信息：
            renderObjectsPass = new RenderObjectsPass(settings.passTag, settings.Event, filter.PassNames,
                filter.RenderQueueType, filter.LayerMask, settings.cameraSettings);
            
            //是否重载材质信息
            renderObjectsPass.overrideMaterial = settings.overrideMaterial;
            renderObjectsPass.overrideMaterialPassIndex = settings.overrideMaterialPassIndex;
            
            //是否重载深度信息
            if (settings.overrideDepthState)
                renderObjectsPass.SetDetphState(settings.enableWrite, settings.depthCompareFunction);
            
            //是否重载模板信息
            if (settings.stencilSettings.overrideStencilState)
                renderObjectsPass.SetStencilState(settings.stencilSettings.stencilReference,
                    settings.stencilSettings.stencilCompareFunction, settings.stencilSettings.passOperation,
                    settings.stencilSettings.failOperation, settings.stencilSettings.zFailOperation);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            //把pass加入到渲染队列里
            renderer.EnqueuePass(renderObjectsPass);
        }
    }
}

