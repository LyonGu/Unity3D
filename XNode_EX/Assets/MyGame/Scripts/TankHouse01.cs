using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility;

public class TankHouse01 : MonoBehaviour
{
    public List<TankHouseBorn> tankHouseBorns;
    bool isStart=false;
    void Start()
    {
        
        }

    void Update()
    {
        if (isStart)
        {
            foreach (var born in tankHouseBorns)
            {
                switch (born.bornType)
                {
                    case BornType.Loop:
                        born.times += Time.deltaTime;
                        if (born.times >= born.createTime)
                        {
                            born.times = 0;
                            GameObject tankObj= GameObjectPool.Get(born.tank.name, born.spline.GetRoutePoint(0).position, Quaternion.Euler(Vector3.zero)) as GameObject;
                            Transform tankTrans = tankObj.transform;
                            tankTrans.SetParent(transform.parent);
                            tankObj.GetComponent<Tank01>().spline = born.spline;
                        }
                        break;
                }
                
            }
            
        }
    }
    private void OnBecameInvisible()
    {
        isStart = false;
    }
    private void OnBecameVisible()
    {
        foreach (var born in tankHouseBorns)
        {
            vp_Timer.Handle Timer = new vp_Timer.Handle();
            vp_Timer.In(born.delayTime, new vp_Timer.Callback(() => { Timer.Cancel();
            GameObject tankObj = GameObjectPool.Get(born.tank.name, born.spline.GetRoutePoint(0).position, Quaternion.Euler(Vector3.zero)/*, transform.parent*/) as GameObject;
            tankObj.transform.SetParent(transform.parent);
                tankObj.GetComponent<Tank01>().spline = born.spline;
                born.isStartLoop = true;
            }), Timer);
            isStart = true;
        }
        
    }
    
}
[System.Serializable]
public class TankHouseBorn
{

    public float delayTime;
    public Transform tank;
    public WaypointCircuit spline;
    
    public BornType bornType;
    public float createTime;
    public float times;
    public bool isStartLoop = false;
}
public enum BornType
{
    Onece,
    Loop
}
