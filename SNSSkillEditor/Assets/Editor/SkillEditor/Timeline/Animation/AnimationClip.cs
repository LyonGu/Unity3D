using System;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SkillEditor.Timeline
{

    [Serializable]
    public class AnimationClip : BaseClip<AnimationBehaviour>, IPropertyPreview
    {
        //public override ClipCaps clipCaps => ClipCaps.Blending;
        public void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            var sceneRoot = SkillEditorManager.Instance.SceneRoot;
            if (sceneRoot != null)
            {
                var modelRoot = sceneRoot.FindDirect("Model");
                if (modelRoot != null)
                {
                    foreach (var trans in modelRoot.GetComponentsInChildren<Transform>(true))
                    {
                        driver.AddFromComponent(trans.gameObject, trans);
                    }
                }
            }
        }

        public override void InitWithData()
        {
            base.InitWithData();
            var castData = (Animation) data;
            timelineClip.displayName = castData.animName;
        }
    }
}