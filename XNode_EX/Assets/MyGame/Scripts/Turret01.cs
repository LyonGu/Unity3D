using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret01 : Enermy
{
    float shootTime=1;
    float times;
    public Transform bullet;
    void Start()
    {
    }

    void Update()
    {
        transform.LookAt2D(Hero.Inst.transform);
        times += Time.deltaTime;
        if (times >= shootTime)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObjectPool.Get(bullet.name, transform.GetChild(i).position, transform.GetChild(i).rotation);
            }
            times = 0;
        }
    }
}
