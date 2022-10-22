using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility;

public class Enermy : MonoBehaviour 
{
    public WaypointCircuit spline;
    public List<Animator> animators;
    public float speed = 1;
    public int maxBlood = 1;
    public int _mBlood=100;
    public Vector3 offset;
    public Transform firePos;
    public bool isSceneEnermy = false;
    Shadow m_shadow;
    public Shadow shadow
    {
        get {
            if (m_shadow == null)
                m_shadow = GetComponentInChildren<Shadow>();
                return m_shadow;
        }
    }
    public int blood
    {
        get { return _mBlood; }
        set{
            _mBlood = value;
            if (_mBlood <= 0)
            {
                SoundManager.PlaySound("EXPLOSION_Medium_Implosion_with_Bright_Overlay_stereo");
                Instantiate(Resources.Load<GameObject>("Explosion01"), transform.position, transform.rotation);
                EnermyDead();
            }
        }
    }
    public void Init()
    {
        SetDefaultAni();
        if(shadow!=null)
        shadow.ShadowInit();
    }
    [HideInInspector]
    public CreateEnermyNode enermyNode;

    public void Start()
    {
        blood = maxBlood;
    }
    float fireTimes = 0;
    public float distance = 0;
    bool isShoot = true;
    public bool isLoopShoot = true;
    void Update()
    {
        if (enermyNode._mPlane.bulletNode != null &&isShoot)
        {
            fireTimes += Time.deltaTime;
            if (fireTimes >= enermyNode._mPlane.delayShootTime)
            {
                fireTimes = 0;
                Fire();
                if(!isLoopShoot)
                isShoot = false;
            }            
        }
        distance += Time.deltaTime* speed;
        Vector3 pos= spline.GetRoutePoint(distance).position;
        transform.position = pos+ offset;
        Vector3 angle = Quaternion.LookRotation(spline.GetRoutePoint(distance).direction ,Vector3.right).eulerAngles;
        transform.eulerAngles = new Vector3(0, 0, spline.GetRoutePoint(distance).angle);

        if (distance >= spline.distances[spline.distances.Length - 2])
        {
            EndReached();
        }
    }
        public void EndReached()
    {
        EnermyDead();
    }
    void EnermyDead()
    {
        if (gameObject.active)
        {
            if (isSceneEnermy)
            {
                gameObject.SetActive(false);
            }
            else
            {
                GameObjectPool.Return(gameObject);
                if (enermyNode != null)
                    enermyNode.curEnermyCount--;
            }           
        }        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 8)
        {
            blood -= collision.GetComponent<Bullet>().power;
            GameObjectPool.Return(collision.gameObject);
        }       
    }

    public void Fire(Transform bullet = null)
    {
        firePos.LookAt2D(firePos.position.ToVector2(), Hero.Inst.transform.position.ToVector2());
        Vector3 angle = firePos.eulerAngles;
        if (firePos.childCount == 0)
        {            
            if (enermyNode != null)
                Instantiate(enermyNode._mPlane.bulletNode.bullet.transform, firePos.position, Quaternion.Euler(angle));
            else if (bullet != null)
                Instantiate(bullet, firePos.position, Quaternion.Euler(angle));
        }
        else
        {
            for (int i= 0; i < firePos.childCount;i++)
            {
                if(enermyNode!=null)
                    Instantiate(enermyNode._mPlane.bulletNode.bullet.transform, firePos.GetChild(i).position, firePos.GetChild(i).rotation);
                else if (bullet != null )
                    Instantiate(bullet, firePos.GetChild(i).position, firePos.GetChild(i).rotation);
            }
        }
    }
    void SetAnimators(string triggerName)
    {
        if (animators.Count == 0)
            return;
        foreach (var animator in animators)
        {
            animator.SetTrigger(triggerName);
        }
    }
    public void SetDefaultAni()
    {
        if (animators.Count==0)
            return;
        foreach (var animator in animators)
        {
            animator.Play("New State");
        }
    } 
}
