using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

[ExecuteAlways]
public class CapturePic : MonoBehaviour
{
    // Start is called before the first frame update

    public Camera camera;
    public RawImage rawImage;
    public int rtW = 100;
    public int rtH= 100;

    private RenderTargetIdentifier captureRtIdentifier;
    private RenderTexture captureRt;
    private Material blitMaterial;

    private void CaptureAction(RenderTargetIdentifier CameraColorRenderTargetIdentifier, CommandBuffer cmd)
    {
        //把对象拷贝到一张RT上
        cmd.Blit(CameraColorRenderTargetIdentifier,captureRtIdentifier);
    }
    private void AddCaptureAction()
    {
        if (captureRt !=null)
        {
            RenderTexture.ReleaseTemporary(captureRt);
            this.RemoveCaptureAction();
            CoreUtils.Destroy(blitMaterial);
        }
        
        captureRt = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.ARGB32);
        captureRt.filterMode = FilterMode.Point;
        captureRt.wrapMode = TextureWrapMode.Clamp;
        captureRt.name = "_CaptureTexture";
        rawImage.texture = captureRt;
        rawImage.SetNativeSize();
        captureRtIdentifier = new RenderTargetIdentifier(captureRt);
        blitMaterial = CoreUtils.CreateEngineMaterial("Hidden/Universal Render Pipeline/Blit" ); 
        CameraCaptureBridge.AddCaptureAction(camera, CaptureAction);
    }

    private void OnEnable()
    {
        AddCaptureAction();
    }

    private void RemoveCaptureAction()
    {
        CameraCaptureBridge.RemoveCaptureAction(camera, CaptureAction);
    }

    private void Clear()
    {
        RemoveCaptureAction();
        if (captureRt !=null)
        {
            RenderTexture.ReleaseTemporary(captureRt);
            CoreUtils.Destroy(blitMaterial);
            captureRt = null;
            blitMaterial = null;
        }

    }

    private void OnDestroy()
    {
        Clear();
    }

    private void OnDisable()
    {
        Clear();
    }

    // Update is called once per frame
   
}
