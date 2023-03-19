namespace SkillEditor.Timeline
{
    public class KeyEventClip : BaseClip<KeyEventBehaviour>
    {
        public override void OnCreate()
        {
            // 仅仅是为了支持编辑器下预览。运行时只需要TimelineClip的起始时间点即可。
            // UnityEngine.Animator animator = SkillEditorUtil.GetAnimatorByTimelineClip(base.timelineClip);
            // UnityEngine.AnimationClip animationClip = SkillEditorUtil.GetAnimationClipByStateName(animator, "death");
            // timelineClip.duration = animationClip.length;
        }
    }
}