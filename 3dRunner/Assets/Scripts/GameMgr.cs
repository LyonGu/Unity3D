
/***
 *
 *  Title: "3DRunner" 项目
 *         描述：游戏管理器脚本
 *
 *  Description:
 *        功能：
 *        负责整个游戏状态的管理。
 *        负责游戏准备、进行中、暂停、结束等阶段管理
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
using UnityEngine.SceneManagement;
using kernal;

public class GameMgr : MonoBehaviour {

    public AudioClip AudioBG = null;

    //引用类_界面UI管理器
    public UIMgr UIMgrObj = null;

    //引用类_英雄动画管理器
    private PlayerAnimationMgr _playerAnimeMgr = null;

    //倒计时数字
    private int _IntCountdownNumber = 3;

	// Use this for initialization
	void Start () {
		
        //游戏运行状态
        Global.CurrentGameState = GameState.Prepare;
        //播放背景音乐
        AudioManager.SetAudioBackgroundVolumns(0.2F);
        AudioManager.PlayBackground(AudioBG);

        //得到UI界面管理器脚本对象
        if (UIMgrObj == null)
            Debug.LogError(GetType() + "/Start()/UIMgrObj==null, 请检查！");

        //得到英雄节点的动画脚本引用
        _playerAnimeMgr = GameObject.FindGameObjectWithTag(Global.HeroTagName).GetComponent<PlayerAnimationMgr>();
        if (_playerAnimeMgr == null)
        {
            Debug.LogError(GetType() + "/Start()/_playerAnimeMgr==null! 请检查");
        }
        //持久化取得最高分
        if (PlayerPrefs.GetInt("HighestSocre") != 0)
        {
            Global.HightestScoreNum = PlayerPrefs.GetInt("HighestSocre");
        }
        //游戏状态检查
        InvokeRepeating("CheckGameProjectState", 1F, 0.2F);
	}

    //循环检查当前游戏状态
    private void CheckGameProjectState()
    {
        switch (Global.CurrentGameState)
        {
            case GameState.Prepare:
                break;
            case GameState.Playing:
                PlayingGame();
                break;
            case GameState.Pause:
                //PauseGameState();
                break;
            case GameState.End:
                EndGame();
                break;
            default:
                Debug.LogError(GetType() + "/Update()/出错了，请检查！");
                break;
        }
    }


    /// <summary>
    /// 游戏继续
    /// </summary>
    private void ContinueGame()
    {
        Time.timeScale = 1;
        UIMgrObj.HideGamePausePanel();
        AudioManager.SetAudioBackgroundVolumns(0.2F);
        Global.CurrentGameState = GameState.Playing;
    }

    /// <summary>
    /// 开始倒计时
    /// </summary>
    private void StartTimeCountdown()
    {
        StopCoroutine("EnableTimeCountdown");
        StartCoroutine("EnableTimeCountdown");
    }

    /// <summary>
    /// 显示倒计时数字
    /// </summary>
    /// <returns></returns>
    private IEnumerator EnableTimeCountdown()
    {
        while (_IntCountdownNumber > 0)
        {
            _IntCountdownNumber -= 1;
            UIMgrObj.ShowCountdownNumber(_IntCountdownNumber);
            yield return new WaitForSeconds(1F);
        }
        _IntCountdownNumber = 3;
        UIMgrObj.HideCountdownNumber();
        Global.CurrentGameState = GameState.Playing;
    }

    //游戏进行中
    private void PlayingGame()
    {
        //前进距离的统计
        Global.RuningMileDistance += Global.PlayerCurRunSpeed * Time.deltaTime;
        //最高分数的判断
        if (Global.CurrentScoreNum > Global.HightestScoreNum)
        {
            Global.HightestScoreNum = Global.CurrentScoreNum;
        }
    }

    //游戏结束
    private void EndGame()
    {
        //播放英雄结束动画
        _playerAnimeMgr.DelAnimationPlayState = _playerAnimeMgr.Dead;
        if(Global.CurrentScoreNum >= Global.HightestScoreNum)
        {
            Global.HightestScoreNum = Global.CurrentScoreNum;
            print("存储最高分数");
            PlayerPrefs.SetInt("HighestSocre", Global.HightestScoreNum);
        }
        //显示游戏结束界面
        UIMgrObj.ShowGameOverPanel();
    }

    /// <summary>
    /// 玩家点击暂停游戏
    /// </summary>
    public void PauseGame()
    {
        Time.timeScale = 0;
        UIMgrObj.ShowGamePausePanel();
        AudioManager.SetAudioBackgroundVolumns(0.01F);
        Global.CurrentGameState = GameState.Pause;
    }

    //重启游戏
    public void Restart()
    {
        SceneManager.LoadScene("Main");
        //重置系统数值
        ResetSysValues();
    }

    //重置系统数值
    private void ResetSysValues()
    {
        Global.ZposByCurrentBuilds = 0;
        Global.CurrentScoreNum = 0;
        Global.RuningMileDistance = 0;
        Global.PlayerCurRunSpeed = Global.PlayerInitRunSpeed;
    }

    //退出游戏 :设备跟编辑器不一样 todo
    public void ExitGame()
    {
        Application.Quit();
    }

    /* 检测玩家鼠标按下 */
	void Update () {
        if (Global.CurrentGameState == GameState.Prepare)
        {
            if (Input.GetMouseButtonDown(0))
            {
                //隐藏游戏准备面板
                UIMgrObj.HideUiGamePreparePanel();
                //开始游戏倒计时
                StartTimeCountdown();
            }
        }
        else if (Global.CurrentGameState == GameState.Pause)
        {
            if (Input.GetMouseButtonDown(0))
            {
                //游戏继续
                ContinueGame();
            }
        }
	}
}
