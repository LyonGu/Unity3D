using System.ComponentModel;
using UnityEngine.Timeline;

namespace SkillEditor.Timeline
{
    [DisplayName("技能编辑器/Camera Shake")]
    [BindSkillTrack(typeof(CameraShake))]
    [TrackClipType(typeof(CameraShakeClip))]
    public class CameraShakeTrack : BaseTrack<CameraShakeBehaviour>
    {
        
    }
}