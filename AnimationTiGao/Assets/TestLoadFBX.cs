using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLoadFBX : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // GameObject 
        GameObject fbxObjRes = Resources.Load("unitychan1") as GameObject;
        GameObject fbxObj = Instantiate(fbxObjRes);
        fbxObj.transform.SetParent(transform, true);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
