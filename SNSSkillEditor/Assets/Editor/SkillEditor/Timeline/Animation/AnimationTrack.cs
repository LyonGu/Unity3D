using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SkillEditor.Timeline
{
    [DisplayName("技能编辑器/Animation")]
    [TrackClipType(typeof(AnimationClip))]
    [TrackBindingType(typeof(UnityEngine.Animator))]
    [BindSkillTrackAttribute(typeof(Animation))]
    [TrackColor(1, 0, 0)]
    public class AnimationTrack: BaseTrack<AnimationBehaviour>
    {
        
    }
}