using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class AddingButton : MonoBehaviour {

    public GameObject goParent;
	// Use this for initialization
	void Start () {
		//脚本创建一个button对象

        GameObject goNewObject = new GameObject("Button");

        //设置button的父对象
        goNewObject.GetComponent<Transform>().SetParent(goParent.GetComponent<Transform>(),false);

        //设置button的显示外观
        Image image = goNewObject.AddComponent<Image>();
        Button btn = goNewObject.AddComponent<Button>();

        //image.overrideSprite = Resources.Load<Sprite>("Resources/Textures/login_select.png");

        //无效尼玛
        //image.overrideSprite = Resources.Load("Textures/DarkFloor.jpg", typeof(Sprite)) as Sprite;//这里就是修改他的图片，

  
        //btn监听事件
        btn.onClick.AddListener(ProcessSomething);

        //image.color = Color.red;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void ProcessSomething()
    {
        print("button 动态添加");
    }
}
