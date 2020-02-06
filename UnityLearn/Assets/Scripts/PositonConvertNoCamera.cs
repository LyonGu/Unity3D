using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*

*/
public class PositonConvertNoCamera : MonoBehaviour {


    public RectTransform canvasRectTransform; //根canvas的RectTransform
    public RectTransform textRectTransform;   //子UI的canvas的RectTransform
    public RectTransform txtParentRectTransform;

    public Transform cubeTransform;

    public RectTransform localUIRectTransform;
    public RectTransform targetUIRectTransform;

    public RectTransform localUIRectTransform2;
    public RectTransform targetUIRectTransform2;

    public Camera UICamera;

    private Camera mainCamera;


    public Transform cubeTargetTransform;
    public Transform mainCameraTransform;


    // Use this for initialization
    void Start () {


        mainCamera = Camera.main;
        worldToScreenInUICamera();
        LocalUIToScreenAndWorld();

    }

    //UGUI 坐标转屏幕坐标 和世界坐标
    void LocalUIToScreenAndWorld()
    {

   
        //overlay 模式，屏幕坐标系原点和世界坐标重合，所以世界坐标就是屏幕坐标
        Vector3 screenPos = localUIRectTransform.position;
        Debug.Log("screenPos:" + screenPos.x + "/  " + screenPos.y);




        Vector2 localPos = SceenPos2UGUI(screenPos, targetUIRectTransform, canvasRectTransform);
        targetUIRectTransform.anchoredPosition = localPos;  //targetUIRectTransform必须是锚点在中间

        Vector2 screenPos2 = localUIRectTransform2.position;
        Debug.Log("screenPos2:" + screenPos2.x + "/  " + screenPos2.y);
        localPos = SceenPos2UGUI(screenPos2, targetUIRectTransform2, canvasRectTransform);
        targetUIRectTransform2.anchoredPosition = localPos;


        ////转成屏幕坐标不可以直接用Main.camera
        //Vector3 worldPos1 = localUIRectTransform.position;
        //Vector3 screenPosOrgin1 = mainCamera.WorldToScreenPoint(worldPos1);
        //Debug.Log("screenPosOrgin1:" + screenPosOrgin1.x + "/  " + screenPosOrgin1.y);



        //利用屏幕坐标转成世界坐标 screenPos
        float oldz = cubeTargetTransform.position.z;
        Vector3 worldPosConvert5 = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(mainCameraTransform.position.z) + oldz));//z为相机的z轴值绝对值
        Debug.Log("worldPosConvert5:" + worldPosConvert5.x + "/  " + worldPosConvert5.y + "/  " + worldPosConvert5.z);
        cubeTargetTransform.position = worldPosConvert5;


    }


    void worldToScreenInUICamera()
    {

        //世界坐标
        Vector3 wPos = cubeTransform.position;

        //转换成屏幕坐标
        Vector3 screenPos = Camera.main.WorldToScreenPoint(wPos);

        Vector2 localPos = SceenPos2UGUI(new Vector2(screenPos.x, screenPos.y), textRectTransform, txtParentRectTransform);
        textRectTransform.anchoredPosition = localPos;


    }

    Vector2 SceenPos2UGUI(Vector2 ScreenPos, RectTransform target, RectTransform parent)
    {
        Vector2 outVec;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, ScreenPos, null, out outVec);
        float pW = parent.rect.width;
        float pH = parent.rect.height;
        Vector2 anchorMax = target.anchorMax;
        Vector2 anchorMin = target.anchorMin;
        Vector2 anchorOffset;
        if (anchorMax.x == anchorMin.x && anchorMax.y == anchorMin.y)
        {
            Debug.Log("锚点为一个点:");
            //锚点为一个点
            anchorOffset = new Vector2(0.5f - anchorMax.x, 0.5f - anchorMax.y);
        }
        else
        {
            //锚点为一个区域（拉伸）
            Debug.Log("锚点为一个区域");
            anchorOffset = new Vector2(0.5f - (anchorMin.x + anchorMax.x) * 0.5f, 0.5f - (anchorMin.y + anchorMax.y) * 0.5f);  
        }
        outVec.x = outVec.x + (anchorOffset.x * pW);
        outVec.y = outVec.y + (anchorOffset.y * pH);
        return outVec;
    }
	// Update is called once per frame
	void Update () {

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 screenPos = Input.mousePosition;
            Debug.Log("screenPos:" + screenPos.x + "/  " + screenPos.y);


            float oldz = cubeTargetTransform.position.z;
            //Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(mainCameraTransform.localPosition.z))); //z为相机的z轴值绝对值

            //ScreenToWorldPoint,传入的vector3的z值 相当于距离摄像机的距离
            //获取的屏幕坐标Input.mousePosition是一个2d坐标，z轴值为0,这个z值是相对于当前camera的，为零表示z轴与相机重合了，
            //因此给ScreenToWorlfdPoint传值时，不能直接传Input.mousePosition，否则获取的世界坐标永远只有一个值；
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(mainCameraTransform.position.z) + oldz)); //z为相机的z轴值绝对值
 
            cubeTargetTransform.position = worldPos;

            //Vector2 outVec;
            //RectTransformUtility.ScreenPointToLocalPointInRectangle(txtParentRectTransform, Input.mousePosition, null, out outVec);


            //float pW = txtParentRectTransform.rect.width;
            //float pH = txtParentRectTransform.rect.height;

            //Vector2 maxOffset = textRectTransform.anchorMax - new Vector2(0.5f, 0.5f);
            //outVec.x = outVec.x - 2 * maxOffset.x * Screen.width / 2;
            //outVec.y = outVec.y - 2 * maxOffset.y * Screen.height / 2;
            //Debug.Log("Setting anchored positiont to: " + outVec);
            //textRectTransform.position = outVec;
            //outVec.x = outVec.x + (0.5f * pW); //（0,0.5）
            //outVec.x = outVec.x + (-0.5f * pW);  //（1.0,0.5）

            //outVec.y = outVec.y + (-0.5f * pH);  //（0.5,1.0）
            //outVec.y = outVec.y + (0.5f * pH);  //（0.5,0）

            //（0.5,0.5）
            //outVec.x = outVec.x + (0f * pW);
            //outVec.y = outVec.y + (0f * pH);


            //（0,1）
            //outVec.x = outVec.x + (0.5f * pW);
            //outVec.y = outVec.y + (-0.5f * pH);

            //（0,0）
            //outVec.x = outVec.x + (0.5f * pW);
            //outVec.y = outVec.y + (0.5f * pH);

            //（1,1）
            //outVec.x = outVec.x + (-0.5f * pW);
            //outVec.y = outVec.y + (-0.5f * pH);

            //（1,0）
            //outVec.x = outVec.x + (-0.5f * pW);
            //outVec.y = outVec.y + (0.5f * pH);


            //拉伸（0-1,0.5） 水平居中拉伸
            //outVec.x = outVec.x + (0f * pW);
            //outVec.y = outVec.y + (0f * pH);

            //拉伸（0-1,1.0）水平部拉伸
            //outVec.x = outVec.x + (0f * pW);
            //outVec.y = outVec.y + (-0.5f * pH);

            //拉伸（0-1,0）水平底部拉伸
            //outVec.x = outVec.x + (0f * pW);
            //outVec.y = outVec.y + (0.5f * pH);


            //拉伸（0.5,0-1）竖直中部拉伸
            //outVec.x = outVec.x + (0f * pW);
            //outVec.y = outVec.y + (0f * pH);


            //拉伸（0,0-1）竖直左部拉伸
            //outVec.x = outVec.x + (0.5f * pW);
            //outVec.y = outVec.y + (0f * pH);


            //拉伸（1.0,0-1）竖直右部拉伸
            //outVec.x = outVec.x + (-0.5f * pW);
            //outVec.y = outVec.y + (0f * pH);

            //拉伸（0-1,0-1）水平竖直都拉伸
            //outVec.x = outVec.x + (0f * pW);
            //outVec.y = outVec.y + (0f * pH);

            //Vector2 textAnchorMax = textRectTransform.anchorMax;
            //Vector2 textAnchorMin = textRectTransform.anchorMin;

            //if (textAnchorMax.x == textAnchorMin.x && textAnchorMax.y == textAnchorMin.y)
            //{
            //    Debug.Log("锚点为一个点:");
            //    //锚点为一个点
            //    Vector2 anchorOffset = new Vector2(0.5f - textAnchorMax.x, 0.5f - textAnchorMax.y);
            //    outVec.x = outVec.x + (anchorOffset.x * pW);
            //    outVec.y = outVec.y + (anchorOffset.y * pH);
            //}
            //else
            //{
            //    //锚点为一个区域（拉伸）
            //    Debug.Log("锚点为一个区域");
            //    Vector2 anchorOffset = new Vector2(0.5f - (textAnchorMin.x + textAnchorMax.x) * 0.5f, 0.5f - (textAnchorMin.y + textAnchorMax.y) * 0.5f);
            //    outVec.x = outVec.x + (anchorOffset.x * pW);
            //    outVec.y = outVec.y + (anchorOffset.y * pH);
            //}

            //// outVec.x = outVec.x + (0.5f * pW);
            //textRectTransform.anchoredPosition = outVec; //anchoredPosition 才是UGUI坐标系的坐标位置（属性面板里position属性）
            ////textRectTransform.position = outVec;  //position是代表世界空间的坐标
            ///

            Vector2 localPos = SceenPos2UGUI(Input.mousePosition, textRectTransform, txtParentRectTransform);
            textRectTransform.anchoredPosition = localPos;
        }
	}
}
