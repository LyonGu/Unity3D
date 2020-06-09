using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class SenderNotification : MonoBehaviour
{

    private PlayableGraph playableGraph;
    // Start is called before the first frame update
    void Start()
    {
        playableGraph = PlayableGraph.Create("SenderNotification");
        var output = ScriptPlayableOutput.Create(playableGraph, "NotificationOutput");
        var receiver = new ReciverExample();
        output.AddNotificationReceiver(receiver);
        output.PushNotification(Playable.Null, new MyNotification());
        playableGraph.Play();
    }

    private void OnDestroy()
    {
        if (playableGraph.IsValid())
            playableGraph.Destroy();
    }
}
