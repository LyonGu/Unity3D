using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo7 : MonoBehaviour {

	// Use this for initialization
	void Start () {

        //2秒之后调用DisplayInfo方法
		Invoke("DisplayInfo",2F);

        //5秒后开始调用，每隔0.3秒调用一次
        //InvokeRepeating("DisplayInfo1",5F,0.3F);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void DisplayInfo()
    {
        Debug.Log("DisplayInfo=================");
    }

    void DisplayInfo1()
    {
        Debug.Log("DisplayInfo1=================");
    }
}
