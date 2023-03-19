using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SkillEditor.Timeline
{
    
    [CustomEditor(typeof(AnimationClip))]
    public class AnimationClipInspector : BaseClipInspector<AnimationBehaviour>
    {
        public override void OnInspectorGUI()
        {
            ItemBase data = Target.data;
            if (data == null)
                return;

            Animation castData = (Animation) data;

            castData.animName = EditorGUILayout.TextField("Animation Name", castData.animName);
            
            //预览AnimationClip
            SkillEditor.Timeline.AnimationClip editorAnimationClip = target as SkillEditor.Timeline.AnimationClip;
            Animator animator = SkillEditorUtil.GetAnimatorByTimelineClip(editorAnimationClip.timelineClip);
            UnityEngine.AnimationClip unityEngineClip = SkillEditorUtil.GetAnimationClipByStateName(animator, castData.animName);
            unityEngineClip = (UnityEngine.AnimationClip)EditorGUILayout.ObjectField(unityEngineClip, typeof(UnityEngine.AnimationClip), false);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+", GUILayout.Width(20)))
            {
                if (castData.speedScale == null)
                {
                    castData.speedScale = new List<SpeedStretch>();
                }
                castData.speedScale.Add(new SpeedStretch());
            }
            EditorGUILayout.LabelField("TimeBegin", GUILayout.Width(100), GUILayout.MaxWidth(200));
            EditorGUILayout.LabelField("Speed", GUILayout.Width(100), GUILayout.MaxWidth(200));
            EditorGUILayout.EndHorizontal();
            if (castData.speedScale != null)
            {
                int count = castData.speedScale.Count;
                int toRemoveIndex = -1;
                for (int i = 0; i < count; i++)
                {
                    var speedScale = castData.speedScale[i];
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        toRemoveIndex = i;
                    }
                    speedScale.timeBegin = EditorGUILayout.FloatField(speedScale.timeBegin, GUILayout.Width(100), GUILayout.MaxWidth(200));
                    speedScale.speed = EditorGUILayout.FloatField(speedScale.speed, GUILayout.Width(100), GUILayout.MaxWidth(200));
                    EditorGUILayout.EndHorizontal();
                }
                if (toRemoveIndex != -1)
                {
                    castData.speedScale.RemoveAt(toRemoveIndex);
                }
            }

            castData.otherElementScale = EditorGUILayout.Toggle("Other Element Scale", castData.otherElementScale);
            castData.priority = (EnumConfig.priority)EditorGUILayout.EnumPopup("Priority", castData.priority);
            castData.fadeTime = EditorGUILayout.FloatField("FadeTime", castData.fadeTime);

            Target.data = castData;
            base.OnInspectorGUI();
            if(Target != null && Target.timelineClip != null)
                Target.timelineClip.displayName = castData.animName;
            
        }
    }
}