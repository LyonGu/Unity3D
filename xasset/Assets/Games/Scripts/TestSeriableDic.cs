using System;
using System.Collections;
using System.Collections.Generic;
using HxpGame;
using UnityEngine;



public class TestSeriableDic : MonoBehaviour
{
    [Serializable]
    public class MyData
    {
        public string key;
        public string name;
    }
    
    [Serializable]
    public  class MyDictionary : SerializedDictionary<Int32, MyData>
    {
    
    }
    
    public MyDictionary myDic = new MyDictionary()
    {
        {12, new MyData() {key = "FirstValue", name = "dfsdfds"}}
    };
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
