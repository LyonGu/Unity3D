
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
/*
 后处理，不过我们后处理的对象改了一下，不是基于屏幕，而是基于Command Buffer输出的那张Render Texture
 
 */
public class CommandBufferRT : PostEffectsBaseNoCamera
{

    public Shader briSatConShader;
    private Material briSatConMaterial;


    public Material material
    {
        get
        {
            briSatConMaterial = CheckShaderAndCreateMaterial(briSatConShader, briSatConMaterial);
            return briSatConMaterial;
        }
    }


    private CommandBuffer commandBuffer = null;
    private RenderTexture rendertexture = null;
    private Renderer targetRenderer = null;

    public GameObject targetObject;
    public Material replaceMaterial;

    [Range(0.0f, 3.0f)]
    public float brightness = 1.0f;

    [Range(0.0f, 3.0f)]
    public float saturation = 1.0f;

    [Range(0.0f, 3.0f)]
    public float contrast = 1.0f;

    void OnEnable()
    {
        targetRenderer = targetObject.GetComponentInChildren<Renderer>();

        //申请RT
        rendertexture = RenderTexture.GetTemporary(512, 512, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 4);
        commandBuffer = new CommandBuffer();

        //设置Command Buffer渲染目标为申请的RT
        commandBuffer.SetRenderTarget(rendertexture);

        //初始颜色设置为灰色
        commandBuffer.ClearRenderTarget(true, true, Color.gray);

        //绘制目标对象，如果没有替换材质，就用自己的材质
        Material mat = replaceMaterial == null ? targetRenderer.sharedMaterial : replaceMaterial;
        commandBuffer.DrawRenderer(targetRenderer, mat);

        this.GetComponent<Renderer>().sharedMaterial.mainTexture = rendertexture;

        if (material)
        {
            //这是个比较危险的写法，一张RT即作为输入又作为输出，在某些显卡上可能不支持，如果不像我这么懒的话...还是额外申请一张RT
            commandBuffer.Blit(rendertexture, rendertexture, material);
        }


        //直接加入相机的CommandBuffer事件队列中 加入到相机的渲染事件中每一帧都会调用
        Camera.main.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, commandBuffer);

    }
    void OnDisable()
    {
        //移除事件，清理资源
        if (Camera.main)
        {
            Camera.main.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, commandBuffer);
        }
        commandBuffer.Clear();
        rendertexture.Release();
        
    }

    //为方便调整，放在update里面了
    void Update()
    {
        material.SetFloat("_Brightness", brightness);
        material.SetFloat("_Saturation", saturation);
        material.SetFloat("_Contrast", contrast);
    }


}
