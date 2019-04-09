using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutLinePostEffect : PostEffectsBase
{

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

    [Range(0.0f, 4.0f)]
    public float luminanceThreshold = 0.6f;

    //
    [Range(0.0f, 1.0f)]
    public float _Outline = 0.1f;

    public Color _OutlineColor = new Color(1, 0, 0, 1);


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

            //把src图像数据缩放后存到buffer0中，使用第一个Pass
            Graphics.Blit(src, buffer0, material, 0);

            for (int i = 0; i < iterations; i++)
            {
                material.SetFloat("_BlurSize", 1.0f + i * blurSpread);

                RenderTexture buffer1 = RenderTexture.GetTemporary(rtW, rtH, 0);

                // Render the vertical pass，使用第二个pass（序号为1）处理buffer0的数据，然后把数据传到buffer1缓冲区中
                Graphics.Blit(buffer0, buffer1, material, 1);

                //释放buffer0缓冲区数据，操作完之后一定要解绑，跟opengl的vao和vbo一样
                RenderTexture.ReleaseTemporary(buffer0);
                buffer0 = buffer1;

                //再次分配一块缓冲区
                buffer1 = RenderTexture.GetTemporary(rtW, rtH, 0);

                // Render the horizontal pass使用第三个pass（序号为2）处理buffer0的数据，然后把数据传到buffer1缓冲区中
                //这里的buffer0就是上一个pass处理过的缓冲区数据了
                Graphics.Blit(buffer0, buffer1, material, 2);

                RenderTexture.ReleaseTemporary(buffer0);
                buffer0 = buffer1;
            }

            //把提取亮度后 进行高斯模糊后的数据传到 shader里的_blurTex纹理
            material.SetTexture("_blurTex", buffer0);

            RenderTexture buffer_lunkuo = RenderTexture.GetTemporary(rtW, rtH, 0);
           
            //用原始图片减去模糊图片得到轮廓数据
            Graphics.Blit(src, buffer_lunkuo, material, 3); 
            RenderTexture.ReleaseTemporary(buffer0);


            material.SetTexture("_blurTex", buffer_lunkuo);
            Graphics.Blit(src, dest, material, 4);  
            RenderTexture.ReleaseTemporary(buffer_lunkuo);

        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }

}
