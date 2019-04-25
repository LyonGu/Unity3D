using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MessageDispatcher  {

    private static MessageDispatcher _instance;

    public Dictionary<int, BaseEnitity> _enitityDic;
    public List<Message> _delayMsgList;

    private MessageDispatcher()
    {
        _enitityDic = new Dictionary<int, BaseEnitity>();
        _delayMsgList = new List<Message>();
    }

    static public MessageDispatcher getInstance()
    {
        if (_instance == null)
        {
            _instance = new MessageDispatcher();
        }
        return _instance;
    }

   
    public void registerEntity(BaseEnitity enitity)
    {
        if (!_enitityDic.ContainsKey(enitity._id))
        {
            _enitityDic.Add(enitity._id, enitity);
        }
    }

    /*
        -- 发送消息
        -- delay<=0为立即发送 单位为毫秒
        -- 发送者ID  接收者ID  消息ID  额外信息
     */
    public void dispatchMessages(float delay, int senderID, int reviererID, MessageType msgID, Dictionary<string, object> extraInfo)
    {
        BaseEnitity sender = _enitityDic[senderID];
        BaseEnitity revier = _enitityDic[reviererID];
        if (sender == null || revier == null)
        {
            return;
        }
        DateTime time = DateTime.Now;

        

    }






	
}
