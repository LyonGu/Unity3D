/***
 *
 *  Title: 
 *         道具组-->奖励道具
 *
 *  Description:
 *        功能：
 *        英雄经过时，增加“金币”等形式的奖励。
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


public class AwardObj:BasePropItem
{
    //奖励（金币）数值
    public int ScoreNum = 1;

    void Awake()
    {
        base.m_PropTriggerHandle = GetAward;
    }

    /// <summary>
    /// 得到奖励
    /// </summary>
    public void GetAward()
    {
        if (Global.HeroMagState == HeroMagicState.ScoreDouble)
        {
            Global.CurrentScoreNum += ScoreNum * 2;
        }
        else
        {
            Global.CurrentScoreNum += ScoreNum;
        }
    }
}//Class_end
