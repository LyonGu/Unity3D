using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAroundByTarget : MonoBehaviour {

    public Transform aroundTraget;
    public float aroundSpeed = 1F;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        this.transform.RotateAround(aroundTraget.position, Vector3.up, aroundSpeed);
	}
}
