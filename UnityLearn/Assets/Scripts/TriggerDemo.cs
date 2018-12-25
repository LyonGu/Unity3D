using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDemo : MonoBehaviour {
    public GameObject goBridge;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void onTriggerEnter(Collider col)
    {
        print("进入触发检测" + col.gameObject.name);

        if (col.name.Equals("test"))
        {
            goBridge.GetComponent<MeshCollider>().enabled = true;
            goBridge.GetComponent<MeshRenderer>().enabled = true;
        }
    }

    void onTriggerStay(Collider col)
    {
        print("停留触发检测" + col.gameObject.name);

    }

    void onTriggerExit(Collider col)
    {
        print("退出触发检测" + col.gameObject.name);
    }
}
