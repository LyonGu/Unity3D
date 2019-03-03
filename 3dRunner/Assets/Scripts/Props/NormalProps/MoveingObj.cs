/***
 *
 *  Title: 
 *         道具组-->移动道具
 *
 *  Description:
 *        功能：
 *        1: 使得道具一直向前移动。
 *        2: 当道具碰触英雄，则本道具固定不动。
 *        
 *  Date: 2017
 * 
 *  Version: 1.0
 *
 *  Modify Recorder:
 *     
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class MoveingObj:BasePropItem
{
    //速度
    public float speed = 5F;

    private void Awake()
    {
        base.m_PropTriggerHandle = StopPropMoving;
    }

    void Start()
    {
        GetComponent<Rigidbody>().velocity = -transform.forward * speed;
    }

    //停止道具运动
    private void StopPropMoving()
    {
        GetComponent<Rigidbody>().isKinematic = true;
    }
}//Class_end

