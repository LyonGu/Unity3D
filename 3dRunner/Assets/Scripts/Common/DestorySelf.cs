/***
 *
 *  Title: "不夜城跑酷" 项目
 *         道具组-->销毁自身
 *
 *  Description:
 *        功能：
 *           1： 当英雄接触道具时，在指定时间后销毁。
 *           2： 在指定时间后自动销毁。
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



public class DestorySelf: BasePropItem
{
    //枚举定义： 销毁类型
    public enum DestroyType
    {
        //英雄碰触触发
        HeroTrigger,
        //纯时间触发
        TimeTirrer
    }

    //销毁类型
    public DestroyType DestType = DestroyType.HeroTrigger;
    //销毁自身延迟时间
    public int DestroyDelayTime = 0;




    void Awake()
    {
        if (DestType == DestroyType.HeroTrigger)
        {
            base.m_PropTriggerHandle = StartDestorySelf;
        }
    }


    //使用对象缓冲池，需要禁用Start() 方法
    //private void Start()
    //{
    //    if (DestType == DestroyType.TimeTirrer)
    //    {
    //        StartDestorySelf();
    //    }
    //}

    /// <summary>
    /// 得到奖励
    /// </summary>
    public void StartDestorySelf()
    {
        //Debug.Log(GetType() + "/销毁自身");
        base.EnableDestory(this.gameObject, DestroyDelayTime);
    }   
     

}//Class_end

