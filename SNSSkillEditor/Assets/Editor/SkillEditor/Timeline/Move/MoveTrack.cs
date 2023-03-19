using System.ComponentModel;
using UnityEngine.Timeline;

namespace SkillEditor.Timeline
{
    [DisplayName("技能编辑器/Move")]
    [BindSkillTrack(typeof(Move))]
    [TrackClipType(typeof(MoveClip))]
    public class MoveTrack : BaseTrack<MoveBehaviour>
    {
        
    }
}