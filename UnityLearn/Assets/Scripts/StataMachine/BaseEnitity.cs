using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public delegate void delayFunc(BaseEnitity enity);
public class DelayCall
{
    public float time;
    public int frameCount;
    public delayFunc _callBack;
    public BaseEnitity _enitity;

    public DelayCall(float t, int frameC, delayFunc callBack,BaseEnitity enitity)
    {
        time = t + GlobalParams.totalTime;
        frameCount = frameC;
        _callBack = callBack;
        _enitity = enitity;
    }
}


public static class GlobalParams{

    public static int gameObjId = 0;

    public static float interval = 1.0f/30;

    public static int frameCount = 0;
    public static float totalTime = 0.0f;

    public static List<DelayCall> _delayCallList = new List<DelayCall>();

    public static void addDelayCall(DelayCall delayCall)
    {
        _delayCallList.Add(delayCall);
        _delayCallList.Sort(compare); //按降序排
    }

    public static int compare(DelayCall obj1, DelayCall obj2)
    {
        if (obj1.time < obj2.time)
        {
            return 1;
        }
        else if (obj1.time > obj2.time)
        {
            return -1;
        }
        return 0;
    }

    public static void update(float totalTime)
    {
        int count = _delayCallList.Count;
        if (count > 0)
        {
            for (int i = count - 1; i >= 0; i--)
            {
                DelayCall delay = _delayCallList[i];
                float time = delay.time;
                if (time <= totalTime)
                {
                    delayFunc call = delay._callBack;
                    BaseEnitity enitity = delay._enitity;
                    call(enitity);
                    _delayCallList.Remove(delay);
                }
            }
        }
    }

};



/*
    实例基类

*/
public class BaseEnitity  {

    public int _id;         //唯一标识id
    public string file;     //模型路径文件或者预设路径文件

    public StateMachine _stateMachine;

    public BaseMode _mode;

    public BaseEnitity()
    {
        _id = GlobalParams.gameObjId;
        GlobalParams.gameObjId++;

        //创建状态机
        StateMachine stateMachine = new StateMachine(this);
        setStateMachine(stateMachine);

        //注册消息机制
        MessageDispatcher.getInstance().registerEntity(this);
    }

    //初始化数据
    virtual public void intDatas()
    { 
        //必须重载
    }

    public GameObject getGameObject(string prefabPath, string name, GameObject parentObj ,Vector3 pos)
    {
        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        GameObject obj = GameObject.Instantiate(prefab);
        obj.name = name;
        obj.transform.localPosition = pos;
        obj.transform.parent = parentObj.transform;
        return obj;
    }

    
    virtual public void initGameObject()
    { 
    
    }


    public void setStateMachine(StateMachine stateMachine)
    {
        _stateMachine = stateMachine;
    }

    public bool handleMessage(Message msg)
    {
        if (_stateMachine != null)
        {
             return _stateMachine.handleMessage(msg);
        }
        return false;
    }

    public void changeState(BaseState state, params object[] values)
    {
        if (_stateMachine != null)
        {
            _stateMachine.changeState(state, values);
        }
    }
}
