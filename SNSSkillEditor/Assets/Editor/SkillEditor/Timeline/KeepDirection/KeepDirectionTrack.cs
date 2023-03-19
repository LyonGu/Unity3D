using System.ComponentModel;
using UnityEngine.Timeline;

namespace SkillEditor.Timeline
{
    [DisplayName("技能编辑器/KeepDirection")]
    [BindSkillTrack(typeof(KeepDirection))]
    [TrackClipType(typeof(KeepDirectionClip))]
    public class KeepDirectionTrack : BaseTrack<KeepDirectionBehaviour>
    {
        
    }
}