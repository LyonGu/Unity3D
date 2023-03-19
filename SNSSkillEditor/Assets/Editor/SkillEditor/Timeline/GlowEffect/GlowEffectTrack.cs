using System.ComponentModel;
using UnityEngine.Timeline;

namespace SkillEditor.Timeline
{
    [DisplayName("技能编辑器/Glow Effect")]
    [BindSkillTrack(typeof(GlowEffect))]
    [TrackClipType(typeof(GlowEffectClip))]
    public class GlowEffectTrack : BaseTrack<GlowEffectBehaviour>
    {
        
    }
}