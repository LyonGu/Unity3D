using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class TimeMachineClip : PlayableAsset, ITimelineClipAsset
{
	[HideInInspector]
    public TimeMachineBehaviour template = new TimeMachineBehaviour ();

	public TimeMachineBehaviour.TimeMachineAction action;
	public TimeMachineBehaviour.Condition condition;
	public string markerToJumpTo = "", markerLabel = "";
	public float timeToJumpTo = 0f;

	public ExposedReference<Platoon> platoon; //如果要使用场景里的组件或者gameObject 必须使用ExposedReference

    public ClipCaps clipCaps
    {
        get { return ClipCaps.None; }
    }

    public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<TimeMachineBehaviour>.Create (graph, template);
        TimeMachineBehaviour clone = playable.GetBehaviour ();
        clone.platoon = platoon.Resolve (graph.GetResolver ());  //通用写法 xxx.Resolve (graph.GetResolver ()) 场景中的物体需要这么写
        clone.markerToJumpTo = markerToJumpTo;
		clone.action = action;
		clone.condition = condition;
		clone.markerLabel = markerLabel;
		clone.timeToJumpTo = timeToJumpTo;

        return playable;
    }
}
