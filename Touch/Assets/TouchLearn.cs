

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TouchLearn : MonoBehaviour {

	// Use this for initialization
    public Text position_txt;
    public Text tips_txt;
    private Transform cameraTran;

    //触摸相关遍历
    private Vector2 touchBegin;
    private float distacne;
    private float lastMoveDis;

    private Vector3 cameraMoveVec3 = Vector3.zero;


	void Start () {
        cameraTran = Camera.main.transform;
        this.GetComponent<MeshRenderer>().material.color = Color.red;
	}
	
	// Update is called once per frame
	void Update () {
        UpdateTouch();
	}


    void resetTouchState()
    {
        touchBegin = Vector2.zero;
        cameraMoveVec3 = Vector3.zero;
        lastMoveDis = 0.0f;
        distacne = 0.0f;
    }
    void UpdateTouch()
    {

        //不支持三指
        int count = Input.touchCount;
        if (count >= 3)
        {
            resetTouchState();
            tips_txt.text = "三指不支持！！！";
            return;
        }

        if (count == 1)
        { 
            //单指
            Touch touchOne = Input.GetTouch(0);
            if (touchOne.phase == TouchPhase.Began)
            {
                touchBegin = touchOne.position;
                position_txt.text = "单指触摸起始点: x" + Mathf.Floor(touchOne.position.x) + " y:" + Mathf.Floor(touchOne.position.y);
                tips_txt.text = "";
            }
            else if (touchOne.phase == TouchPhase.Moved)
            {
                touchOne.position += touchOne.deltaPosition;
                position_txt.text = "单指触摸移动点: x" + Mathf.Floor(touchOne.position.x) + " y:" + Mathf.Floor(touchOne.position.y);
                tips_txt.text = "";
                cameraMoveVec3.x = touchOne.deltaPosition.x;
                //cameraMoveVec3.y = touchOne.deltaPosition.y;
                cameraTran.Translate(cameraMoveVec3 * 0.02f);
            }
            else if (touchOne.phase == TouchPhase.Ended)
            {
                resetTouchState();
                position_txt.text = "单指触摸移动点结束";
                tips_txt.text = "";
            }
        
        }
        else if (count == 2)
        {
            Touch touchOne = Input.GetTouch(0);
            Touch touchTwo = Input.GetTouch(1);
            if (touchOne.phase == TouchPhase.Began || touchTwo.phase == TouchPhase.Began)
            {
                distacne = Vector2.Distance(touchOne.position, touchTwo.position);
                position_txt.text = "双指触摸起始时距离: dis" + Mathf.Floor(distacne);
                tips_txt.text = "";
            }
            else if (touchOne.phase == TouchPhase.Moved || touchTwo.phase == TouchPhase.Moved)
            {
                
                float tempDis = Vector2.Distance(touchOne.position, touchTwo.position);
                if (lastMoveDis > 0.0f)
                {
                    if (Mathf.Abs(tempDis - lastMoveDis)> 2.0f)
                    {
                        cameraMoveVec3.z = tempDis - lastMoveDis;
                        cameraTran.Translate(cameraMoveVec3 * 0.02f);
                    }
                }
                lastMoveDis = tempDis;
                float disDetail = tempDis - distacne;
                position_txt.text = "双指触摸移动时距离: dis:" + Mathf.Floor(tempDis) + " 变化了：" + Mathf.Floor(disDetail);
                tips_txt.text = "";

            }
            else if (touchOne.phase == TouchPhase.Ended || touchTwo.phase == TouchPhase.Ended)
            {
                resetTouchState();
                position_txt.text = "双指触摸移动点结束";
                tips_txt.text = "";
            }
        }

        

       

    } 
}
