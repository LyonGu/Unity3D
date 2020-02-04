using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*

*/
public class PositonConvertNoCamera : MonoBehaviour {


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

        RectTransform ParentRectTransform = (RectTransform)localUIRectTransform.parent;
        //targetUIRectTransform的父节点是根画布
        Vector2 targetScreenPos = localUIRectTransform.anchoredPosition + ParentRectTransform.anchoredPosition; //得到相对于根画布的坐标
                                                                                                                //targetUIRectTransform.anchoredPosition = targetScreenPos;
        //计算屏幕坐标
        Vector2 RootCanvasPos = localUIRectTransform.anchoredPosition;
        RectTransform cur = localUIRectTransform;
        while (cur.parent)
        {
            cur = (RectTransform)cur.parent;
            if (cur == canvasRectTransform) break;
            RootCanvasPos += cur.anchoredPosition;
        }


        float rax = RootCanvasPos.x / 800; //canvas的宽
        float ray = RootCanvasPos.y / 450; //canvas的高
        Vector2 ratio = new Vector2(rax + 0.5f, ray + 0.5f); //根画布的中心点为中间，先把坐标系转到左下角
        Debug.Log("screenSize:" + Screen.width + "/  " + Screen.height);
        Vector2 screenPos = new Vector2(Screen.width * ratio.x, Screen.height * ratio.y); ; //是相对于根画布的坐标targetScreenPos
        Debug.Log("screenPos:" + screenPos.x + "/  " + screenPos.y);

        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPos, null, out localPos);
        targetUIRectTransform.anchoredPosition = localPos;


        Vector2 RootCanvasPos2 = localUIRectTransform2.anchoredPosition;
        cur = localUIRectTransform2;
        while (cur.parent)
        {
            cur = (RectTransform)cur.parent;
            if (cur == canvasRectTransform) break;
            RootCanvasPos2 += cur.anchoredPosition;
        }

        rax = RootCanvasPos2.x / 800; //canvas的宽
        ray = RootCanvasPos2.y / 450; //canvas的高
        ratio = new Vector2(rax + 0.5f, ray + 0.5f); //根画布的中心点为中间，先把坐标系转到左下角

        Vector2 screenPos2 = new Vector2(Screen.width * ratio.x, Screen.height * ratio.y); ; //是相对于根画布的坐标targetScreenPos
        Debug.Log("screenPos2:" + screenPos2.x + "/  " + screenPos2.y);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPos2, null, out localPos);
        targetUIRectTransform2.anchoredPosition = localPos;


        //转成屏幕坐标不可以直接用Main.camera
        Vector3 worldPos1 = localUIRectTransform.position;
        Vector3 screenPosOrgin1 = mainCamera.WorldToScreenPoint(worldPos1);
        Debug.Log("screenPosOrgin1:" + screenPosOrgin1.x + "/  " + screenPosOrgin1.y);



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


        Vector2 localPos;
        //当canvas没有摄像机时，camera传入null即可
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPos, null, out localPos);
        textRectTransform.anchoredPosition = localPos;

        //public static bool ScreenPointToLocalPointInRectangle(RectTransform rect, Vector2 screenPoint, Camera cam, out Vector2 localPoint);
        //public static bool ScreenPointToWorldPointInRectangle(RectTransform rect, Vector2 screenPoint, Camera cam, out Vector3 worldPoint);
        //public static Vector2 WorldToScreenPoint(Camera cam, Vector3 worldPoint);


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

            Vector2 outVec;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, Input.mousePosition, null, out outVec);

            Debug.Log("Setting anchored positiont to: " + outVec);
            //textRectTransform.position = outVec;
            textRectTransform.anchoredPosition = outVec; //anchoredPosition 才是UGUI坐标系的坐标位置（属性面板里position属性）
            //textRectTransform.position = outVec;  //position是代表世界空间的坐标
        }
	}
}
