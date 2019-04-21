using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelloWorld : MonoBehaviour {


    private GameObject testCube;
	// Use this for initialization
	void Start () {
        Debug.Log("Hello world");

        GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");

        testCube = camera.transform.Find("testCube").gameObject;//取某个gameobject的子gameobject
        testCube.GetComponent<MeshRenderer>().material.color = Color.blue;
	}
	
	// Update is called once per frame
	void Update () {
        this.transform.Rotate(Vector3.up, 1F);


        if (Input.touchCount == 1)
        {
            if (Input.touches[0].phase == TouchPhase.Began)
            {
                Vector2 m_screenPos = Input.touches[0].position;   //记录手指刚触碰的位置  
                print("touch pos:" + m_screenPos.x + "," + m_screenPos.y);

                testCube.transform.Translate(Vector3.right * 1);
            }
        }

        if(Input.GetMouseButtonDown(0))
        {
            Vector2 t = Input.mousePosition; //获取的仅仅是屏幕坐标
            print("t pos:" + t.x + "," + t.y);

            //跟随移动需要转换坐标
            Vector3 ScreenSpace = Camera.main.WorldToScreenPoint(transform.position);
            Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(t.x, t.y, ScreenSpace.z));
           print("pos pos:" + pos.x + "," + pos.y);
           transform.position = new Vector3(pos.x, pos.y, pos.z);

        }
	}
}
