using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.855f, 0.8623f, 0.87f)]
[TrackClipType(typeof(TimeLineControlClip))]
public class TimeLineControlTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {

        var playable = ScriptPlayable<TimeLineControlMixerBehaviour>.Create(graph, inputCount);

        var behaviour = playable.GetBehaviour();

        foreach (var item in GetClips())
        {
            TimeLineControlClip clip = item.asset as TimeLineControlClip;

            if (clip.template.markType == MarkerType.Mark)
            {
                behaviour.markerDic.Add(clip.template.markerName, item.start);
            }
        }

        return playable;
    }
}
