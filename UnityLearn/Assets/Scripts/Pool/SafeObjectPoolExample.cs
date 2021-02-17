using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Msg : IPoolable
{
    public void OnRecycled()
    {
        Debug.Log("OnRecycled Msg");
    }

    public bool IsRecycled { get; set; }
}

//需要多继承一个IPoolType
class Bullet : IPoolable, IPoolType
{
    public void OnRecycled()
    {
        Debug.Log("OnRecycled  Bullet");
    }

    public bool IsRecycled { get; set; }

    public static Bullet Allocate()
    {
        return SafeObjectPool<Bullet>.Instance.Allocate();
    }

    public void Recycle2Cache()
    {
        SafeObjectPool<Bullet>.Instance.Recycle(this);
    }
}

public class SafeObjectPoolExample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

        //非单例模式使用
        var msgPool = new SafeObjectPoolNormal<Msg>();

        msgPool.Init(100, 50); // max count:100 init count: 50

        Debug.Log($"msgPool.CurCount:{msgPool.CurCount}");
        //从对象池拿出一个对象
        var fishOne = msgPool.Allocate();

        Debug.Log($"msgPool.CurCount:{msgPool.CurCount}");

        //丢回对象池中一个对象
        msgPool.Recycle(fishOne);

        Debug.Log($"msgPool.CurCount:{msgPool.CurCount}");

        for (int i = 0; i < 10; i++)
        {
            msgPool.Allocate();
        }

        Debug.Log($"msgPool.CurCount:{msgPool.CurCount}");

        //单例模式使用
        SafeObjectPool<Bullet>.Instance.Init(50, 25);
        Debug.Log($"BulletPool.CurCount:{ SafeObjectPool<Bullet>.Instance.CurCount}");

        //从对象池拿出一个对象
        var bullet = Bullet.Allocate();
        Debug.Log($"BulletPool.CurCount:{ SafeObjectPool<Bullet>.Instance.CurCount}");
        //丢回对象池中一个对象
        bullet.Recycle2Cache();

        Debug.Log($"BulletPool.CurCount:{ SafeObjectPool<Bullet>.Instance.CurCount}");
    }

    // Update is called once per frame



}
