using System.ComponentModel;
using UnityEngine.Timeline;

namespace SkillEditor.Timeline
{
    [DisplayName("技能编辑器/Alert")]
    [BindSkillTrack(typeof(Alert))]
    [TrackClipType(typeof(AlertClip))]
    public class AlertTrack : BaseTrack<AlertBehaviour>
    {
        
    }
}