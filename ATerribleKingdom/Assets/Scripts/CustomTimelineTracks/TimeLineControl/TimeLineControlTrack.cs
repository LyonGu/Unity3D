using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.855f, 0.8623f, 0.87f)]
[TrackClipType(typeof(TimeLineControlClip))]
public class TimeLineControlTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {

        var playable = ScriptPlayable<TimeLineControlMixerBehaviour>.Create(graph, inputCount);  //获取对应的playable

        var behaviour = playable.GetBehaviour();//获取对应的behaviour, 这里就是TimeLineControlMixerBehaviour

        //GetClips() ==> 得到track上所有的clip，这里的clip类型就是TrackClipType(typeof(xxxx))定义的
        foreach (var item in GetClips())
        {
            TimeLineControlClip clip = item.asset as TimeLineControlClip;  // clip.asset 转成目标clip

            if (clip.template.markType == MarkerType.Mark)
            {
                behaviour.markerDic.Add(clip.template.markerName, item.start);
            }
        }

        return playable;
    }
}
