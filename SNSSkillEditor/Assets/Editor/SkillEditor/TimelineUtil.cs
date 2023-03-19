using System;
using System.Collections;
using System.Reflection;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SkillEditor
{
    public static class TimelineUtil
    {
        public static TimelineClip CreateTimelineClip(TrackAsset trackAsset, Type clipType)
        {
            var createClipMethod = trackAsset.GetType().GetMethod("CreateClip", BindingFlags.Instance | BindingFlags.NonPublic);
            return (TimelineClip)createClipMethod.Invoke(trackAsset, new[] {clipType});
        }

        //清理Director并重绘Timeline窗口
        public static void InitTimeline(PlayableDirector director)
        {
            //清除掉旧的ExposedReference和SceneBinding
            SerializedObject serializedObject = new SerializedObject(director);
            SerializedProperty prop = serializedObject.FindProperty("m_ExposedReferences");
            var reference = prop.FindPropertyRelative("m_References");
            reference.ClearArray();
            prop = serializedObject.FindProperty("m_SceneBindings");
            prop.ClearArray();
            serializedObject.ApplyModifiedProperties();
            
            //重绘新的Timeline窗口
            var timelineWindow = TimelineEditor.GetOrCreateWindow();
            Selection.activeObject = director.gameObject;
            timelineWindow.Show(true);
            timelineWindow.Focus();
            timelineWindow.SetTimeline(director);
            timelineWindow.Repaint();
            timelineWindow.locked = true;
            EditorCoroutineUtility.StartCoroutine(SetTimeRange(timelineWindow), new object());
        }
        
        // 这个应该是解决 Clip长度闪烁的问题
        private static IEnumerator SetTimeRange(TimelineEditorWindow timelineWindow)
        {
            var timelineEditorWindowType = typeof(TimelineEditorWindow);
            var assembly = timelineEditorWindowType.Assembly;
            var type = assembly.GetType("UnityEditor.Timeline.FrameSelectedAction");
            var method = type.GetMethod("FrameRange", BindingFlags.Static | BindingFlags.Public);
            var timelineWindowType = timelineWindow.GetType();
            var timeArea = timelineWindowType.GetField("m_TimeArea", BindingFlags.Instance | BindingFlags.NonPublic);
            while (true)
            {
                if (timeArea?.GetValue(timelineWindow) != null)
                {
                    method?.Invoke(null, new[] {(object) 0, (object) SkillEditorSettings.DefaultTimeRange});
                    yield break;
                }
                yield return null;
            }
        }
        
    }
}