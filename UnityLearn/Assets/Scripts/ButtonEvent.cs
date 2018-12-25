using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonEvent : MonoBehaviour {

    public Text Text_DisplayInfo;   //信息显示控件

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void DisplayInfo()
    {
        Debug.Log("Button 被击中了！！！！！");
    }

    public void DisplayInfoByText()
    {
        Text_DisplayInfo.text = "Button 被击中了";
    }
}
