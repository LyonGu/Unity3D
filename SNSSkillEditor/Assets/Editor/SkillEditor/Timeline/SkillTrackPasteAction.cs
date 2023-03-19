using System.Collections.Generic;
using SkillEditor.Timeline;
using UnityEditor;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.Timeline;

namespace SkillEditor.Timeline
{
    [MenuEntry("技能编辑器Action/Paste")]
    public class SkillTrackPasteAction : TrackAction
    {
        public override ActionValidity Validate(IEnumerable<TrackAsset> tracks)
        {
            return ActionValidity.Valid;
        }

        //拷贝一个SkillTrack到剪切板
        public override bool Execute(IEnumerable<TrackAsset> tracks)
        {
            if (SkillEditorManager.Instance.SkillTrackOnCopyBoard.Count == 0)
            {
                Debug.Log("请先Copy一个SkillTrack");
                return true;
            }
            GroupTrack groupTrack = Selection.activeObject as GroupTrack;
            if (groupTrack == null)
            {
                Debug.Log($"Paste SkillTrack Fail. 需要选中GroupTrack. Selection.activeObject:{Selection.activeObject}");
                return true;
            }
            SkillEditorManager.Instance.CreateSubTrackByPaste(groupTrack);
            Debug.Log("Paste SkillTrack Suc");
            return true;
        }

        // [TimelineShortcut("SampleTrackAction", KeyCode.H)]
        // public static void HandleShortCut(ShortcutArguments args)
        // {
        //     Invoker.InvokeWithSelectedTracks<SampleTrackAction>();
        // }
    }
}