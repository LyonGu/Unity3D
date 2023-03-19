using System.ComponentModel;
using UnityEngine.Timeline;

namespace SkillEditor.Timeline
{
    [DisplayName("技能编辑器/Projectile Effect")]
    [BindSkillTrack(typeof(ProjectileEffect))]
    [TrackClipType(typeof(ProjectileEffectClip))]
    [TrackColor(1,1,0)]
    public class ProjectileEffectTrack : BaseTrack<ProjectileEffectBehaviour>
    {
        
    }
}