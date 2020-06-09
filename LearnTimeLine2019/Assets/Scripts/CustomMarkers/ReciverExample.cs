using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class ReciverExample : INotificationReceiver
{
    public void OnNotify(Playable origin, INotification notification, object context)
    {
        if (notification != null)
        {
            var time = origin.IsValid() ? origin.GetTime() : 0;
            Debug.Log($"接收到通知 {notification.GetType()}, 时间是{time}");
        }
    }
}
