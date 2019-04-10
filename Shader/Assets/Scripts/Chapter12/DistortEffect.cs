using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistortEffect : PostEffectsBase
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


    //OnRenderImage绘制绘制完所有透明和不透明的物体后每一帧调用
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (material != null)
        {
            //设置shader属性值
            material.SetTexture("_NoiseTex", NoiseTexture);
            material.SetFloat("_DistortTimeFactor", DistortTimeFactor);
            material.SetFloat("_DistortStrength", DistortStrength);


            //把屏幕纹理src传进shader里_MainTex属性
            Graphics.Blit(src, dest, material);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }

}
