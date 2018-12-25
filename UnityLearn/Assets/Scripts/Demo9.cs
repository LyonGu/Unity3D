using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo9 : MonoBehaviour {

	// Use this for initialization
	void Start () {

        Debug.Log("gaibianyanse====================");

        //按照名称查找游戏对象
        GameObject obj = GameObject.Find("Cube_1");
        obj.GetComponent<Renderer>().material.color = Color.blue;

        //按照tag来找游戏对象 以及 游戏数组
        GameObject obj1 = GameObject.FindGameObjectWithTag("SphereTag");
        obj1.GetComponent<Renderer>().material.color = Color.red;

        GameObject[] objs = GameObject.FindGameObjectsWithTag("CapsuleTag");
        foreach(GameObject o in objs)
        {
            o.GetComponent<Renderer>().material.color = Color.yellow;
        }


	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
