using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int power = 1;
    public float speed = 5;
    public Transform target;
    public float heroZ=0;
    public Vector3 targetPos;
    bool follow = true;
    public enum BulletType
    {
        Forward,
        turn
    }
    public BulletType bulletType = BulletType.Forward;
    void Start()
    {
        
    }
    public void SetInit()
    {
        switch (bulletType)
        {
            case BulletType.turn:
                follow = true;
                targetPos = Hero.Inst.transform.position;
                break;
        }
    }

    void Update()
    {
        transform.Translate(Vector3.up * Time.deltaTime * speed);
        switch (bulletType)
        {
            case BulletType.turn:
                if (follow)
                {
                    target.LookAt2D(targetPos);
                    heroZ = target.eulerAngles.z;
                    transform.eulerAngles = new Vector3(0, 0, Mathf.Lerp(transform.eulerAngles.z, heroZ, Time.deltaTime * 3));
                    if (Vector3.Distance(transform.position, targetPos) <= 1.5f)
                    {
                        follow = false;
                    }
                }
                break;
        }
        if (GameTools.IsInScreen(new Vector2(transform.position.x, transform.position.y)))
        {
        }
        else
        {
            if (gameObject.active)
            {
                GameObjectPool.Return(gameObject);
                transform.position = Vector3.zero;
            }   
        }
    }
}
