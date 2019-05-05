
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

public class AltasTest : MonoBehaviour {

   
	// Use this for initialization
	void Start () {

        Object[] _atlas = Resources.LoadAll("Plist/" + "guanyu");
        SpriteRenderer render = this.GetComponent<SpriteRenderer>();
        render.sprite = (Sprite)_atlas[1];
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
