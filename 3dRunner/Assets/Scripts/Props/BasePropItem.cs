
/***
 *
 *  Title: "3DRunner" 项目
 *         描述：
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
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BasePropItem : MonoBehaviour {


    //道具出发委托
    public Action m_PropTriggerHandle;

    /// 道具（接触英雄）触发检测
    protected void OnTriggerEnter(Collider col)
    { 
        if(col.tag.Equals(Global.HeroTagName))
        {
            if (m_PropTriggerHandle!=null)
            {
                m_PropTriggerHandle.Invoke();
            }
        }
    }


    /// <summary>
    /// 销毁自身
    /// </summary>
    /// <param name="destoryTarget">销毁目标对象</param>
    /// <param name="destoryDelay">销毁延迟时间</param>
    protected void EnableDestory(GameObject destroyTarget, int destoryDelay = 0)
    { 
        if(destroyTarget!= null)
        {
            Destroy(destroyTarget, destoryDelay);
        }
    }


    /// <summary>
    /// 启动粒子特效
    /// 备注：1秒后自动销毁
    /// </summary>
    /// <param name="particlePrefab">粒子预设</param>
    /// <param name="displayPos">显示方位</param>
    /// <param name="parentNode">父节点</param>

    protected void EnableParticleEffect(GameObject particlePrefab, Vector3 displayPos, Transform parentNode = null)
    {
        ClonePrefabs(particlePrefab, displayPos, parentNode, 1);
    }


    /// <summary>
    /// 克隆游戏对象预设
    /// </summary>
    /// <param name="goPrefab">游戏对象预设体</param>
    /// <param name="displayPos">显示方位</param>
    /// <param name="parentNode">父对象节点(默认无父对象)</param>
    /// <param name="destoryTime">销毁时间(默认不销毁)</param>
    public void ClonePrefabs(GameObject goPrefab, Vector3 displayPos, Transform parentNode = null, int destoryTime = 0,string name = "")
    {
        //参数检查
        if (goPrefab == null || displayPos == Vector3.zero)
        {
            Debug.LogError(GetType() + "/EnableParticleEffect()/goPrefab==null Or displayPos==null ! 请检查错误");
        }


        GameObject goClone = Instantiate(goPrefab, displayPos,Quaternion.identity);
        if (!name.Equals(""))
        {
            goClone.name = name;
        }
        if (parentNode!=null)
        {
            //父节点定义
            goClone.transform.parent = parentNode;
        }

        //定义粒子预设的相对位移
        goClone.transform.localPosition = new Vector3(goClone.transform.localPosition.x, goClone.transform.localPosition.y , goClone.transform.localPosition.z);
        //定时销毁(1秒定时)
        if (destoryTime > 0)
        {
            EnableDestory(goClone, destoryTime);
        }
    }


    /// <summary>
    /// 得到指定范围的随机整数
    /// </summary>
    /// <param name="minNum">最小数值</param>
    /// <param name="MaxNum">最大数值</param>
    /// <returns></returns>
    public int GetRandomNum(int minNum, int maxNum)
    {
        int randomNumResult = 0;

        if (minNum == maxNum)
        {
            randomNumResult = minNum;
        }
        randomNumResult = UnityEngine.Random.Range(minNum, maxNum + 1);
        return randomNumResult;
    }

    /// <summary>
    /// 概率生成算法
    /// </summary>
    /// <param name="number">概率数值（1/number）</param>
    /// <returns>
    /// true: 得到概率
    /// false; 没有得到
    /// </returns>
    public bool GetProbability(int number)
    {
        bool boolResult = false;
        int randomNum = 999;

        randomNum = GetRandomNum(0, number);
        if (randomNum == 0)
        {
            boolResult = true;
        }

        return boolResult;
    }

	
}
