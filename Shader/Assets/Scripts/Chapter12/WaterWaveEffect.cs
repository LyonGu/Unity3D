using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterWaveEffect : PostEffectsBase{

    public Shader waterWaveShader;
    private Material waterWaveMaterial;


    public Material material
    {
        get {
            waterWaveMaterial = CheckShaderAndCreateMaterial(waterWaveShader, waterWaveMaterial);
            return waterWaveMaterial;
        }
    }

     //距离系数
    [Range(1.0f, 100.0f)]
    public float distanceFactor = 60.0f;

     //时间系数
    [Range(-50.0f, 50.0f)]
    public float timeFactor = -30.0f;

    //sin函数结果系数
    [Range(0.0f, 1.0f)]
    public float totalFactor = 1.0f;


    //波纹宽度
    [Range(0.0f, 1.0f)]
    public float waveWidth = 0.3f;
    //波纹扩散的速度
    public float waveSpeed = 0.3f;
 
    private float waveStartTime;
    private Vector4 startPos = new Vector4(0.5f, 0.5f, 0, 0);

    public bool isLoop = false;

    // private float _IntervalTime = 1.0f;

    //OnRenderImage绘制绘制完所有透明和不透明的物体后每一帧调用
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (material != null)
        {   
            //计算波纹移动的距离，根据enable到目前的时间*速度求解
            float curWaveDistance = (Time.time - waveStartTime) * waveSpeed;

            //设置shader属性值
            material.SetFloat("_distanceFactor", distanceFactor);
            material.SetFloat("_timeFactor", timeFactor);
            material.SetFloat("_totalFactor", totalFactor);
            material.SetFloat("_waveWidth", waveWidth);
            material.SetFloat("_curWaveDis", curWaveDistance);
            material.SetVector("_startPos", startPos);
            material.SetInt("_isLoop", System.Convert.ToInt32(isLoop));

            //把屏幕纹理src传进shader里_MainTex属性
            Graphics.Blit(src, dest, material);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2 mousePos = Input.mousePosition;
            //将mousePos转化为（0，1）区间
            startPos = new Vector4(mousePos.x / Screen.width, mousePos.y / Screen.height, 0, 0);
            waveStartTime = Time.time;
        }
 
    }

    void OnEnable()
    {
        //设置startTime
        // waveStartTime = Time.time;
    }


}
