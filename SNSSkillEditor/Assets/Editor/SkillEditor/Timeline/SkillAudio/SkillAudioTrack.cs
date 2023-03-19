using System.ComponentModel;
using UnityEngine.Timeline;

namespace SkillEditor.Timeline
{
    
    [DisplayName("技能编辑器/Skill Audio")]
    [TrackClipType(typeof(SkillAudioClip))]
    [BindSkillTrack(typeof(SkillAudio))]
    public class SkillAudioTrack : BaseTrack<SkillAudioBehaviour>
    {
        
    }
}