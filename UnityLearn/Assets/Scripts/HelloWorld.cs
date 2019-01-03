using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelloWorld : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Debug.Log("Hello world");

        GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");

        GameObject testCube = camera.transform.Find("testCube").gameObject;//取某个gameobject的子gameobject
        testCube.GetComponent<MeshRenderer>().material.color = Color.blue;
	}
	
	// Update is called once per frame
	void Update () {
        this.transform.Rotate(Vector3.up, 1F);
	}
}
