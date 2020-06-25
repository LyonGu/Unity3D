using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class TimeLineControlBehaviour : PlayableBehaviour
{
    public MarkerType markType;
    public string markerName;
    public override void OnGraphStart (Playable playable)
    {
        
    }
}

public enum MarkerType:byte
{
    Mark,
    JumpToMark,

}
