using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGame : MonoBehaviour {


    private Dictionary<int, BaseEnitity> _enitityDic;

    private float _interval = 0.033f;
    private MessageDispatcher _msgDispatcher;

    private float totalTime = 1.0f; //刷新频率

    void Awake()
    {
        _enitityDic = new Dictionary<int, BaseEnitity>();
        _msgDispatcher = MessageDispatcher.getInstance();
    }
	// Use this for initialization
	void Start () {

        BaseEnitity enitity = new PlayerEnitity();
        _enitityDic.Add(enitity._id, enitity);

        //状态机设置
        BaseState state = new PlayerIdleState(enitity);
        StateMachine stateMachine = new StateMachine(enitity);
        stateMachine.setCurrentState(state);
        enitity.setStateMachine(stateMachine);
        
        //初始化显示对象
        enitity.initGameObject();
        
	}

    public void testDispatcMsg()
    { 
        Dictionary<string, object> exInfo = new Dictionary<string, object>();

        exInfo.Add("msgParams", "测试消息1========");
        //立刻发
        _msgDispatcher.dispatchMessages(5000, 0, 0, MessageCustomType.msg1, exInfo);
    }
	
	// Update is called once per frame
	void Update () {
        totalTime -= Time.deltaTime;
        if (totalTime <= 0.0f)
        {
            foreach (KeyValuePair<int, BaseEnitity> obj in _enitityDic)
            {

                int id = obj.Key;
                BaseEnitity enitity = obj.Value;
                enitity._stateMachine.update(Time.deltaTime);
            }

            //消息发送
            _msgDispatcher.dispatchDelayedMessages();

            totalTime = 1.0f;
        }
        
	}
}
