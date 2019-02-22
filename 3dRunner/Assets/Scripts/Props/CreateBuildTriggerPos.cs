/***
 *
 *  Title: "不夜城跑酷" 项目
 *         道具组-->生成建筑物触发检测点
 *
 *  Description:
 *        功能：[本脚本的主要功能描述] 
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


namespace CityNightParkour
{
    public class CreateBuildTriggerPos : BasePropItem
    {
        void Awake()
        {
            base.m_PropTriggerHandle= CreateBuilding;
        }

        /// <summary>
        /// 允许创建建筑群
        /// </summary>
        public void CreateBuilding()
        {
            //Debug.Log(GetType()+"/发现英雄，继续生成建筑物预设");
            Global.IsCreateBuildings = true;            
        }

    }//Class_end
}
