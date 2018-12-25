using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendObj : MonoBehaviour {

    //接收对象
    public GameObject goReceiveObj;


	// Use this for initialization
	void Start () {

        //怎么感觉就在调用其他对象的方法就是了


        //发消息
        goReceiveObj.SendMessage("DisplayInfo_A");

        goReceiveObj.SendMessage("DisplayInfo_B", SendMessageOptions.RequireReceiver); //如果消息发送不成功，则报错

       // goReceiveObj.SendMessage("DisplayInfo_B", SendMessageOptions.DontRequireReceiver); //如果消息发送不成功，不报错

        goReceiveObj.SendMessage("DisplayInfo_2","大家好"); // 带参数
        goReceiveObj.SendMessage("DisplayInfo_3", 999);

        System.Object[] objArray = new System.Object[3];
        objArray[0] = "dajasdfsdf";
        objArray[1] = 88;
        objArray[2] = 66.6F;

        goReceiveObj.SendMessage("DisplayInfo_4", objArray);

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
