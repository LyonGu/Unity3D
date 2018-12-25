using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceiveObj : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void DisplayInfo_A()
    {
        print(GetType() + "/DisplayInfo_A 无参方法");
    }

    public void DisplayInfo_B()
    {
        print(GetType() + "/DisplayInfo_B 无参方法");
    }

    public void DisplayInfo_2(string info)
    {
        print(GetType() + "/DisplayInfo_2 带一个字符串参数，info = " + info);
    }

    public void DisplayInfo_3(int info)
    {
        print(GetType() + "/DisplayInfo_3 带一个整形参数，info = " + info);
    }


    public void DisplayInfo_4(System.Object[] objArray)
    {
        print(GetType() + "/DisplayInfo_4 objArray[0] = " + objArray[0]);
        print(GetType() + "/DisplayInfo_4 objArray[1] = " + objArray[1]);
        print(GetType() + "/DisplayInfo_4 objArray[2] = " + objArray[2]);
    }

}
