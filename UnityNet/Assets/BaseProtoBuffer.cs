using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseProtoBuffer : MonoBehaviour
{
    // Start is called before the first frame update‘

    [Serializable]
    public class  Person
    {
        public int age = 10;
        public string name = "dd";
    }
    
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
