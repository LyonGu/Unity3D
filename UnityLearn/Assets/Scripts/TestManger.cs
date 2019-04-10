using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestManger : MonoBehaviour {


    public GameObject _obj;

	// Use this for initialization
	void Start () {
        Invoke("clearTestObj", 3.0f);
	}

    void clearTestObj()
    {
        DestroyImmediate(_obj);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
