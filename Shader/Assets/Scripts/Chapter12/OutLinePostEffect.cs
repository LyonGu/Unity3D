using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutLinePostEffect : PostEffectsBase
{
    public Shader outLinePreShader;
    public Shader outLineShader;
    private Material outLineaterial;


    public Material material
    {
        get {
            outLineaterial = CheckShaderAndCreateMaterial(outLineShader, outLineaterial);
            return outLineaterial;
        }
    }

    // Blur iterations - larger number means more blur.
    [Range(0, 4)]
    public int iterations = 3;

    // Blur spread for each iteration - larger value means more blur
    [Range(0.2f, 3.0f)]
    public float blurSpread = 0.6f;

    [Range(1, 8)]
    public int downSample = 2;


    //
    [Range(0.0f, 1.0f)]
    public float _Outline = 0.1f;

    public Color _OutlineColor = new Color(1, 0, 0, 1);


    private Camera mainCam = null;
    private Camera additionalCam = null;
    private RenderTexture renderTexture = null;


    //OnRenderImage绘制绘制完所有透明和不透明的物体后每一帧调用

    /// 1st edition: just apply blur
    //	void OnRenderImage(RenderTexture src, RenderTexture dest) {
    //		if (material != null) {
    //			int rtW = src.width;
    //			int rtH = src.height;
    //			RenderTexture buffer = RenderTexture.GetTemporary(rtW, rtH, 0);
    //
    //			// Render the vertical pass
    //			Graphics.Blit(src, buffer, material, 0);
    //			// Render the horizontal pass
    //			Graphics.Blit(buffer, dest, material, 1);
    //
    //			RenderTexture.ReleaseTemporary(buffer);
    //		} else {
    //			Graphics.Blit(src, dest);
    //		}
    //	} 

    /// 2nd edition: scale the render texture
    //	void OnRenderImage (RenderTexture src, RenderTexture dest) {
    //		if (material != null) {
    //			int rtW = src.width/downSample;
    //			int rtH = src.height/downSample;
    //			RenderTexture buffer = RenderTexture.GetTemporary(rtW, rtH, 0);
    //			buffer.filterMode = FilterMode.Bilinear;
    //
    //			// Render the vertical pass
    //			Graphics.Blit(src, buffer, material, 0);
    //			// Render the horizontal pass
    //			Graphics.Blit(buffer, dest, material, 1);
    //
    //			RenderTexture.ReleaseTemporary(buffer);
    //		} else {
    //			Graphics.Blit(src, dest);
    //		}
    //	}

    /// 3rd edition: use iterations for larger blur
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (material != null)
        {
           
            int rtW = src.width / downSample;
            int rtH = src.height / downSample;

            //分配一块缓冲区
            RenderTexture buffer0 = RenderTexture.GetTemporary(rtW, rtH, 0);
            buffer0.filterMode = FilterMode.Bilinear;

            //直接拷贝进去 纯色图拷贝进去，接下来进行高斯模糊
            Graphics.Blit(renderTexture, buffer0);

            for (int i = 0; i < iterations; i++)
            {
                material.SetFloat("_BlurSize", 1.0f + i * blurSpread);

                RenderTexture buffer1 = RenderTexture.GetTemporary(rtW, rtH, 0);

                // Render the vertical pass，使用第二个pass（序号为1）处理buffer0的数据，然后把数据传到buffer1缓冲区中
                Graphics.Blit(buffer0, buffer1, material, 0);

                //释放buffer0缓冲区数据，操作完之后一定要解绑，跟opengl的vao和vbo一样
                RenderTexture.ReleaseTemporary(buffer0);
                buffer0 = buffer1;

                //再次分配一块缓冲区
                buffer1 = RenderTexture.GetTemporary(rtW, rtH, 0);

                // Render the horizontal pass使用第三个pass（序号为2）处理buffer0的数据，然后把数据传到buffer1缓冲区中
                //这里的buffer0就是上一个pass处理过的缓冲区数据了
                Graphics.Blit(buffer0, buffer1, material, 1);

                RenderTexture.ReleaseTemporary(buffer0);
                buffer0 = buffer1;
            }

            ////高斯模糊后的纹理数据设置到shader里
            material.SetTexture("_BlurTex", buffer0);

            RenderTexture buffer_lunkuo = RenderTexture.GetTemporary(rtW, rtH, 0);
           
            //用原始图片减去模糊图片得到轮廓数据
            Graphics.Blit(renderTexture, buffer_lunkuo, material, 2); 
            RenderTexture.ReleaseTemporary(buffer0);

            Graphics.Blit(buffer_lunkuo, dest);

            ////轮廓的纹理数据设置到shader里
            material.SetTexture("_BlurTex", buffer_lunkuo);
            Graphics.Blit(src, dest, material, 3);
            RenderTexture.ReleaseTemporary(buffer_lunkuo);
      

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
        if (mainCam == null)
            return;
 
        Transform addCamTransform = transform.Find("outLineCam");
        if (addCamTransform != null)
            DestroyImmediate(addCamTransform.gameObject);
 
        GameObject additionalCamObj = new GameObject("outLineCam");
        additionalCam = additionalCamObj.AddComponent<Camera>();
 
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
            additionalCam.cullingMask = 1 << LayerMask.NameToLayer("outLine");
            additionalCam.depth = -999; 
            if (renderTexture == null)
                renderTexture = RenderTexture.GetTemporary(additionalCam.pixelWidth >> downSample, additionalCam.pixelHeight >> downSample, 0);
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
            RenderTexture.ReleaseTemporary(renderTexture);
        }
        DestroyImmediate(additionalCam.gameObject);
    }


    //unity提供的在渲染之前的接口，在这一步渲染描边到RT
    void OnPreRender()
    {
        //使用OutlinePrepass进行渲染，得到RT
        if(additionalCam.enabled)
        {
            //渲染到RT上
            //首先检查是否需要重设RT，比如屏幕分辨率变化了
            if (renderTexture != null && (renderTexture.width != Screen.width >> downSample || renderTexture.height != Screen.height >> downSample))
            {
                RenderTexture.ReleaseTemporary(renderTexture);
                renderTexture = RenderTexture.GetTemporary(Screen.width >> downSample, Screen.height >> downSample, 0);
            }
            additionalCam.targetTexture = renderTexture;

            //设置全局颜色
            Shader.SetGlobalColor("_OutlinePrePassLineColor", _OutlineColor);
            Shader.SetGlobalFloat("_OutlinePrePassOutline",_Outline);
            additionalCam.RenderWithShader(outLinePreShader, "");
        }
    }

}
