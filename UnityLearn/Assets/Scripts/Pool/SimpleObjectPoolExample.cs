using System.Collections;
using System.Collections.Generic;
using UnityEngine;


class Fish
{

}
public class SimpleObjectPoolExample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var pool = new SimpleObjectPool<Fish>(() => new Fish(), initCount: 50);
        Debug.Log($"pool curCount is {pool.CurCount}");

        //从对象池拿出一个
        var fish = pool.Allocate();
        Debug.Log($"pool curCount is {pool.CurCount}");

        //对象池回收一个
        pool.Recycle(fish);
        Debug.Log($"pool curCount is {pool.CurCount}");
    }

    // Update is called once per frame

}
