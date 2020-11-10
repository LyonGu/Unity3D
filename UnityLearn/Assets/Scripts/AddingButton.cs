using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class AddingButton : MonoBehaviour {

    public Image testImag;

    public GameObject goParent;

    public TextMeshProUGUI tMeshUGUI;
    // Use this for initialization
    void Start () {
		//脚本创建一个button对象

        GameObject goNewObject = new GameObject("Button");

        //设置button的父对象
        goNewObject.GetComponent<Transform>().SetParent(goParent.GetComponent<Transform>(),false);

        //设置button的显示外观
        Image image = goNewObject.AddComponent<Image>();
        Button btn = goNewObject.AddComponent<Button>();

        image.overrideSprite = Resources.Load<Sprite>("Textures/login_select");//不用写Asset/Resources,不用加后缀 Textures/login_select.png 就不行

        //无效尼玛
        //image.overrideSprite = Resources.Load("Textures/DarkFloor.jpg", typeof(Sprite)) as Sprite;//这里就是修改他的图片，

  
        //btn监听事件,添加不带参数的回调
        //btn.onClick.AddListener(ProcessSomething);

        //image.color = Color.red;

        //Image img_t = (Image)GameObject.FindGameObjectWithTag("player");

        testImag.overrideSprite = Resources.Load<Sprite>("Textures/Floor"); //不用写Asset/Resources,不用加后缀 Textures/Floor.png 就不行

        //添加带参数的回调
        btn.onClick.AddListener(
            delegate()
                {
                    OnClickBtn3(3);
                }
            );

        tMeshUGUI.text = "dfasdfasdf";

        Transform[] trans = goParent.GetComponentsInChildren<Transform>();
        Image img = trans[1].GetComponent<Image>();
        int a = 10;


        int warrningTime = 500;
        string showTime = warrningTime.ToString();
        //showTime = DateTime.Parse(showTime).ToString("mm:ss");
        //DateTime b = DateTime.ParseExact(showTime, "yyyyMMdd", System.Globalization.CurrentCulture);
        ////var dt = DateTime.Now;
        ////showTime = string.Format("{0:t}", dt);
        //Debug.Log($"showTime =  {b.ToString()}");

        int second = warrningTime % 60;
        int min = (warrningTime - second) / 60;
        Debug.Log($"showTime =  {min}:{second}");

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void ProcessSomething()
    {
        print("button 动态添加");
    }

    void OnClickBtn3(int i)
    {
        print("button 动态添加带参数的回调"+"i:"+i);
    }
}
