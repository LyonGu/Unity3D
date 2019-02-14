
/***
 *
 *  Title: "3DRunner" 项目
 *         描述：主角动作控制
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
using System.ComponentModel.Design.Serialization;
using UnityEngine;


// 使用这个类，必须包含CharacterController和PlayerAnimationMgr组件
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerAnimationMgr))]

public class PlayerCtrl : MonoBehaviour {

    //引用类_英雄动画管理器
    private PlayerAnimationMgr _playerAnimeMgr;

    //主角状态控制器
    private CharacterController _CC = null;

    //键盘是否按下
    private bool _IsKeyboardPress = false;

    //当前玩家方向控制(上下左右)
    private DirectionInput _CurDirectionInput = DirectionInput.None;

    //当前玩家的位置（左中右）
    private CurrentPosition _CurrentHeroPos = CurrentPosition.Middle;

    //主角移动向量
    private Vector3 _VecMoving = Vector3.zero;

    private Animation _Animation;



	// Use this for initialization
	void Start () {
        //得到引用类_英雄动画管理器
        _playerAnimeMgr = this.GetComponent<PlayerAnimationMgr>();

        //得到角色控制器
        _CC = this.GetComponent<CharacterController>();

        //主角当前跑动速度
        Global.PlayerCurRunSpeed = Global.PlayerInitRunSpeed;

        _Animation = this.GetComponent<Animation>();
	}
	
	// Update is called once per frame
	void Update () {
		//if(Global.CurrentGameState == GameState.Playing)
        {
            //键盘输入检测
            InputInfoByKeyboard();
            //判断英雄(左中右)位置,播放动画
            JudgeHeroPosition();

            //无敌状态速度加倍
            if (Global.HeroMagState == HeroMagicState.Invincible)
            {
                Global.PlayerCurRunSpeed *= 2;
            }
            else
            {
                Global.PlayerCurRunSpeed = Global.PlayerInitRunSpeed;
            }

            //英雄前进移动处理，包含（跳跃、翻滚等动作处理）
            MoveForwardProcess();

        }
	}

    //键盘输入检测
    private void InputInfoByKeyboard()
    { 
        if(Input.anyKeyDown)
        {
            _IsKeyboardPress = true;
        }

        if (_IsKeyboardPress)
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space))
            {
                _CurDirectionInput = DirectionInput.Up;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                _CurDirectionInput = DirectionInput.Down;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                _CurDirectionInput = DirectionInput.Left;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                _CurDirectionInput = DirectionInput.Right;
            }

            _IsKeyboardPress = false;
        }
        else
        {
            _CurDirectionInput = DirectionInput.None;
        }
    
    }


    //判断英雄位置(左中右),播放移动动画
    private void JudgeHeroPosition()
    {
        switch(_CurrentHeroPos)
        {
            case CurrentPosition.Middle:
                //英雄右边->中间跳
                if (_CurDirectionInput == DirectionInput.Left)
                {
                    //主角是否在地上
                    if (_CC.isGrounded)
                    {
                        //停止当前动画播放
                        _Animation.Stop();

                        //定义播放动画的委托， PlayerAnimationMgr里会自动调用
                        //播放“左移”动画                    
                        _playerAnimeMgr.DelAnimationPlayState = _playerAnimeMgr.TurnLeft;
                    }

                    _CurrentHeroPos = CurrentPosition.Left;
                    //播放移动音频
                    //todo....
                }
                //英雄左边位置-->中间位置
                else if (_CurDirectionInput == DirectionInput.Right)
                {
                    if (_CC.isGrounded)
                    {
                        //停止当前动画播放
                        _Animation.Stop();
                        //播放“右移”动画                    
                        _playerAnimeMgr.DelAnimationPlayState = _playerAnimeMgr.TurnRight;
                    }
                    _CurrentHeroPos = CurrentPosition.Right;
                    //播放移动音频
                    //todo....
                }
                //插值位移处理
                transform.position = Vector3.Lerp(transform.position,
                    new Vector3(Global.HeroZeroPos.x, transform.position.y, transform.position.z), Global.HeroLerpMultipe * Time.deltaTime);
                break;
            case CurrentPosition.Left:
                //左边->中间
                if (_CurDirectionInput == DirectionInput.Right)
                {
                    if (_CC.isGrounded)
                    {
                        _Animation.Stop();
                        _playerAnimeMgr.DelAnimationPlayState = _playerAnimeMgr.TurnRight;
                    }
                  
                    _CurrentHeroPos = CurrentPosition.Middle;
                    //播放移动音频
                    //todo....
                }
                //插值位移处理
                transform.position = Vector3.Lerp(transform.position,
                    new Vector3(Global.LeftTrackX, transform.position.y, transform.position.z), Global.HeroLerpMultipe * Time.deltaTime);

                break;
            case CurrentPosition.Right:
                //英雄右边位置-->中间位置
                if (_CurDirectionInput == DirectionInput.Left)
                {
                    if (_CC.isGrounded)
                    {
                        //停止当前动画播放
                        _Animation.Stop();
                        //播放“左移”动画                    
                        _playerAnimeMgr.DelAnimationPlayState = _playerAnimeMgr.TurnLeft;
                    }
                    _CurrentHeroPos = CurrentPosition.Middle;
                    //播放移动音频
                    //todo....
                }

                //插值位移处理
                transform.position = Vector3.Lerp(transform.position,
                    new Vector3(Global.RightTrackX, transform.position.y, transform.position.z), Global.HeroLerpMultipe * Time.deltaTime);
                break;
            default:
                break;
        }
    }

    //英雄前进移动处理，包含（跳跃、翻滚等动作处理）
    private void MoveForwardProcess()
    {
        //地面
        if (_CC.isGrounded)
        {
            _VecMoving = Vector3.zero;
            switch (_CurDirectionInput)
            {
                case DirectionInput.Up:
                    //播放“跳跃”动画                    
                    _playerAnimeMgr.DelAnimationPlayState = _playerAnimeMgr.Jump;
                    _VecMoving.y += Global.JumpPower;
                    //“跳跃”音频
                    //todo...
                    break;
                case DirectionInput.Down:
                    //停止当前动画播放
                    _Animation.Stop();
                    //播放“俯身翻滚”动画                    
                    _playerAnimeMgr.DelAnimationPlayState = _playerAnimeMgr.Roll;
                    //“俯身翻滚”音频
                    //todo...
                    break;
            }
        }
        //空中
        else
        {
            if (_CurDirectionInput == DirectionInput.Down)
            {
                //快速下落
                _VecMoving.y -= Global.Gravity * Global.HeroLerpMultipe;
            }
        }

        /* 综合移动处理 */
        _VecMoving.z = 0;
        _VecMoving += this.transform.TransformDirection(Vector3.forward * Global.PlayerCurRunSpeed);
        //重力模拟
        _VecMoving.y -= Global.Gravity * Time.deltaTime;
        //角色控制器移动
        _CC.Move(_VecMoving * Time.deltaTime);
    }
}
