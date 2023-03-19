using System.ComponentModel;
using UnityEngine.Timeline;

namespace SkillEditor.Timeline
{
    [DisplayName("技能编辑器/Damage Text")]
    [BindSkillTrack(typeof(DamageText))]
    [TrackClipType(typeof(DamageTextClip))]
    public class DamageTextTrack : BaseTrack<DamageTextBehaviour>
    {
        
    }
}