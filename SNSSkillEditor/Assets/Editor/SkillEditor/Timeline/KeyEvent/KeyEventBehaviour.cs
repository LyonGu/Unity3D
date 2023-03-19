using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using UnityEngine.Timeline;

namespace SkillEditor.Timeline
{
    public class KeyEventBehaviour : BaseBehaviour
    {
        private Animator animator;

        private UnityEngine.AnimationClip _clip = null;

        private AnimationClipPlayable clipPlayable;
        
        private PlayableGraph _graph;
        private UnityEngine.AnimationClip animationClip
        {
            get
            {
                if (_clip == null)
                {
                    if (animator != null)
                    {
                        _clip = SkillEditorUtil.GetAnimationClipByStateName(animator, "death");
                    }
                }
                if (_clip == null)
                {
                    Debug.LogError($"找不到动画 death !!!!!");
                }

                return _clip;
            }
        }

        public override void OnGraphStart(Playable playable)
        {
            if (animator != null)
            {
                AnimationPlayableUtilities.PlayClip(animator, SkillEditorUtil.GetAnimationClipByStateName(animator, "idle"), out _graph).Pause();
            }
        }
        public override void OnGraphStop(Playable playable)
        {
            if (animator != null)
            {
                AnimationPlayableUtilities.PlayClip(animator, SkillEditorUtil.GetAnimationClipByStateName(animator, "idle"), out _graph).Pause();
            }
        }
        

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            animator = SkillEditorUtil.GetAnimatorByTimelineClip(clip);
            if (animator == null)
            {
                Debug.LogError($"animator is null. clip.displayName:{clip.displayName}");
                return;
            }
            if (animationClip != null)
            {
                clipPlayable = AnimationPlayableUtilities.PlayClip(animator, animationClip, out _graph);
                clipPlayable.Pause();
            }
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (clipPlayable.IsValid())
                clipPlayable.Pause();
            if(_graph.IsValid())
                _graph.Destroy();

            SkillEditorManager.Instance.SetCurrentSpeed(clip, 1f);
        }

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            if (clipPlayable.IsValid())
            {
                var time = GetTimeInClip(playable);
                clipPlayable.SetTime(time /** animationClip.length*/);
            }
        }
        
    }
}