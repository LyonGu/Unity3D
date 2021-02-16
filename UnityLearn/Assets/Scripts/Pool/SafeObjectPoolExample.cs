using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Msg : IPoolable
{
    public void OnRecycled()
    {
        Debug.Log("OnRecycled");
    }

    public bool IsRecycled { get; set; }
}

public class SafeObjectPoolExample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var msgPool = new SafeObjectPool<Msg>();

        msgPool.Init(100, 50); // max count:100 init count: 50

        Debug.Log($"msgPool.CurCount:{msgPool.CurCount}");

        var fishOne = msgPool.Allocate();

        Debug.Log($"msgPool.CurCount:{msgPool.CurCount}");

        msgPool.Recycle(fishOne);

        Debug.Log($"msgPool.CurCount:{msgPool.CurCount}");

        for (int i = 0; i < 10; i++)
        {
            msgPool.Allocate();
        }

        Debug.Log($"msgPool.CurCount:{msgPool.CurCount}");
    }

    // Update is called once per frame
   
}
