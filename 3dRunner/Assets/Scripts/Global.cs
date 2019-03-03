
/***
 *
 *  Title: "3DRunner" 项目
 *         项目全局变量定义
 *
 *  Description:
 *        功能：
 *        1： 游戏状态
 *        2： 游戏速度、分数、距离等。
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

//游戏（全局）状态
public enum GameState
{
    //空
    None,
    //游戏准备       
    Prepare,
    //游戏进行中
    Playing,
    //游戏暂停
    Pause,
    //游戏结束
    End
}

//主角当前位置(中间、左边、右边)
public enum CurrentPosition
{
    Middle,
    Left,
    Right
}

//玩家方向控制(上、下、左、右)
public enum DirectionInput
{
    None,
    Up,
    Down,
    Left,
    Right
}


/// <summary>
/// 主角魔法状态
/// </summary>
public enum HeroMagicState
{
    //无
    None,
    //分数加倍特效
    ScoreDouble,
    //速度加倍冲刺无敌特效
    Invincible,
    //引力魔法特效
    Magnet
}

#region 委托定义
/// <summary>
/// 动画播放状态
/// </summary>
public delegate void AnimationPlayStateHandle();
#endregion

//结构体类型定义 自定义数据结构序列化
/// <summary>
/// 动画剪辑组
/// </summary>
[System.Serializable] //可序列化标识
public struct AnimationClipSet
{
    //动画剪辑
    public AnimationClip AnimaClip;
    //动画剪辑播放速度
    public float ClipPlaySpeed;
}
   
public class Global {
    //当前游戏状态
    public static GameState CurrentGameState = GameState.None;
    //主角魔法状态
    public static HeroMagicState HeroMagState = HeroMagicState.None;
    //主角时候俯身翻滚
    public static bool IsRolling = false;

    /* 主角数值 */
    //主角Tag 名称
    public static string HeroTagName = "Player";
    //主角初始跑动速度
    public static float PlayerInitRunSpeed = 10;
    //主角当前跑动速度        
    public static float PlayerCurRunSpeed = 10F;
    //主角当前已经跑完里程
    public static float RuningMileDistance = 0;
    //主角当前分数
    public static int CurrentScoreNum = 0;
    //主角(历史)最高分数
    public static int HightestScoreNum = 0;
    //主角重力模拟
    public static float Gravity = 15F;
    //主角弹跳力
    public static float JumpPower = 5.5F;
    //主角起始（零点）方位
    public static Vector3 HeroZeroPos = new Vector3(0F, 0F, 0F);
    //主角左跑道X数值
    public static float LeftTrackX = -1.8F;
    //主角右跑道X数值
    public static float RightTrackX = 1.8F;
    //主角左右移动插值速率        
    public static float HeroLerpMultipe = 6F;

    /* 建筑物与道具生成算法数值 */
    //是否产生建筑物
    public static bool IsCreateBuildings = false;
    //当前（最后）建筑的Z坐标数值
    public static float ZposByCurrentBuilds = 0F;
    //场景大型建筑Z轴长度
    public static float ZLengthByBuildPrefab = 32F;
    //每片道路的距离长度（代表一个[组]道具生产单位）
    public static int ZLengthEveryFloor = 6;
    //道具之间的最小间隔
    public static float IntervalOfProp = 3F;
    //金币之间的生成间隔
    public static float IntervalOfCoins = 1.3F;

    /* 小型道具数值 */
    //道具Tag名称
    public static string PropTagName = "Item";   
    //魔法道具名称

    //使用对象缓冲池后，所有魔法道具，必须使用Tag 来做判断。
    public static string Tag_Magnet = "Tag_Magnet";
    public static string Tag_Multiply = "Tag_Multiply";
    public static string Tag_Sprint = "Tag_Sprint";
    public static string Tag_Coin = "Tag_Coin";      
	
}