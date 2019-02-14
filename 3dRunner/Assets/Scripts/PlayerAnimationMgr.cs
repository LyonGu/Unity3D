
/***
 *
 *  Title: "3DRunner" 项目
 *         描述：主角动画管理器
 *
 *  Description:
 *         功能：
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


public class PlayerAnimationMgr : MonoBehaviour {

    //委托实例_动画播放状态
    public AnimationPlayStateHandle DelAnimationPlayState;

    //动画剪辑结构体(跑动，左转，右转，跳跃，俯身翻滚，结束)
    public AnimationClipSet Runing, TurnLeftClip, TurnRightClip, Jumping, Rolling, DeadClip;

    //英雄动画组件（播放动画）
    private Animation _HeroAnimation;


	// Use this for initialization
	void Start () {
        _HeroAnimation = this.gameObject.GetComponent<Animation>();
        //_HeroAnimation = this.GetComponent<Animation>();
        DelAnimationPlayState = Run;
	}
	
	// Update is called once per frame
	void Update () {
		if(DelAnimationPlayState != null)
        {
            DelAnimationPlayState.Invoke(); //调用事件委托
        }
	}


    //奔跑
    public void Run()
    { 
        //主角是否翻滚
        Global.IsRolling = false;

        //播放动画
        string name = Runing.AnimaClip.name;
        _HeroAnimation.Play(name);
        //动画播放速度
        _HeroAnimation[name].speed = Runing.ClipPlaySpeed;
    }

    /// <summary>
    /// 跳跃
    /// </summary>
    public void Jump()
    {
        ProcessAnimation(Jumping);
    }
    public void Roll()
    {
        //主角是否俯身翻滚
        Global.IsRolling = true;
        ProcessAnimation(Rolling);
    }

    /// <summary>
    /// 英雄结束（死亡）
    /// </summary>
    public void Dead()
    {
        _HeroAnimation.Play(DeadClip.AnimaClip.name);
    }

    /// <summary>
    /// 转向右边
    /// </summary>
    public void TurnRight()
    {
        //AudioManager.PlayAudioEffectB("slide");
        ProcessAnimation(TurnRightClip);
    }

    /// <summary>
    /// 转向左边
    /// </summary>
    public void TurnLeft()
    {
        //AudioManager.PlayAudioEffectA("slide");
        ProcessAnimation(TurnLeftClip);
    }

    //处理动画
    private void ProcessAnimation(AnimationClipSet animClipSet)
    {
        _HeroAnimation.Play(animClipSet.AnimaClip.name);

        if (_HeroAnimation[animClipSet.AnimaClip.name].normalizedTime>0.95)
        {
            //动画播放结束 切换为奔跑
            DelAnimationPlayState = Run;
        }
     
    }
}
