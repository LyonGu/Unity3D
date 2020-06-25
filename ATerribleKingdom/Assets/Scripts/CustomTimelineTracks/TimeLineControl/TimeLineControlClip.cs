using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class TimeLineControlClip : PlayableAsset, ITimelineClipAsset
{
    public TimeLineControlBehaviour template = new TimeLineControlBehaviour ();

    public ClipCaps clipCaps
    {
        get { return ClipCaps.None; }
    }

    public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<TimeLineControlBehaviour>.Create (graph, template);
        TimeLineControlBehaviour clone = playable.GetBehaviour ();
        return playable;
    }
}
