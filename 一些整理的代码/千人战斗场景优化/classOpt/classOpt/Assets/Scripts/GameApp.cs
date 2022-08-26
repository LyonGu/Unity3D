using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameApp : MonoBehaviour {

    public GameObject archerPrefab = null;

	// Use this for initialization
	void Start () { // 20x40 --->800个小兵;
        float z = 0;
        for (int i = 0; i < 20; i++) {
            float x = 0;
            for (int j = 0; j < 40; j++) {
                GameObject archer = GameObject.Instantiate(this.archerPrefab);
                archer.transform.SetParent(this.transform, false);
                archer.transform.position = new Vector3(x, 0, z);
                x ++;
            }
            z += 1;
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
