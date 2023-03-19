using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SkillEditor.Timeline
{
    public class MoveClip : BaseClip<MoveBehaviour>, IPropertyPreview
    {
        public void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            var modelRoot = SkillEditorManager.Instance.ModelRoot;
            if (modelRoot != null)
            {
                foreach (var trans in modelRoot.GetComponentsInChildren<Transform>(true))
                {
                    driver.AddFromComponent(trans.gameObject, trans);
                }
            }
        }
    }
}