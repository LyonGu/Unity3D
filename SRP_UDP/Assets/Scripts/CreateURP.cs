using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CreateURP : MonoBehaviour
{
    private UniversalRenderPipelineAsset hight;
    private UniversalRenderPipelineAsset low;

    // Start is called before the first frame update
    void Start()
    {
        //渲染配置文件的设置

        //获取不透明物体的深度贴图
        Shader.GetGlobalTexture("_CameraDepthTexture");

        UniversalRenderPipelineAsset pipelineAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

        //动态启动深度图
        pipelineAsset.supportsCameraDepthTexture = true;

        //动态启动不透明图
        pipelineAsset.supportsCameraOpaqueTexture = true;

        //动态启动HDR
        pipelineAsset.supportsHDR = true;

        /*
            动态启动4倍抗锯齿

            Disabled = 1,
            _2x = 2,
            _4x = 4,
            _8x = 8
         */
        pipelineAsset.msaaSampleCount = 4;

        //动态设置分辨率
        pipelineAsset.renderScale = 0.5f;

        //阴影最大距离
        pipelineAsset.shadowDistance = 50f;

        //阴影联级数量
        pipelineAsset.shadowCascadeCount = 4;

        //阴影深度偏移
        pipelineAsset.shadowDepthBias = 2.5f;

        //阴影法线偏移
        pipelineAsset.shadowNormalBias = 2.5f;


        //设置ColorGrading的模式和LUT贴图大小
        pipelineAsset.colorGradingMode = ColorGradingMode.LowDynamicRange;
        pipelineAsset.colorGradingLutSize = 32;

        //设置SRPBatcher启动
        pipelineAsset.useSRPBatcher = true;

        //动态合批是否启动
        pipelineAsset.supportsDynamicBatching = false;


        //高低配配置不同的渲染配置文件
        if(hight!=null)
            GraphicsSettings.renderPipelineAsset = hight;

        //摄像机可以绑定多个渲染器（就是渲染配置文件 renderlist中的文件），代码里动态切换每个渲染器 设置index索引即可
        Camera.main.GetComponent<UniversalAdditionalCameraData>().SetRenderer(0);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
