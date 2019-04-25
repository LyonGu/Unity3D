using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : BaseState {

    public PlayerState(BaseEnitity enity)
        : base(enity)
    {
       
    }
}



//玩家Idle状态
public class PlayerIdleState : PlayerState
{

    public PlayerIdleState(BaseEnitity enity):base(enity)
    {
       
    }

    override public  void enter(params object[] values)
    {
       
        
    }

    override public void excute(params object[] values)
    {
        Debug.Log("玩家excute================");
    }


    override public void exit(params object[] values)
    {
        
    }

    override public bool onMessage(BaseEnitity enitity, Message msg)
    {
        if (msg._messageId == MessageCustomType.msg1)
        {

            Dictionary<string, object> extraInfo = msg._extraInfo;

            string valus = (string)extraInfo["msgParams"];
            Debug.Log("接收到消息了"+msg._dispatchTime + " 消息ID：" + msg._messageId + " 消息内容是：" + valus);

            return true;
        }

        return false;
    }


}
