
/***
 *
 *  Title: "3DRunner" 项目
 *         描述：界面管理器
 *
 *  Description:
 *        功能： 负责游戏的UI界面显示
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

public class UIMgr : MonoBehaviour {

    /* 控件 */
    //当前分数
    public Text TxtCurrentScoreNum = null;
    //最高分数
    public Text TxtHightestScoreNum = null;
    //当前跑动里程
    public Text TxtMileNum = null;
    //倒计时数字贴图
    public GameObject GoCountdownNum = null;

    /* 重要面板控制 */
    //游戏准备面板
    public GameObject UiGamePrepareTip = null;
    //游戏结束面板
    public GameObject UiGameOverTip = null;
    //游戏暂停面板
    public GameObject UiGamePause = null;
    //倒计时"精灵"截图数组
    public Sprite[] _SpriteCountdownArray = null;




	// Use this for initialization
	void Start () {
        //循环调用委托
        InvokeRepeating("RunCurrentHeroInfo", 1F, 0.5F);  
	}

    /// <summary>
    /// 运行当前英雄信息（当前分数、最高分数、跑步里程）
    /// 注意：使用本脚本委托作“注册调用”。
    /// </summary>
    public void RunCurrentHeroInfo()
    {
        TxtCurrentScoreNum.text = Global.CurrentScoreNum.ToString();
        TxtHightestScoreNum.text = Global.HightestScoreNum.ToString();
        TxtMileNum.text = Mathf.RoundToInt(Global.RuningMileDistance).ToString();
    }

    /// 显示倒计时数字
    public void ShowCountdownNumber(int countdownNum = 2)
    {
        //参数合法性检查
        if (countdownNum < 0 || countdownNum >= 3)
        {
            Debug.LogError(GetType() + "/ShowCountdownNumber()/范围越界！");
            return;
        }
        GoCountdownNum.SetActive(true);
        GoCountdownNum.GetComponent<Image>().overrideSprite = _SpriteCountdownArray[countdownNum];
    }

    /// 隐藏倒计时数字
    public void HideCountdownNumber()
    {
        GoCountdownNum.SetActive(false);
    }

    /// <summary>
    /// 显示“游戏准备”面板
    /// </summary>
    public void ShowUiGamePreparePanel()
    {
        if (UiGamePrepareTip != null && !UiGamePrepareTip.activeSelf)
        {
            UiGamePrepareTip.SetActive(true);
        }
    }

    /// <summary>
    /// 隐藏“游戏准备”面板
    /// </summary>
    public void HideUiGamePreparePanel()
    {
        if (UiGamePrepareTip != null && UiGamePrepareTip.activeSelf)
        {
            UiGamePrepareTip.SetActive(false);
        }
    }

    /// <summary>
    /// 显示"游戏结束"面板
    /// </summary>
    public void ShowGameOverPanel()
    {
        if (UiGameOverTip != null && !UiGameOverTip.activeSelf)
        {
            //print("显示'游戏结束'面板");
            UiGameOverTip.SetActive(true);
        }
    }

    /// <summary>
    /// 隐藏"游戏结束"面板
    /// </summary>
    public void HideGameOverPanel()
    {
        if (UiGameOverTip != null && UiGameOverTip.activeSelf)
        {
            //print("隐藏'游戏结束'面板");
            UiGameOverTip.SetActive(false);
        }
    }

    /// <summary>
    /// 显示"游戏暂停"面板
    /// </summary>
    public void ShowGamePausePanel()
    {
        if (UiGamePause != null && !UiGamePause.activeSelf)
        {
            UiGamePause.SetActive(true);
        }
    }

    /// <summary>
    /// 隐藏"游戏暂停"面板
    /// </summary>
    public void HideGamePausePanel()
    {
        if (UiGamePause != null && UiGamePause.activeSelf)
        {
            UiGamePause.SetActive(false);
        }
    }

}
