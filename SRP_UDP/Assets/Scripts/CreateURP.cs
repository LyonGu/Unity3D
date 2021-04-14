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
        //��Ⱦ�����ļ�������

        //��ȡ��͸������������ͼ
        Shader.GetGlobalTexture("_CameraDepthTexture");

        UniversalRenderPipelineAsset pipelineAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

        //��̬�������ͼ
        pipelineAsset.supportsCameraDepthTexture = true;

        //��̬������͸��ͼ
        pipelineAsset.supportsCameraOpaqueTexture = true;

        //��̬����HDR
        pipelineAsset.supportsHDR = true;

        /*
            ��̬����4�������

            Disabled = 1,
            _2x = 2,
            _4x = 4,
            _8x = 8
         */
        pipelineAsset.msaaSampleCount = 4;

        //��̬���÷ֱ���
        pipelineAsset.renderScale = 0.5f;

        //��Ӱ������
        pipelineAsset.shadowDistance = 50f;

        //��Ӱ��������
        pipelineAsset.shadowCascadeCount = 4;

        //��Ӱ���ƫ��
        pipelineAsset.shadowDepthBias = 2.5f;

        //��Ӱ����ƫ��
        pipelineAsset.shadowNormalBias = 2.5f;


        //����ColorGrading��ģʽ��LUT��ͼ��С
        pipelineAsset.colorGradingMode = ColorGradingMode.LowDynamicRange;
        pipelineAsset.colorGradingLutSize = 32;

        //����SRPBatcher����
        pipelineAsset.useSRPBatcher = true;

        //��̬�����Ƿ�����
        pipelineAsset.supportsDynamicBatching = false;


        //�ߵ������ò�ͬ����Ⱦ�����ļ�
        if(hight!=null)
            GraphicsSettings.renderPipelineAsset = hight;

        //��������԰󶨶����Ⱦ����������Ⱦ�����ļ� renderlist�е��ļ����������ﶯ̬�л�ÿ����Ⱦ�� ����index��������
        Camera.main.GetComponent<UniversalAdditionalCameraData>().SetRenderer(0);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
