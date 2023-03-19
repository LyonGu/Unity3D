using System.ComponentModel;
using UnityEngine.Timeline;

namespace SkillEditor.Timeline
{
    [DisplayName("技能编辑器/Skill Event")]
    [BindSkillTrack(typeof(SkillEvent))]
    [TrackClipType(typeof(SkillEventClip))]
    [TrackColor(1,1,0)]
    public class SkillEventTrack : BaseTrack<SkillEventBehaviour>
    {
        
    }
}