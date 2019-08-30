
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

public class TestVector3MoveToWards : MonoBehaviour {

    public Vector3 targetPos;
    public float speed;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        //效果不好
        //Vector3 dis = targetPos - transform.position;
        //transform.Translate(dis * speed * Time.deltaTime, Space.World);
	}
}
