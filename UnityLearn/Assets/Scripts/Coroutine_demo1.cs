using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coroutine_demo1 : MonoBehaviour {

	// Use this for initialization
	void Start () {
        print("1");

        //启动协程
        StartCoroutine("CoroutineDemo1");
        print("3");
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    IEnumerator CoroutineDemo1()
    {
        //等待1秒
        yield return new WaitForSeconds(1F);
        print("2");
    }
}
