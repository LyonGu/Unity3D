
/***
 *
 *  Title: "Guardian" 项目
 *         描述：
 *
 *  Description:
 *        功能：
 *       
 *
 *  Date: 2019
 * 
 *  Version: 1.0
 *
 *  Modify Recorder:
 *     
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
public class CommandBufferOutLinePostEffect : PostEffectsBaseNoCamera
{
    //描边prepass shader（渲染纯色贴图的shader）
    public Shader outLinePreShader;

    public Shader outLineShader;
    private RenderTexture renderTexture = null;
    private CommandBuffer commandBuffer = null;
    private Material outlineMaterial = null;
    private Material outlinePreMaterial = null;

    public Material material
    {
        get
        {
            outlineMaterial = CheckShaderAndCreateMaterial(outLineShader, outlineMaterial);
            return outlineMaterial;
        }
    }

  
    //降采样
    [Range(1, 8)]
    public int downSample = 1;
    //迭代次数
    [Range(0, 4)]
    public int iteration = 2;

    // Blur spread for each iteration - larger value means more blur
    [Range(0.2f, 3.0f)]
    public float blurSpread = 0.6f;

    //描边颜色
    public Color outLineColor = Color.green;
    //描边强度
    [Range(0.0f, 10.0f)]
    public float outLineStrength = 3.0f;
    //目标对象
    public GameObject targetObject = null;

    //是否开启柔和效果
    public bool _isSoft = true;

    void OnEnable()
    {
        if (outLinePreShader == null)
            return;
        if (outlinePreMaterial == null)
            outlinePreMaterial = new Material(outLinePreShader);
        Renderer[] renderers = targetObject.GetComponentsInChildren<Renderer>();
        if (renderTexture == null)
            renderTexture = RenderTexture.GetTemporary(Screen.width >> downSample, Screen.height >> downSample, 0);
        //创建描边prepass的command buffer
        commandBuffer = new CommandBuffer();
        commandBuffer.SetRenderTarget(renderTexture);
        commandBuffer.ClearRenderTarget(true, true, Color.black);
        foreach (Renderer r in renderers)
            commandBuffer.DrawRenderer(r, outlinePreMaterial);
    }

    void OnDisable()
    {
        if (renderTexture)
        {
            RenderTexture.ReleaseTemporary(renderTexture);
            renderTexture = null;
        }
        if (outlineMaterial)
        {
            DestroyImmediate(outlineMaterial);
            outlineMaterial = null;
        }

        if (outlinePreMaterial)
        {
            DestroyImmediate(outlinePreMaterial);
            outlinePreMaterial = null;
        }
        if (commandBuffer != null)
        {
            commandBuffer.Release();
            commandBuffer = null;
        }

    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (material && renderTexture && outlineMaterial && commandBuffer != null)
        {
            //通过Command Buffer可以设置自定义材质的颜色
            outlinePreMaterial.SetColor("_OutlineCol", outLineColor);
            outlinePreMaterial.SetFloat("_OutLineStrength", outLineStrength);
            //直接通过Graphic执行Command Buffer
            Graphics.ExecuteCommandBuffer(commandBuffer);

            int rtW = src.width / downSample;
            int rtH = src.height / downSample;

            //分配一块缓冲区
            RenderTexture buffer0 = RenderTexture.GetTemporary(rtW, rtH, 0);
            buffer0.filterMode = FilterMode.Bilinear;

            //直接拷贝进去 纯色图拷贝进去，接下来进行高斯模糊
            Graphics.Blit(renderTexture, buffer0);

            for (int i = 0; i < iteration; i++)
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


            ////轮廓的纹理数据设置到shader里
            material.SetTexture("_BlurTex", buffer_lunkuo);

            material.SetInt("_soft", System.Convert.ToInt32(_isSoft));
            //if (!_isSoft)
            //{
            //    outlinePreMaterial.SetColor("_OutlineCol", outLineColor);
            //}

            Graphics.Blit(src, dest, material, 3);
            RenderTexture.ReleaseTemporary(buffer_lunkuo);

        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }
}
