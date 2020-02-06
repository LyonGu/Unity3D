using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 Canvas 的 Render Mode 设置为 Screen Space - Camera 并设置相机)时:

    Unity 中一个 UI 单位不再是对应一个 Unity 单位 ==> UI节点的position属性值不能直接赋给世界坐标系中3d物体的position属性值
     
     
*/
public class PositonConvert : MonoBehaviour {


    public RectTransform canvasRectTransform; //根canvas的RectTransform
    public RectTransform textRectTransform;   //子UI的canvas的RectTransform

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



        //Vector3 t1 = targetUIRectTransform.anchoredPosition;
        //Debug.Log("t1:" + t1.x + "/  " + t1.y + "/  " + t1.z);
        //targetUIRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        //targetUIRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        //Vector3 t2 = targetUIRectTransform.anchoredPosition;  //立刻改完不会生效
        //Debug.Log("t2:" + t2.x + "/  " + t2.y + "/  " + t2.z);

        Vector3 worldPos1 = localUIRectTransform.position;
        Vector3 screenPosOrgin1 = UICamera.WorldToScreenPoint(worldPos1);

        Vector2 localPos = SceenPos2UGUI(screenPosOrgin1, targetUIRectTransform, canvasRectTransform, UICamera);
        targetUIRectTransform.anchoredPosition = localPos; //targetUIRectTransform必须是锚点在中间


  
        Vector3 worldPos2 = localUIRectTransform2.position;
        Vector3 screenPosOrgin2 = UICamera.WorldToScreenPoint(worldPos2);
        localPos = SceenPos2UGUI(screenPosOrgin2, targetUIRectTransform2, canvasRectTransform, UICamera);
        targetUIRectTransform2.anchoredPosition = localPos;






        //转屏幕坐标
        // 1 有单独摄像机直接用UICamera.WorldToScreenPoint(worldPostion);
        //Vector3 screenPosOrgin = UICamera.WorldToScreenPoint(worldPostion);
        //Debug.Log("screenPosOrgin:" + screenPosOrgin.x + "/  " + screenPosOrgin.y + "/  " + screenPosOrgin.z);







        //转世界坐标 （不同摄像机用屏幕坐标做中介转换）
        float oldz = cubeTargetTransform.position.z;
        Vector3 worldPosConvert5 = mainCamera.ScreenToWorldPoint(new Vector3(screenPosOrgin1.x, screenPosOrgin1.y, Mathf.Abs(mainCameraTransform.position.z) + oldz));//z为相机的z轴值绝对值
        Debug.Log("worldPosConvert5:" + worldPosConvert5.x + "/  " + worldPosConvert5.y + "/  " + worldPosConvert5.z);
        cubeTargetTransform.position = worldPosConvert5;


    }

    Vector2 SceenPos2UGUI(Vector2 ScreenPos, RectTransform target, RectTransform parent, Camera camera)
    {
        Vector2 outVec;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, ScreenPos, camera, out outVec);
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
    void worldToScreenInUICamera()
    {

        //世界坐标
        Vector3 wPos = cubeTransform.position;

        //转换成屏幕坐标
        Vector3 screenPos = Camera.main.WorldToScreenPoint(wPos);

        Vector2 localPos = SceenPos2UGUI(screenPos, textRectTransform, canvasRectTransform, UICamera);
        textRectTransform.anchoredPosition = localPos;

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

            Vector2 outVec = SceenPos2UGUI(Input.mousePosition, textRectTransform, canvasRectTransform, UICamera);
            Debug.Log("Setting anchored positiont to: " + outVec);
           
            textRectTransform.anchoredPosition = outVec; //anchoredPosition 才是UGUI坐标系的坐标位置（属性面板里position属性）
            //textRectTransform.position = outVec;  //position是代表世界空间的坐标
        }
	}
}
