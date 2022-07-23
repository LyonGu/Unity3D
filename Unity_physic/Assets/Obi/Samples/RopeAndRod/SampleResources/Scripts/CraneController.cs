using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class CraneController : MonoBehaviour {

	ObiRopeCursor cursor;
	ObiRope rope;

	// Use this for initialization
	void Start () {
		cursor = GetComponentInChildren<ObiRopeCursor>();
		rope = cursor.GetComponent<ObiRope>();
	}


	private Vector3 _prePos;
	// Update is called once per frame
	void Update () {
		
		//原始插件代码
		// if (Input.GetKey(KeyCode.W)){
		// 	if (rope.restLength > 6.5f)
		// 		cursor.ChangeLength(rope.restLength - 1f * Time.deltaTime);
		// }
		//
		// if (Input.GetKey(KeyCode.S)){
		// 	cursor.ChangeLength(rope.restLength + 1f * Time.deltaTime);
		// }
		//
		// if (Input.GetKey(KeyCode.A)){
		// 	transform.Rotate(0,Time.deltaTime*15f,0);
		// }
		//
		// if (Input.GetKey(KeyCode.D)){
		// 	transform.Rotate(0,-Time.deltaTime*15f,0);
		// }
		
		//自己修改测试
		if (Input.GetMouseButtonDown(0))
		{
			_prePos = Input.mousePosition;
		}
		else if (Input.GetMouseButton(0))
		{
			var detal = Input.mousePosition - _prePos;
			if (detal.x > 0)
			{
				transform.Rotate(0,-Time.deltaTime*15f,0);
			}
			else if(detal.x < 0)
			{
				transform.Rotate(0,Time.deltaTime*15f,0);
			}

			if (detal.y > 0)
			{
				if (rope.restLength > 6.5f)
					cursor.ChangeLength(rope.restLength - 1f * Time.deltaTime);
			}
			else if (detal.y < 0)
			{
				cursor.ChangeLength(rope.restLength + 1f * Time.deltaTime);
			}

			_prePos = Input.mousePosition;
		}



		// Debug.Log($"X Z========={x}  {z}");
	}
}
