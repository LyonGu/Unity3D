using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PlayerEnitity:BaseEnitity  {


    public PlayerMode _mode;
    private GameObject _rootObj;
    public GameObject _gameObject;

    private List<string> _animationNameList;

    private List<BaseState> _stateList;

    private Animation _animation;

    public PlayerEnitity()
    {
        _mode = new PlayerMode();
        _mode._file = "Models/SwordsMan/GreateWarriorNew";

        _stateList = new List<BaseState>();

        
        BaseState playerIdleState = new PlayerIdleState(this);
        BaseState playerRunState = new PlayerRunState(this);
        BaseState playerAttackState = new PlayerAttackState(this);
        BaseState playerDeadState = new PlayerDeadState(this);

        _stateList.Add(playerIdleState);
        _stateList.Add(playerRunState);
        _stateList.Add(playerAttackState);
        _stateList.Add(playerDeadState);

        //状态机设置
        //changeState(playerIdleState);
        _stateMachine.setCurrentState(playerIdleState);

        _animationNameList = new List<string>();
        
     
    }
    override public void initGameObject()
    {
        _rootObj = GameObject.Find("StartGame");
        if (_rootObj)
        {
            _gameObject = getGameObject(_mode._file, "GreateWarriorNew", _rootObj, Vector3.zero);
            _gameObject.transform.localScale = new Vector3(20.0f, 20.0f, 20.0f);
            _gameObject.transform.eulerAngles = new Vector3(0.0f, 180.0f, 0);

        }

        //动作添加
        if (_gameObject != null)
        {
            AddAinimainClips();
            Animation animation = _gameObject.GetComponent<Animation>();
            if(animation == null)
            {
                _gameObject.AddComponent<Animation>();
                animation = _gameObject.GetComponent<Animation>();
            }

            _animation = animation;
            foreach(string clipName in _animationNameList)
            {
                string path = "Models/SwordsMan/SwordsManResources/Animations/StoneKing@" + clipName;
                AnimationClip clip = Resources.Load<AnimationClip>(path);
                _animation.AddClip(clip, clipName);
            }
            changeAniamtion(_animationNameList[0], 1.0f, true);
            //测试代码
            addBtnListener();
        }
    }


    public void AddAinimainClips()
    {
        _animationNameList.Add("Idle");
        _animationNameList.Add("Run");
        _animationNameList.Add("Attack2");
        _animationNameList.Add("Death");
    }

    public void onClick(Button btn)
    { 
        //点击按钮切换不同的状态
        string name = btn.name;
        int stateIndex = 0;
        for (int i = 0; i < _animationNameList.Count; i++)
        {
            if (name.Equals(_animationNameList[i]))
            {
                stateIndex = i;

                break;
            }
        }


        //切换状态
        BaseState state = _stateList[stateIndex];
        changeState(state);

        //切换动作
        string animatinName = name;
        changeAniamtion(animatinName);
        
    }


    public void changeAniamtion(string animatinName, float speed = 1.0f, bool isLoop = false)
    {

        /*
         * AnimationClip:
                frameRate  帧率
         *      length     长度（秒）
         *      AddEvent 添加帧事件
         *      
         * 
         * AnimationState：
         *      clip
         *      length 长度（秒）
         *      speed  播放倍速
         *      time  好像可以回放 。。。不确定
         
         */
        if (_animation != null)
        {
            //_animation.CrossFade(animatinName);

            foreach (AnimationState state in _animation)
            {
                if (animatinName.Equals(state.name))
                {
                    state.speed = speed;
                    AnimationClip clip = state.clip;

                    if (isLoop)
                    {
                        //循环播放
                        _animation.wrapMode = WrapMode.Loop;
                    }
                    else
                    {
                        _animation.wrapMode = WrapMode.Once;
                    }

                    
                    _animation.CrossFade(animatinName);
                    break;
                }
            }

        }


        
    }


    //以下是测试代码
    public void addBtnListener()
    {
        GameObject canvas = GameObject.Find("Canvas");
        //拿到该对象上（包括子对象）所有的按钮
        Button[] btns = canvas.GetComponentsInChildren<Button>();

        foreach (Button btn in btns)
        {
            btn.onClick.AddListener(delegate()
            {
                this.onClick(btn);
            });
        }

    }

}
