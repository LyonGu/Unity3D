using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class TestClearPath : MonoBehaviour
{
    public AIBase _Ai;

    private float countTimer = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        countTimer += Time.deltaTime;
      
        if (countTimer >= 3.0f)
        {
            //设置Mode为Never或者把目标位置设置float.PositiveInfinity，再次开启后就不会自动去计算寻路路径了
            _Ai.autoRepath.mode = AutoRepathPolicy.Mode.Never;
            _Ai.enabled = true; //启用后会在Update里自动计算寻路路线
        }
        else if (countTimer >= 1.0f)
        {
            _Ai.enabled = false; //OnDisable里会清理路径数据（会调用clearPath）
            
            
        }
    }
}
