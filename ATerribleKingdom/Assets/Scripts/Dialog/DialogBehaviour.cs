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
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        UIMgr.Instance.SetDialog(characterName, content);
    }

}
