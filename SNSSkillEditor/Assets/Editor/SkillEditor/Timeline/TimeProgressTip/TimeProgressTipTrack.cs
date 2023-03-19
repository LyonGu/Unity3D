using System.ComponentModel;
using UnityEngine.Timeline;

namespace SkillEditor.Timeline
{
    [DisplayName("技能编辑器/读条")]
    [BindSkillTrack(typeof(TimeProgressTip))]
    [TrackClipType(typeof(TimeProgressTipClip))]
    [TrackColor(1,1,0)]
    public class TimeProgressTipTrack : BaseTrack<TimeProgressTipBehaviour>
    {
        
    }
}