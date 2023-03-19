using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SkillEditor.Timeline
{
    public class CameraShakeClip : BaseClip<CameraShakeBehaviour>, IPropertyPreview
    {
        public void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            if(Camera.main != null)
            {
                var main = Camera.main;
                driver.AddFromComponent(main.gameObject, main.transform);
            }
        }
    }
}