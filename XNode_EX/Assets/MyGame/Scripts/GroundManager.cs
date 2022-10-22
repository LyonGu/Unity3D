using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GroundManager : GameInst<GroundManager>
{
    public float speed = 5;
    public List<Transform> grounds;
    public Transform nextGround;
    bool changeGround = false;
    GroundNode groundNode;
    bool isMoveLastGround = false;
    void Start()
    {        
    }
    float lastSpeed;
    void Update()
    {
        transform.Translate(Vector3.down * Time.deltaTime * speed);
        if (transform.position.y <-15.36f)
        {
            transform.position = Vector3.zero;
            GameObjectPool.Return(grounds[0].gameObject);
            grounds.RemoveAt(0);
            Object ground = GameObjectPool.Get(nextGround.gameObject.name, new Vector3(0, -2.68f + 15.36f, 0), Quaternion.Euler(Vector3.zero));
            GameObject groundTrans = ground as GameObject;
            groundTrans.transform.SetParent(transform);
            grounds.Add(groundTrans.transform);
            grounds[0].transform.position = new Vector3(0, -2.68f , 1);
            if (changeGround)
            {
                changeGround = false;
                lastSpeed = speed;
                DOTween.To(() => speed, x => speed = x, groundNode.speed, 5).SetEase( Ease.Linear).OnComplete(() => {
                });
                isMoveLastGround = groundNode.isMoveLastScene;
            }
            if(groundNode!=null)
            groundNode.MoveNext();

        }
        if(isMoveLastGround)
        grounds[0].transform.Translate(Vector3.down * Time.deltaTime * (lastSpeed - speed));
    }
    public void SetGround(GroundNode groundNode)
    {
            nextGround = groundNode.ground;
            changeGround = true;
            this.groundNode = groundNode;
    }
    public void SetGroundSpeed(SetGroundSpeedNode setGroundSpeedNode)
    {

        DOTween.To(() => speed, x => speed = x, setGroundSpeedNode.speed, setGroundSpeedNode.delayTime).SetEase( Ease.Linear).OnComplete(() => {
            changeGround = true;
            setGroundSpeedNode.MoveNext();
            DOTween.To(() => speed, x => speed = x, setGroundSpeedNode.nextGroundNode().speed, 10).SetEase(Ease.Linear);
        });


    }
    public void GroundInit(GroundNode groundNode)
    {
        for (int i = 0; i < 2; i++)
        {
            grounds.Add(Instantiate(groundNode.ground, new Vector3(0, -2.68f + i * 15.36f, 1), Quaternion.Euler(Vector3.zero), transform));
        }
        nextGround = groundNode.ground;
        speed = groundNode.speed;
        lastSpeed = speed;
    }
}
