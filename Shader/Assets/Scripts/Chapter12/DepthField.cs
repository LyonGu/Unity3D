using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

    大部分的景深效果是前景清晰，远景模糊，这也是景深的标准用法，
    不过有时候也有需要近景模糊，远景清晰的效果，或者前后都模糊，中间焦点位置清晰，
    在实现上我们通过像素点深度到达焦点的距离作为参数，在清晰和模糊图像之间插值，先计算远景的，
    结果与模糊图片再进行插值，得到最终的效果。
    
*/

public class DepthField : PostEffectsBase {

	public Shader depthFieldShader;
    private Material depthFieldMaterial;


    //高斯模糊相关参数
    [Range(1, 8)]
    public int downSample = 2;

    [Range(0.2f, 3.0f)]
    public float blurSpread = 0.6f;

    [Range(0, 4)]
    public int iterations = 3;


    //深度信息相关参数
    [Range(0.0f, 100.0f)]
    public float focalDistance = 10.0f;

    [Range(0.0f, 100.0f)]
    public float nearBlurScale = 0.0f;

    [Range(0.0f, 1000.0f)]
    public float farBlurScale = 50.0f;


    public Material material
    {
        get {
            depthFieldMaterial = CheckShaderAndCreateMaterial(depthFieldShader, depthFieldMaterial);
            return depthFieldMaterial;
        }
    }

    private Camera _mainCam = null;
    public Camera MainCam
    {
        get
        {
            if (_mainCam == null)
                _mainCam = GetComponent<Camera>();
            return _mainCam;
        }
    }


    void OnEnable()
    {
        //maincam的depthTextureMode是通过位运算开启与关闭的
        MainCam.depthTextureMode |= DepthTextureMode.Depth;
    }
 
    void OnDisable()
    {
        MainCam.depthTextureMode &= ~DepthTextureMode.Depth;
    }


	
	//OnRenderImage绘制绘制完所有透明和不透明的物体后每一帧调用
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (material != null)
        {
            
            //首先将我们设置的焦点限制在远近裁剪面之间
            Mathf.Clamp(focalDistance, MainCam.nearClipPlane, MainCam.farClipPlane);



            int rtW = src.width / downSample;
            int rtH = src.height / downSample;

            //分配一块缓冲区
            RenderTexture buffer0 = RenderTexture.GetTemporary(rtW, rtH, 0);
            buffer0.filterMode = FilterMode.Bilinear;

            //把src图像数据缩放后存到buffer0中
            Graphics.Blit(src, buffer0);

            //进行高斯模糊
            for (int i = 0; i < iterations; i++)
            {
                material.SetFloat("_BlurSize", 1.0f + i * blurSpread);

                RenderTexture buffer1 = RenderTexture.GetTemporary(rtW, rtH, 0);

                
                Graphics.Blit(buffer0, buffer1, material, 0);

                //释放buffer0缓冲区数据，操作完之后一定要解绑，跟opengl的vao和vbo一样
                RenderTexture.ReleaseTemporary(buffer0);
                buffer0 = buffer1;

                //再次分配一块缓冲区
                buffer1 = RenderTexture.GetTemporary(rtW, rtH, 0);

    
                //这里的buffer0就是上一个pass处理过的缓冲区数据了
                Graphics.Blit(buffer0, buffer1, material, 1);

                RenderTexture.ReleaseTemporary(buffer0);
                buffer0 = buffer1;
            }
            material.SetTexture("_BlurTex", buffer0);
            //设置shader的参数，主要是焦点和远近模糊的权重，权重可以控制插值时使用模糊图片的权重
            material.SetFloat("_focalDistance", FocalDistance01(focalDistance));
            material.SetFloat("_nearBlurScale", nearBlurScale);
            material.SetFloat("_farBlurScale", farBlurScale);
           
            //把原始图片进行shader里pass处理，融合原始图与buffer0
            Graphics.Blit(src, dest, material, 2);  
            RenderTexture.ReleaseTemporary(buffer0);  //最后一定要解绑
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }

    //计算设置的焦点被转换到01空间中的距离，以便shader中通过这个01空间的焦点距离与depth比较
    private float FocalDistance01(float distance)
    {
        return MainCam.WorldToViewportPoint((distance - MainCam.nearClipPlane) * MainCam.transform.forward + MainCam.transform.position).z / (MainCam.farClipPlane - MainCam.nearClipPlane);
    }
	
}
