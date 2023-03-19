using System.ComponentModel;
using UnityEngine.Timeline;

namespace SkillEditor.Timeline
{
    [DisplayName("技能编辑器/Adjust Direction")]
    [BindSkillTrack(typeof(AdjustDirection))]
    [TrackClipType(typeof(AdjustDirectionClip))]
    public class AdjustDirectionTrack : BaseTrack<AdjustDirectionBehaviour>
    {
        
    }
}