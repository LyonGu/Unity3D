using System.ComponentModel;
using UnityEngine.Timeline;

namespace SkillEditor.Timeline
{
    [DisplayName("技能编辑器/Model Shake")]
    [BindSkillTrack(typeof(ModelShake))]
    [TrackClipType(typeof(ModelShakeClip))]
    public class ModelShakeTrack : BaseTrack<ModelShakeBehaviour>
    {
        
    }
}