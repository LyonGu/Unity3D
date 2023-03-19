using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SkillEditor.Timeline
{
    public abstract class BaseClip<T> : PlayableAsset, ITimelineClipAsset where T : BaseBehaviour, new()
    {

        [SerializeReference]
        public ItemBase data;

        [NonSerialized] public TimelineClip timelineClip = null;


        public virtual void OnCreate()
        {
            
        }
        
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var template = Activator.CreateInstance<T>();
            template.clip = timelineClip;
            var playable = ScriptPlayable<T>.Create(graph, template);

            data.timeBegin = (float)timelineClip.start;
            data.time = (float)timelineClip.duration;

            // var skillTimelineAsset = (SkillTimelineAsset) timelineClip.GetParentTrack().timelineAsset;
            // skillTimelineAsset.MarkTimelineChange();
            return playable;
        }

        public virtual ClipCaps clipCaps => ClipCaps.None;

        public virtual void InitWithData()
        {
            timelineClip.start = data.timeBegin;
            timelineClip.duration = data.time;
            timelineClip.displayName = data.des;
            if (!string.IsNullOrEmpty(data.trackName))
                timelineClip.GetParentTrack().name = data.trackName;
            else
                data.trackName = timelineClip.GetParentTrack().name;

        }

        public Transform GetSelfModelRoot()
        {
            string trackType = GetTrackType();
            if (trackType.Equals("attacker"))
            {
                return SkillEditorManager.Instance.AttackerGo.transform;
            }
            else if (trackType.Equals("target"))
            {
                return SkillEditorManager.Instance.TargetGo.transform;
            }

            return null;
        }

        public Transform GetOtherModelRoot()
        {
            string trackType = GetTrackType();
            if (trackType.Equals("attacker"))
            {
                return SkillEditorManager.Instance.TargetGo.transform;
            }
            else if (trackType.Equals("target"))
            {
                return SkillEditorManager.Instance.AttackerGo.transform;
            }

            return null;
        }

        private string GetTrackType()
        {
            var track = timelineClip.GetParentTrack().GetGroup();
            if (track != null)
            {
                return track.name;
            }

            return "";
        }
        
    }
}