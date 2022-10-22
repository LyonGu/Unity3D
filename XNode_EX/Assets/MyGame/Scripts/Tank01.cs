using UnityEngine;
public class Tank01 : Enermy
{
    public float delayTime = 10;
    public Transform bullet;
    public Transform head;
    public Transform shadowHead;
    float shootTime = 3;
    float times;
    public bool isHead = true;
    void Start()
    {
        if (isSceneEnermy)
        {
            Vector3 pos = spline.GetRoutePoint(0).position;
            offset = transform.position - pos;
        }
        
    }

    void Update()
    {
        if (!isHead)
            return;
        times += Time.deltaTime;
        if (times >= shootTime)
        {
            Fire(bullet);
            times = 0;
        }
        if (Hero.Inst != null)
        {
            head.LookAt2D(Hero.Inst.transform);
            shadowHead.LookAt2D(Hero.Inst.transform);
        }

        distance += Time.deltaTime;
        Vector3 pos = spline.GetRoutePoint(distance).position;
        transform.position =pos + offset;
        transform.eulerAngles = new Vector3(0, 0, spline.GetRoutePoint(distance).angle);
    }
}
