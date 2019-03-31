using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldFilmEffect1 : PostEffectsBase{

    public Shader oldFilmShader;
    private Material oldFilmMaterial;


    public Material material
    {
        get {
            oldFilmMaterial = CheckShaderAndCreateMaterial(oldFilmShader, oldFilmMaterial);
            return oldFilmMaterial;
        }
    }

    #region Variables
   
    [Range(0.0f, 1.0f)]
    public float oldFilmEffectAmount = 1.0f;
 
    public Color sepiaColor = Color.white;

    //眩晕纹理
    public Texture2D vignetteTexture;

    [Range(0.0f, 1.0f)]
    public float vignetteAmount = 1.0f;
 
    //刮痕纹理
    public Texture2D scratchesTexture;

    [Range(0.0f, 1.0f)]
    public float scratchesXSpeed = 0.1f;

    [Range(0.0f, 200.0f)]
    public float scratchesYSpeed = 160.0f;
    
    //灰尘纹理
    public Texture2D dustTexture;

    [Range(0.0f, 1000.0f)]
    public float dustXSpeed = 650.0f;

    [Range(0.0f, 1000.0f)]
    public float dustYSpeed = 350.0f;
    
    [Range(-1.0f, 1.0f)]
    public float randomValue = 0.5f;
    #endregion

    

    //OnRenderImage绘制绘制完所有透明和不透明的物体后每一帧调用
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (material != null)
        {
            //设置shader属性值
            material.SetColor("_SepiaColor", sepiaColor);
            material.SetFloat("_VignetteAmount", vignetteAmount);
            material.SetFloat("_EffectAmount", oldFilmEffectAmount);
            
            //眩晕图：两边有黑圈
            if (vignetteTexture) {
                material.SetTexture("_VignetteTex", vignetteTexture);
            }
            
            //刮痕
            if (scratchesTexture) {
                material.SetTexture("_ScratchesTex", scratchesTexture);
                material.SetFloat("_ScratchesXSpeed", scratchesXSpeed);
                material.SetFloat("_ScratchesYSpeed", scratchesYSpeed);
            }
            
            //灰尘
            if (dustTexture) {
                material.SetTexture("_DustTex", dustTexture);
                material.SetFloat("_DustXSpeed", dustXSpeed);
                material.SetFloat("_DustYSpeed", dustYSpeed);
                material.SetFloat("_RandomValue", randomValue);

            }

            //把屏幕纹理src传进shader里_MainTex属性
            Graphics.Blit(src, dest, material);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }

}
