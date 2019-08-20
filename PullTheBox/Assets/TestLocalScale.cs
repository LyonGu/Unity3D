
/***
 *
 *  Title: "Guardian" 项目
 *         描述：
 *
 *  Description:
 *        功能：
 *       
 *
 *  Date: 2019
 * 
 *  Version: 1.0
 *
 *  Modify Recorder:
 *     
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestLocalScale : MonoBehaviour {


    private Transform cube;
	// Use this for initialization
	void Start () {
        cube = this.transform;
        StartCoroutine("scaleAction");
	}

    IEnumerator scaleAction()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.2f);
            Vector3 scale = cube.localScale;
            scale.z += 0.2f;
            cube.localScale = scale;
        }
       
    }

	// Update is called once per frame
	void Update () {
		
	}
}
