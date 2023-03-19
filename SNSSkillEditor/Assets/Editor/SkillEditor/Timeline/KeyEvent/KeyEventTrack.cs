using System.ComponentModel;
using UnityEngine.Timeline;

namespace SkillEditor.Timeline
{
    [DisplayName("技能编辑器/KeyEvent")]
    [BindSkillTrack(typeof(KeyEvent))]
    [TrackClipType(typeof(KeyEventClip))]
    public class KeyEventTrack : BaseTrack<KeyEventBehaviour>
    {
        
    }
}