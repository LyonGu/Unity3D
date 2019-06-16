

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
/*
 
    对于后处理中一些效果可以使用commanderBuffer来提高效率：景深，扭曲
 
 */

public class CommoandBufferPostEffect : MonoBehaviour {

	private CommandBuffer commandBuffer = null;
    private Renderer targetRenderer = null;
 
    void OnEnable()
    {
        targetRenderer = this.GetComponentInChildren<Renderer>();
        if (targetRenderer)
        {
            // commandBuffer的渲染是前向渲染，targetRenderer.material中使用的shader不能是后向渲染
            commandBuffer = new CommandBuffer();
            commandBuffer.DrawRenderer(targetRenderer, targetRenderer.material);
            //直接加入相机的CommandBuffer事件队列中,
            Camera.main.AddCommandBuffer(CameraEvent.AfterImageEffects, commandBuffer);
            targetRenderer.enabled = false;
        }
    }
 
    void OnDisable()
    {
        if (targetRenderer)
        {
            //移除事件，清理资源
            Camera.main.RemoveCommandBuffer(CameraEvent.AfterImageEffects, commandBuffer);
            commandBuffer.Clear();
            targetRenderer.enabled = true;
        }
    }

}
