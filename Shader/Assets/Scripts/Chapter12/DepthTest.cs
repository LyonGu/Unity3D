using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthTest : PostEffectsBase {

	public Shader depthShader;
    private Material depthMaterial;


    public Material material
    {
        get {
            depthMaterial = CheckShaderAndCreateMaterial(depthShader, depthMaterial);
            return depthMaterial;
        }
    }

	public void onEnable()
	{
		//开启深度纹理模式，可以从shader里获取深度纹理贴图
		GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
	}

	// Use this for initialization
	void Start () 
	{
		GetComponent<Camera>().depthTextureMode &= ~DepthTextureMode.Depth;
	}
	
	//OnRenderImage绘制绘制完所有透明和不透明的物体后每一帧调用
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (material != null)
        {
            //设置shader属性值
            //material.SetFloat("_Brightness", brightness);
            //material.SetFloat("_Saturation", saturation);
            //material.SetFloat("_Contrast", contrast);

            //把屏幕纹理src传进shader里_MainTex属性
            Graphics.Blit(src, dest, material);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }
	
}
