#if UNITY_EDITOR

using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SkillEditor.Timeline
{
    public class ModelShakeClip : BaseClip<ModelShakeBehaviour>,IPropertyPreview
    {
        public void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            var attackerGo = SkillEditorManager.Instance.AttackerGo;
            if (attackerGo != null)
            {
                driver.AddFromComponent(attackerGo, attackerGo.transform);
            }
            var targetGo = SkillEditorManager.Instance.TargetGo;
            if (targetGo != null)
            {
                driver.AddFromComponent(targetGo, targetGo.transform);
            }
        }
    }
}

#endif