
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

/*
 
 直接在一张RT上画个人，其实类似于摄影机效果，我们用当前的相机看见正常要看见的对象，
 然后在一张幕布（简单的来说，就是一个。。额，面片）再渲染一次这个人物（也可以直接渲染到UI上）。
 
 */
public class CommandBufferTest : MonoBehaviour {


    private CommandBuffer commandBuffer = null;
    private RenderTexture rendertexture = null;
    private Renderer targetRenderer = null;

    public GameObject targetObject;
    public Material replaceMaterial;


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
        Material mat = replaceMaterial == null ? targetRenderer.material : replaceMaterial;
        commandBuffer.DrawRenderer(targetRenderer, mat);

        this.GetComponent<Renderer>().material.mainTexture = rendertexture;

        //直接加入相机的CommandBuffer事件队列中，加入到相机的渲染事件中每一帧都会调用
        Camera.main.AddCommandBuffer(CameraEvent.AfterForwardOpaque, commandBuffer);
    
    }
     void OnDisable()
        {
            //移除事件，清理资源
            Camera.main.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, commandBuffer);
            commandBuffer.Clear();
            rendertexture.Release();
        }

     //也可以在OnPreRender中直接通过Graphics执行Command Buffer，不过OnPreRender和OnPostRender只在挂在相机的脚本上才有作用！！！
    //void OnPreRender()
    //{
    //    //在正式渲染前执行Command Buffer
    //    Graphics.ExecuteCommandBuffer(commandBuffer);
    //}

}
