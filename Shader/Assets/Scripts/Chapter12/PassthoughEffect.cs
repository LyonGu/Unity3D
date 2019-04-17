using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PassthoughEffect : PostEffectsBase{

    public Shader passthoughShader;
    private Material passthoughMaterial;


    public Material material
    {
        get {
            passthoughMaterial = CheckShaderAndCreateMaterial(passthoughShader, passthoughMaterial);
            return passthoughMaterial;
        }
    }

    //扭曲强度
    [Range(0, 1)]
    public float distortFactor = 0.0f;
    //扭曲中心（0-1）屏幕空间，默认为中心点
    public Vector2 distortCenter = new Vector2(0.5f, 0.5f);

    //噪声图
    public Texture NoiseTexture = null;
    //屏幕扰动强度
    [Range(0, 2.0f)]
    public float distortStrength = 1.0f;

    //屏幕收缩总时间
    public float passThoughTime = 1.0f;
    //当前时间
    private float currentTime = 0.0f;
    //曲线控制权重
    public float curveFactor = 0.2f;
    //屏幕收缩效果曲线控制
    public AnimationCurve curve;


    //扰动曲线系数
    public float distortCurveFactor = 1.0f;
    //屏幕扰动效果曲线控制
    public AnimationCurve distortCurve;

    private float _maxPass = 0.3f;

    public void Start()
    {
        currentTime = 0.0f;
        StartCoroutine("UpdatePassthoughEffect");
    }

    void CreateOrOPenFile(string pathName, string info)
    {          //路径、文件名、写入内容
        StreamWriter sw;
        FileInfo fi = new FileInfo(pathName);
        sw = fi.CreateText();        //直接重新写入，如果要在原文件后面追加内容，应用fi.AppendText()
        sw.WriteLine(info);
        sw.Close();
        sw.Dispose();
    }

    private IEnumerator UpdatePassthoughEffect()
    {

        string result1 = "";
        string result2 = "";
        while (currentTime < passThoughTime)
        {
            currentTime += Time.deltaTime;
            float t = currentTime / passThoughTime;

            float test1 = (float)(Mathf.Round(curve.Evaluate(t) * 100))/100;
            float test2 = (float)(Mathf.Round(distortCurve.Evaluate(t) * 100)) / 100;
            result1 = result1 + test1 + ",";
            result2 = result2 + test2 + ",";
           
            //根据时间占比在曲线（0，1）区间采样，再乘以权重作为收缩系数
            distortFactor = curve.Evaluate(t) * curveFactor;
            distortStrength = distortCurve.Evaluate(t) * distortCurveFactor;
            yield return null;
            //结束时强行设置为0
            distortFactor = 0.0f;
            distortStrength = 0.0f;

        }

        CreateOrOPenFile("config/1.txt", result1);
        CreateOrOPenFile("config/2.txt", result2);

    }



    //OnRenderImage绘制绘制完所有透明和不透明的物体后每一帧调用
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (material != null)
        {
            currentTime += Time.deltaTime;
            float t = currentTime / passThoughTime;
            //distortFactor = Mathf.Lerp(0, 1, t * curveFactor);

            //if (currentTime >= passThoughTime*0.5)
            //{
            //    distortStrength = Mathf.Lerp(0, 1, t * distortCurveFactor);
            //}
            

            //if (currentTime >= passThoughTime)
            //{
            //    distortFactor = 0;
            //    distortStrength = 0;
            //}


          

            //设置shader属性值
            material.SetTexture("_NoiseTex", NoiseTexture);
            material.SetFloat("_DistortFactor", distortFactor);
            material.SetVector("_DistortCenter", distortCenter);
            material.SetFloat("_DistortStrength", distortStrength);

            //把屏幕纹理src传进shader里_MainTex属性
            Graphics.Blit(src, dest, material);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }

}
