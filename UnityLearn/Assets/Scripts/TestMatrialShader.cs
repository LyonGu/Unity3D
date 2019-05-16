
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

public class TestMatrialShader : MonoBehaviour {

	// Use this for initialization
	void Start () {

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "cube";
        cube.transform.parent = this.gameObject.transform;
        cube.transform.position = new Vector3(0.0f, 0.0f, 0.0f);


        GameObject cube1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube1.name = "cube1";
        cube1.transform.parent = this.gameObject.transform;
        cube1.transform.position = new Vector3(2.0f, 0.0f, 0.0f);


        //Material cubeMa = cube.GetComponent<Renderer>().material;

        Shader sh = Resources.Load<Shader>("Shaders/Blue");
        cube.GetComponent<Renderer>().material.shader = sh;  //material 相当于新建了一个新的材质， 修改这个不会影响其他使用同一个材质的游戏对象

        //cube.GetComponent<Renderer>().sharedMaterial.shader = sh; //sharedMaterial 所有的gameObject都共享着同一个材质 修改这个会影响其他使用同一个材质的游戏对象

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
