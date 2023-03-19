using System;
using UnityEditor;

namespace SkillEditor.Timeline
{
    [CustomEditor(typeof(BaseClip<>))]
    public class BaseClipInspector<T> : Editor where T : BaseBehaviour, new()
    {
        protected BaseClip<T> Target => target as BaseClip<T>;

        public override void OnInspectorGUI()
        {

            ItemBase data = Target.data;
            if (data == null)
                return;

            var clip = Target;
            if (clip == null || clip.timelineClip == null)
                return;

            data.des = EditorGUILayout.TextField("Description", data.des);
            clip.timelineClip.displayName = data.des;
            data.timeBegin = (float)clip.timelineClip.start;
            data.time = (float)clip.timelineClip.duration;
            data.trackName = clip.timelineClip.GetParentTrack().name;

            Target.data = data;
        }
    }
}