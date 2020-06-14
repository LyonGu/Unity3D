using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class DialogBehaviour : PlayableBehaviour
{
    public string characterName;
    public string content;


    public bool showDialog = false;
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        UIMgr.Instance.SetDialog(characterName, content);
        showDialog = true;
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (showDialog)
        {
            var director = playable.GetGraph().GetResolver() as PlayableDirector;
            GameMgr.Instance.PauseTimeLine(director);
        }
    }

}
