using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUI_demo1 : MonoBehaviour {


    private string _StrText = "";
    private string _StrPW = "";
    private int _IntSelectIndex = 0;
    private bool _BoolCheck1 = false;



	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnGUI()
    {
        //每一帧都调用 重新绘制（擦除再绘制）
        GUI.Label(new Rect(0, 0, 100, 30), "我是标签框");

        GUI.Button(new Rect(200, 0, 150, 30), "我是按钮");
    }
}
