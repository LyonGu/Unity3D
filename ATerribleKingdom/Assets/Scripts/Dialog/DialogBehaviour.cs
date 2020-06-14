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
    public bool pauseAfterPlay;

    private bool showDialog = false;
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        var uiInstance = UIMgr.Instance;
        if (uiInstance != null)
        {
            UIMgr.Instance.SetDialog(characterName, content);
            showDialog = pauseAfterPlay;
            UIMgr.Instance.ShowTips(pauseAfterPlay);
        }

    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (showDialog)
        {
            var director = playable.GetGraph().GetResolver() as PlayableDirector;
            GameMgr.Instance.PauseTimeLine(director);
        }
        else
        {
            var uiInstance = UIMgr.Instance;
            if (uiInstance != null)
            {
                UIMgr.Instance.HideDialog();
            }
               
        }
    }

}
