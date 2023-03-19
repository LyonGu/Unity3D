using System.ComponentModel;
using UnityEngine.Timeline;

namespace SkillEditor.Timeline
{
    [DisplayName("技能编辑器/Buff")]
    [BindSkillTrack(typeof(AddBuff))]
    [TrackClipType(typeof(BuffClip))]
    public class BuffTrack : BaseTrack<BuffBehaviour>
    {
        
    }
}