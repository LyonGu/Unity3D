using System.ComponentModel;
using UnityEngine;
using UnityEngine.Timeline;

namespace SkillEditor.Timeline
{
    [DisplayName("技能编辑器/Effect")]
    [TrackClipType(typeof(EffectClip))]
    [BindSkillTrack(typeof(Effect))]
    [TrackColor(0,0,1)]
    public class EffectTrack : BaseTrack<EffectBehaviourBase>
    {
        
    }
}