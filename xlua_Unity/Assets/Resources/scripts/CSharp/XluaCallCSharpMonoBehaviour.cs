using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XluaCallCSharpMonoBehaviour : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}


    public void xluaCallCSharp()
    {

        Debug.Log(GetType() + "/xluaCallCSharp :从xlua调用过来");
    
    }
	// Update is called once per frame
	void Update () {
		
	}

	public void TestClickImage()
	{
        Debug.Log(GetType() + "/TestClickImage");
	}

	~XluaCallCSharpMonoBehaviour()
	{
		Debug.Log(GetType() + "/析构调用====");
	}
}
