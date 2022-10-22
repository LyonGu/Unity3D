using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Hero : GameInst<Hero>
{
    public bool isShoot = true;
    public List<Transform> firePoses;
    public Transform firePosParent;
    public Transform bullet01;
    public Transform bullet02;
    public Transform firePos1;
    public Transform firePos2;
    public Transform firePos3;
    float speed = 2;
    float heroWidth = 0.3f;
    float heroHeight = 0.2f;
    void Start()
    {
        createShootTime = shortShootTime;
        for (int i = 0; i < firePosParent.childCount; i++)
        {
            for (int j = 0; j < firePosParent.GetChild(i).childCount; j++)
            {
                firePoses.Add(firePosParent.GetChild(i).GetChild(j));
            }
        }
    }
    float times;
    public float createShootTime = 0.02f;
    float shortShootTime = 0.05f;
    float longShootTime = 0.15f;
    int shootCount=0;
    private Vector3 cTP = new Vector3();
    private Vector3 sTP = new Vector3();
    private Vector3 sTfP = new Vector3();

    void Update()
    {
        if (Input.touchCount == 1)
        {
            Debug.LogError("Touch");
            Touch cT = Input.touches[0];
            if (cT.phase == TouchPhase.Moved)
            {
                cTP = Camera.main.ScreenToWorldPoint(cT.position);
                transform.position = new Vector3(
                    cTP.x - sTP.x + sTfP.x,
                    cTP.y - sTP.y + sTfP.y,
                    sTfP.z);
            }
            else if (cT.phase == TouchPhase.Began)
            {
                sTP = Camera.main.ScreenToWorldPoint(cT.position);
                sTfP = transform.position;
            }
        }
            if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.right * Time.deltaTime * -speed);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * Time.deltaTime * speed);
        }
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(Vector3.up * Time.deltaTime * speed);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(Vector3.up * Time.deltaTime * -speed);
        }
        if (transform.position.x <= -GameInfo.screenX+heroWidth)
        {
            transform.position = new Vector3(-GameInfo.screenX + heroWidth, transform.position.y, 0);
        }else if (transform.position.x >= GameInfo.screenX- heroWidth)
        {
            transform.position = new Vector3(GameInfo.screenX - heroWidth, transform.position.y, 0);
        }

        if (transform.position.y <= -GameInfo.screenY+heroHeight)
        {
            transform.position = new Vector3(transform.position.x, -GameInfo.screenY + heroHeight, 0);
        }
        else if (transform.position.y >= GameInfo.screenY- heroHeight)
        {
            transform.position = new Vector3(transform.position.x, GameInfo.screenY - heroHeight, 0);
        }


        if(isShoot)
        { 
            times += Time.deltaTime;
            if (times >= createShootTime)
            {
                times = 0;
                foreach (var pos in firePoses)
                {
                    GameObjectPool.Get("bullet01", pos.position, pos.rotation);
                }
            
            
                if (shootCount == 0)
                {
                    createShootTime = shortShootTime;
                    SoundManager.PlaySound("BLASTER_Weak_Subtle_Distorted_stereo");
                }
                else
                if (shootCount >= 3)
                {
                    createShootTime = longShootTime;
                    shootCount = -1;
                
                }
                shootCount++;
            }
        }
        Camera.main.transform.position = new Vector3(transform.position.x * 0.3f, 0, -10);
    }
}
