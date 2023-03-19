using System.ComponentModel;
using UnityEngine.Timeline;

namespace SkillEditor.Timeline
{
    [DisplayName("技能编辑器/Adjust Pos")]
    [BindSkillTrack(typeof(AdjustPos))]
    [TrackClipType(typeof(AdjustPosClip))]
    public class AdjustPosTrack : BaseTrack<AdjustPosBehaviour>
    {
        
    }
}