using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SkillEditor.Timeline
{
    public class SkillTimelineAsset : TimelineAsset
    {
        private Dictionary<string, List<ItemBase>> skillData = new Dictionary<string, List<ItemBase>>();

        public void RecordItemBase(string groupTrackName, ItemBase data)
        {
            if (groupTrackName != null)
            {
                skillData[groupTrackName].Add(data);
            }
        }
        
        public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
        {
            skillData.Clear();
            skillData["attacker"] = new List<ItemBase>();
            skillData["target"] = new List<ItemBase>();
            
            var ret = base.CreatePlayable(graph, go);
            SkillEditorManager.Instance.SyncSelectedSkillStage(skillData);
            return ret;
        }

        // public void MarkTimelineChange()
        // {
        //     OnTimelineChange();
        // }
        //
        // async void OnTimelineChange()
        // {
        //     await Task.Delay(100);
        //     if (skillData == null)
        //     {
        //         return;
        //     }
        //     skillData.Clear();
        //     
        //     var groupTracks = GetRootTracks().Where(t => t is GroupTrack);
        //     foreach (var groupTrack in groupTracks)
        //     {
        //         skillData.Add(groupTrack.name, new List<ItemBase>());
        //     }
        //     
        //     var outPutTracks = GetOutputTracks();
        //     foreach (var track in outPutTracks)
        //     {
        //         if (track.name == "Marker")
        //         {
        //             continue;
        //         }
        //         var groupTrack = track.GetGroup();
        //         if (groupTrack != null)
        //         {
        //             List<ItemBase> skillClips = skillData[groupTrack.name];
        //             foreach (var timelineClip in track.GetClips())
        //             {
        //                 var baseClip = timelineClip.asset;
        //                 ItemBase data = (ItemBase) baseClip.GetType().BaseType.GetField("data").GetValue(baseClip);
        //                 var dataType = data.GetType();
        //                 var newData = (ItemBase)SkillEditorUtil.CloneObject(data, dataType);
        //                 skillClips.Add(newData);
        //             }
        //         }
        //     }
        //     
        //     SkillEditorManager.Instance.RebindAnimator();
        // }
    }
}