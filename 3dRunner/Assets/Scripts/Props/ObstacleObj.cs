
/***
 *
 *  Title: "3DRunner" 项目
 *         描述：道具组-->障碍物
 *
 *  Description:
 *        功能：
 *       
 *
 *  Date: 2019
 * 
 *  Version: 1.0
 *
 *  Modify Recorder:
 *     
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleObj : BasePropItem{

    //是俯身翻滚道具吗
    public bool isRollProp = false;


    void Awake()
    {
        base.m_PropTriggerHandle = HitObstacle;
    }

    public void HitObstacle()
    { 
        //处理英雄暂时无敌状态
        if (Global.HeroMagState == HeroMagicState.Invincible)
        {
            base.EnableDestory(this.gameObject);
            return;
        }

        //是俯身翻滚道具
        if(isRollProp)
        {
            if (Global.IsRolling)
            {
                return;
            }
        }
        //普通障碍物道具
        //Debug.Log(GetType() + "/碰到障碍物，游戏结束")
        Global.CurrentGameState = GameState.End;
    }


}
