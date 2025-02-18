﻿using UnityEngine;
using System.Collections;
using Assets.Script.Utils;
using System.Collections.Generic;

public class ShadowProjector : MonoBehaviour 
{
    private Projector _projector;
    //
    private Camera _lightCamera = null;
    private RenderTexture _shadowTex;
    //
    private Camera _mainCamera;
    private List<Renderer> _shadowCasterList = new List<Renderer>();
    private BoxCollider _boundsCollider;
    public float boundsOffset = 1;//边界偏移，
    public Shader shadowReplaceShader;
	void Start () 
    {
        _projector = GetComponent<Projector>();
        _mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        //
        if(_lightCamera == null)
        {
            _lightCamera = gameObject.AddComponent<Camera>();
            _lightCamera.orthographic = true;
            _lightCamera.cullingMask = LayerMask.GetMask("ShadowCaster"); //只渲染ShadowCaster层
            _lightCamera.clearFlags = CameraClearFlags.SolidColor;
            _lightCamera.backgroundColor = new Color(0,0,0,0);
            _shadowTex = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32); //创建一张屏幕大小的rt
            _shadowTex.filterMode = FilterMode.Bilinear;
            _lightCamera.targetTexture = _shadowTex; //设置相机的渲染目标为_shadowTex，渲染的数据都绘制到_shadowTex上
            _lightCamera.SetReplacementShader(shadowReplaceShader, "RenderType"); //替换ShadowCaster层上所有物体使用的shader
            _projector.material.SetTexture("_ShadowTex", _shadowTex); //设置projector组件材质的使用贴图为_shadowTex
            _projector.ignoreLayers = LayerMask.GetMask("ShadowCaster"); //忽略ShadowCaster层，ShadowCaster层不受projector层影响
        }
         GameObject plane = GameObject.Find("Plane");
         foreach (Transform trans in plane.transform)
         {
             if(trans.gameObject.layer == LayerMask.NameToLayer("ShadowCaster"))
             {
                 _shadowCasterList.Add(trans.gameObject.GetComponent<Renderer>());
             }
         }

        _boundsCollider = new GameObject("Test use to show bounds").AddComponent<BoxCollider>();
	}

    void LateUpdate()
    {
        //求阴影产生物体的包围盒
        Bounds b = new Bounds();
        for (int i = 0; i < _shadowCasterList.Count; i++)
        {
            if(_shadowCasterList[i] != null)
            {
                b.Encapsulate(_shadowCasterList[i].bounds);
            }
        }
        b.extents += Vector3.one * boundsOffset;
#if UNITY_EDITOR
        _boundsCollider.center = b.center;
        _boundsCollider.size = b.size;
#endif
        //根据mainCamera来更新lightCamera和projector的位置，和设置参数
        ShadowUtils.SetLightCamera(b, _lightCamera);
        _projector.aspectRatio = _lightCamera.aspect;
        _projector.orthographicSize = _lightCamera.orthographicSize;
        _projector.nearClipPlane = _lightCamera.nearClipPlane;
        _projector.farClipPlane = _lightCamera.farClipPlane;
	}
}
