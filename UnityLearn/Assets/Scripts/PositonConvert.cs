using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PositonConvert : MonoBehaviour {


    public RectTransform canvasRectTransform; //根canvas的RectTransform
    public RectTransform textRectTransform;   //子UI的canvas的RectTransform

    public Transform cubeTransform;

    public Camera UICamera;


	// Use this for initialization
	void Start () {

        worldToScreenInUICamera();
	}


    void worldToScreenInUICamera()
    {

        //世界坐标
        Vector3 wPos = cubeTransform.position;

        //转换成屏幕坐标
        Vector3 screenPos = Camera.main.WorldToScreenPoint(wPos);


        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPos, UICamera, out localPos);
        textRectTransform.anchoredPosition = localPos;

    
    }
	// Update is called once per frame
	void Update () {

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 screenPos = Input.mousePosition;
            Vector2 outVec;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, Input.mousePosition, UICamera, out outVec);

            Debug.Log("Setting anchored positiont to: " + outVec);
            //textRectTransform.position = outVec;
            textRectTransform.anchoredPosition = outVec; //anchoredPosition 才是UGUI坐标系的坐标位置（属性面板里position属性）
            //textRectTransform.position = outVec;  //position是代表世界空间的坐标
        }
	}
}
