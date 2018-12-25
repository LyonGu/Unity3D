using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo5_getCompent : MonoBehaviour {

	// Use this for initialization
	void Start () {

        //使用GetComponent方法，做类之间的传递
        int value = this.gameObject.GetComponent<Demo6>().getValue();
        string str = this.gameObject.GetComponent<Demo6>().str;

        Debug.Log("得到demo6中的组件value是"+ value);
        Debug.Log("得到demo6中的组件str是" + str);
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
