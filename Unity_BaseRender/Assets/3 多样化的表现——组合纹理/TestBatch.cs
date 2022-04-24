using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBatch : MonoBehaviour
{
    public GameObject cube1;
    public GameObject cube2;

    public Texture tex1;
    public Texture tex2;
    // Start is called before the first frame update
    void Start()
    {
        MaterialPropertyBlock prop1 = new MaterialPropertyBlock();
        cube1.GetComponent<Renderer>().GetPropertyBlock(prop1);
        prop1.SetTexture("_MainTex",tex1);
        cube1.GetComponent<Renderer>().SetPropertyBlock(prop1);
        
        MaterialPropertyBlock prop2 = new MaterialPropertyBlock();
        cube2.GetComponent<Renderer>().GetPropertyBlock(prop2);
        prop2.SetTexture("_MainTex",tex2);
        cube2.GetComponent<Renderer>().SetPropertyBlock(prop2);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
