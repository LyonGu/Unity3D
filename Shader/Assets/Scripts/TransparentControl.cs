
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransparentControl : MonoBehaviour {

    public class TransparentParam
    {
        public Material[] materials = null;
        public Material[] sharedMats = null;
        public float currentFadeTime = 0;
        public bool isTransparent = true;
        public bool isResume = false;
        public float maxTranspaent = 0.0f;
    }

    public Transform targetObject = null;   //目标对象
    public float height = 0.0f;             //目标对象Y方向偏移
    public float destTransparent = 0.9f;    //遮挡半透的最终半透强度，
    public float fadeInTime = 1.0f;         //开始遮挡半透时渐变时间
    private int transparentLayer;           //需要遮挡半透的层级
    private Dictionary<Renderer, TransparentParam> transparentDic = new Dictionary<Renderer, TransparentParam>();
    private List<Renderer> clearList = new List<Renderer>();

    void Start ()
    {
        transparentLayer = 1 << LayerMask.NameToLayer("OcclusionTran");
	}
	
	void Update ()
    {
        if (targetObject == null)
            return;

        UpdateTransparentObject();
        UpdateRayCastHit();
        RemoveUnuseTransparent();
    }


    public void UpdateTransparentObject()
    {
        var var = transparentDic.GetEnumerator();
        while (var.MoveNext())
        {
            TransparentParam param = var.Current.Value;
            param.isTransparent = false;
            foreach (var mat in param.materials)
            {
                if (!param.isResume && param.maxTranspaent <1.0f)
                {
                    Color col = mat.GetColor("_Color");
                    param.currentFadeTime += Time.deltaTime;
                    float t = param.currentFadeTime / fadeInTime;
                    param.maxTranspaent = t;
                    col.a = Mathf.Lerp(1, destTransparent, t);
                    mat.SetColor("_Color", col);
                }
               
            }
        }
    }

    public void UpdateRayCastHit()
    {
        RaycastHit[] rayHits = null;
        //视线方向为从自身（相机）指向目标位置
        Vector3 targetPos = targetObject.position;
        targetPos.y -= height;
        Vector3 oriPos = transform.position;
        Vector3 viewDir = (targetPos - oriPos).normalized;

        float distance = Vector3.Distance(oriPos, targetPos);
        Ray ray = new Ray(oriPos, viewDir);
        rayHits = Physics.RaycastAll(ray, distance, transparentLayer);
        //直接在Scene画一条线，方便观察射线
        Debug.DrawLine(oriPos, targetPos, Color.red);
         foreach (var hit in rayHits)
        {
            Renderer[] renderers = hit.collider.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                AddTransparent(r);
            }
        }
    }

    void AddTransparent(Renderer renderer)
    { 
        TransparentParam param = null;
        transparentDic.TryGetValue(renderer, out param);
        if (param == null)
        {
            param = new TransparentParam();
            transparentDic.Add(renderer, param);
            //此处顺序不能反，调用material会产生材质实例。
            param.sharedMats = renderer.sharedMaterials;  //记录之前的材质
            param.materials = renderer.materials;
            foreach(var v in param.materials)
            {
                v.shader = Shader.Find("Occlusion/OcclusionTransparent");
            }
        }
        param.isTransparent = true;
    }


    public void RemoveUnuseTransparent()
    {
        clearList.Clear();
        var var = transparentDic.GetEnumerator();
        while (var.MoveNext())
        {
            TransparentParam param = var.Current.Value;
            if (param.isTransparent == false)
            {
                param.isResume = true;
                bool isResumeOver = false;
                foreach (var mat in param.materials)
                {
                    Color col = mat.GetColor("_Color");
                    param.currentFadeTime -= Time.deltaTime;
                    float t = param.currentFadeTime / fadeInTime;
                    col.a = Mathf.Lerp(1, destTransparent, t);
                    mat.SetColor("_Color", col);
                    if (t <= 0.0f)
                    {
                        isResumeOver = true;   
                    }
                }

                if (isResumeOver)
                {
                    //用完后材质实例不会销毁，可以被unloadunuseasset销毁或切场景销毁。
                    param.isResume = false;
                    param.maxTranspaent = 0.0f;
                    var.Current.Key.materials = param.sharedMats;  //var.Current.Key --》render
                    clearList.Add(var.Current.Key);
                }
 
            }
        }
        foreach (var v in clearList)
            transparentDic.Remove(v);
    }

}
