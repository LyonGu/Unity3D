using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDemo : MonoBehaviour {
   // public GameObject goTestObj;

	// Use this for initialization
	void Start () {
		//指定与某个游戏对象之间进行碰撞过滤
        //Physics.IgnoreCollision(this.GetComponent<Collider>(), goTestObj.GetComponent<Collider>());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //碰撞进入检测
    void OnCollisionEnter(Collision col)
    {
        print("碰撞进入，对象名称："+ col.gameObject.name);
    }

    //碰撞停留检测
    void OnCollisionStay(Collision col)
    {
        print("碰撞停留，对象名称：" + col.gameObject.name);
    }

    //碰撞退出检测
    void OnCollisionExit(Collision col)
    {
        print("碰撞退出，对象名称：" + col.gameObject.name);
    }

    
}
