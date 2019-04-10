using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/*
 
    通过生成一个额外的摄像机 利用mask标识
 * 
 * 思路：
 *  1 创建一个与主摄像机一样的摄像机 addtionCam
 *  2 新建一个层distort
 *  3 让摄像机addtionCam的mask的标识为只显示distort层
 *  4 新建一个面片（扭动区域），设置layer层为distort
 *  5 在OnPreRender函数中用RenderWithShader，将面片渲染到一张RT上renderTexture
 *  6 在OnRenderImage中把renderTexture设置为遮罩图，相当于只有遮罩区域为1会显示扭曲效果，其他地方都是0
 
 */

public class DistortEffectByCamera : PostEffectsBase
{

    public Shader distortShader;
    private Material distortMaterial;


    public Material material
    {
        get {
            distortMaterial = CheckShaderAndCreateMaterial(distortShader, distortMaterial);
            return distortMaterial;
        }
    }

    //扭曲的时间系数
    [Range(0.0f, 1.0f)]
    public float DistortTimeFactor = 0.15f;
    //扭曲的强度
    [Range(0.0f, 0.2f)]
    public float DistortStrength = 0.01f;
    //噪声图
    public Texture NoiseTexture = null;
    //渲染Mask图所用的shader
    public Shader maskObjShader = null;
    //降采样系数
    public int downSample = 4;
 
    private Camera mainCam = null;
    private Camera additionalCam = null;
    private RenderTexture renderTexture = null;


    //OnRenderImage绘制绘制完所有透明和不透明的物体后每一帧调用
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (material != null)
        {
            //设置shader属性值
            material.SetTexture("_NoiseTex", NoiseTexture);
            material.SetFloat("_DistortTimeFactor", DistortTimeFactor);
            material.SetFloat("_DistortStrength", DistortStrength);
            material.SetTexture("_MaskTex", renderTexture);

            //把屏幕纹理src传进shader里_MainTex属性
            Graphics.Blit(src, dest, material);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }


    void Awake()
    {
        //创建一个和当前相机一致的相机
        InitAdditionalCam();
    }

    private void InitAdditionalCam()
    {
        mainCam = GetComponent<Camera>();
        if (mainCam == null) return;

        Transform addCamTransform = transform.Find("additionalDistortCam");
        if (addCamTransform != null)
        {
            DestroyImmediate(addCamTransform.gameObject);
        }

        //创建一个对象，然后给这个对象上加一个camera组件
        GameObject additionalCamObj = new GameObject("additionalDistortCam");
        additionalCam = additionalCamObj.AddComponent<Camera>();

        //设置额外摄像机参数
        SetAdditionalCam();

    }

    private void SetAdditionalCam()
    {
        if (additionalCam)
        {
            additionalCam.transform.parent = mainCam.transform;
            additionalCam.transform.localPosition = Vector3.zero;
            additionalCam.transform.localRotation = Quaternion.identity;
            additionalCam.transform.localScale = Vector3.one;
            additionalCam.farClipPlane = mainCam.farClipPlane;
            additionalCam.nearClipPlane = mainCam.nearClipPlane;
            additionalCam.fieldOfView = mainCam.fieldOfView;
            additionalCam.backgroundColor = Color.clear;
            additionalCam.clearFlags = CameraClearFlags.Color;
            additionalCam.cullingMask = 1 << LayerMask.NameToLayer("Distort"); //只渲染Distrot这一层
            additionalCam.depth = -999; // depth越小 越先渲染
            //分辨率可以低一些
            if (renderTexture == null)
                renderTexture = RenderTexture.GetTemporary(Screen.width >> downSample, Screen.height >> downSample, 0);
        }
    }


    void OnEnable()
    {
        SetAdditionalCam();
        additionalCam.enabled = true;
    }

    void OnDisable()
    {
        additionalCam.enabled = false;
    }

    void OnDestroy()
    {
        if (renderTexture)
        {
            //释放renderTexture
            RenderTexture.ReleaseTemporary(renderTexture);
        }
        DestroyImmediate(additionalCam.gameObject);
    }

    //在真正渲染前的回调，此处渲染Mask遮罩图
    void OnPreRender()
    {
        //maskObjShader进行渲染
        //在OnPreRender函数中用RenderWithShader，将面片渲染到一张RT上（这个RT可以多降低一些分辨率），渲染的shader就用一个纯白色的shader就可以了。比如下面的这个Shader:
        if (additionalCam.enabled)
        {
            additionalCam.targetTexture = renderTexture;

            //RenderWithShader 替换sheder，第二个参数为"",表示替换所有additionalCam看见物体的渲染shader
            additionalCam.RenderWithShader(maskObjShader, "");
        }
    }

}
