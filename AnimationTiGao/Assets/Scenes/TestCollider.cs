using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCollider : MonoBehaviour
{

    public BoxCollider BoxCollider;
    // Start is called before the first frame update
    void Start()
    {
        Vector3 size = BoxCollider.size;
        Vector3 size1 = transform.localScale;
        
        //BoxCollider.size 跟 localScale 不同步啊！！！！！
        Debug.Log($"BoxCollider.size==={BoxCollider.size}, transform.localScale = {transform.localScale}");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
