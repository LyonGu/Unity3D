using System.Collections.Generic;
using SkillEditor.Timeline;
using UnityEditor;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.Timeline;

namespace SkillEditor.Timeline
{
    [MenuEntry("技能编辑器Action/Copy")]
    public class SkillTrackCopyAction : TrackAction
    {
        public override ActionValidity Validate(IEnumerable<TrackAsset> tracks)
        {
            return ActionValidity.Valid;
        }

        //拷贝一个SkillTrack到剪切板
        public override bool Execute(IEnumerable<TrackAsset> tracks)
        {
            SkillEditorManager.Instance.SkillTrackOnCopyBoard.Clear();
            if (Selection.activeObject?.GetType().BaseType.GetGenericTypeDefinition() == typeof(BaseTrack<>))
            {
                TrackAsset trackAsset = Selection.activeObject as TrackAsset;
                foreach (var timelineClip in trackAsset.GetClips())
                {
                    var baseClip = timelineClip.asset;
                    ItemBase data = (ItemBase)baseClip.GetType().GetField("data").GetValue(baseClip);
                    SkillEditorManager.Instance.SkillTrackOnCopyBoard.Add(data);
                }
                Debug.Log("Copy SkillTrack Suc");
            }
            else
            {
                Debug.Log($"Copy SkillTrack Fail. Selection.activeObject:{Selection.activeObject}");
            }
            return true;
        }

        // [TimelineShortcut("SampleTrackAction", KeyCode.H)]
        // public static void HandleShortCut(ShortcutArguments args)
        // {
        //     Invoker.InvokeWithSelectedTracks<SampleTrackAction>();
        // }
    }
}