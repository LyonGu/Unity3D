using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class DialogClip : PlayableAsset, ITimelineClipAsset
{

    public DialogBehaviour Templete;
    // Start is called before the first frame update
    public ClipCaps clipCaps { get; }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<DialogBehaviour>.Create(graph, Templete);
        return playable;
    }

}
