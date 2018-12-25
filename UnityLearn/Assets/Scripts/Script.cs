using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Script : MonoBehaviour {

    private GameObject gCloneObj;
	// Use this for initialization
	void Start () {
		
        //创建游戏对象
        GameObject gCreateObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        
        //添加材质
        gCreateObj.GetComponent<Renderer>().material.color = Color.red;

        //添加名称
        gCreateObj.name = "CubeName";

        //克隆对象
        gCloneObj = (GameObject)GameObject.Instantiate(gCreateObj); 
	}
	
	// Update is called once per frame
	void Update () {
		//销毁对象
        if (Input.GetKey(KeyCode.D))
        {
            Destroy(gCloneObj, 2F); //2F表示2秒后销毁，没有参数立即销毁
        }
	}
}
