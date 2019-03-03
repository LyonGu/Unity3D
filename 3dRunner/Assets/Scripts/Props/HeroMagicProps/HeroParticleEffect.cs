
/***
 *
 *  Title: "3DRunner" 项目
 *         描述：道具组-->英雄粒子特效
 *
 *  Description:
 *        功能：
 *        1： 英雄接触“魔法特效道具”后，身体出现各种对应“粒子特效”。
 *        2： 英雄接触“魔法特效道具”后，各种特效处理。
 *        注意：本脚本必须挂载在英雄上。
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
using UnityEngine.UI;

public class HeroParticleEffect : BasePropItem{

    //魔法生效时间段数值
    public int IntervalTime = 3;

    //粒子特效预设体
    public GameObject GoParticlePrefab_CollectFlash = null;
    public GameObject GoParticlePrefab_Magnet = null;
    public GameObject GoParticlePrefab_ScoreDouble = null;
    public GameObject GoParticlePrefab_SpeedDouble = null;

    //英雄对魔法道具的触发检测
    void OnTriggerEnter(Collider col)
    { 
        if(col.tag.Equals(Global.PropTagName))
        {
            //吸引魔法
            //吸引魔法
            if (col.gameObject.name.Equals(Global.Item_Magnet))
            {
                //print("吸引魔法");
                StartEffect(GoParticlePrefab_Magnet,new Vector3(0,1,0));
                ProcessMagnetItem();
            }
            //加倍分数魔法
            else if (col.gameObject.name.Equals(Global.Item_Multiply))
            {
                //print("加倍魔法");
                StartEffect(GoParticlePrefab_ScoreDouble, new Vector3(0, 1, 0));
                ProcessDoubleScore();
            }
            //无敌魔法(加速)
            else if (col.gameObject.name.Equals(Global.Item_Sprint))
            {
                //print("无敌魔法");
                StartEffect(GoParticlePrefab_SpeedDouble, new Vector3(0, 1, 0));
                ProcessInvincible();
            }
            //收集奖励魔法(即：金币)
            else if (col.gameObject.name.Equals(Global.Coin))
            {
                //print("金币魔法");
                StartEffect(GoParticlePrefab_CollectFlash, new Vector3(0, 0.5F, 0));
            }
        }
    }

    private void StartEffect(GameObject particlePrefabType, Vector3 offset)
    {
        base.EnableParticleEffect(particlePrefabType, this.transform.position + offset, this.transform);
    }

    /// <summary>
    /// 加倍分数魔法
    /// </summary>
    private void ProcessDoubleScore()
    {
        Global.HeroMagState = HeroMagicState.ScoreDouble;
        StopCoroutine("ReturnInitStateByInterval");
        StartCoroutine("ReturnInitStateByInterval");
    }

    /// <summary>
    /// 处理无敌魔法
    /// </summary>
    private void ProcessInvincible()
    {
        Global.HeroMagState = HeroMagicState.Invincible;
        StopCoroutine("ReturnInitStateByInterval");
        StartCoroutine("ReturnInitStateByInterval");
    }

    /// <summary>
    /// 处理磁铁吸引魔法道具
    /// </summary>
    private void ProcessMagnetItem()
    {
        Global.HeroMagState = HeroMagicState.Magnet;
        StopCoroutine("ReturnInitStateByInterval");
        StartCoroutine("ReturnInitStateByInterval");
    }

    //经过指定时间段，回复英雄的默认魔法状态
    private IEnumerator ReturnInitStateByInterval()
    {
        yield return new WaitForSeconds(IntervalTime);
        Global.HeroMagState = HeroMagicState.None;
        StopCoroutine("ReturnInitStateByInterval");
    }


}
