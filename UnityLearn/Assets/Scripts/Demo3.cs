using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Demo3 : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.A))
        {
			this.gameObject.AddComponent(typeof(SelfRotation));
			GameObject.Destroy(this.gameObject);
        }
	}

	private void OnDisable()
	{
		Debug.Log(GetType() + "/OnDisable");
	}

	private void OnDestroy()
	{
		Debug.Log(GetType() + "/OnDestroy");
	}

}
