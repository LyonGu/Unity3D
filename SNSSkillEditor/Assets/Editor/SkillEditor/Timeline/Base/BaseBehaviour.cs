#if UNITY_EDITOR

using System.Linq;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SkillEditor.Timeline
{
    public class BaseBehaviour : PlayableBehaviour
    {
        public TimelineClip clip;
        
        public override void OnPlayableCreate(Playable playable)
        {
            //Debug.Log("OnPlayableCreate");
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            //Debug.Log("ProcessFrame");
        }

        public float GetTimeInClip(Playable playable)
        {
            double time = -1;
            double clipStartTime = clip.start;
            var director = playable.GetGraph().GetResolver() as PlayableDirector;
            if (director != null)
            {
                time = director.time - clipStartTime;
            }

            return (float) time;
        }

        public ItemBase GetData()
        {
            var baseClip = clip.asset;
            var data = baseClip.GetType().GetField("data").GetValue(baseClip);
            return (ItemBase) data;
        }
        


        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            //Debug.Log("OnBehaviourPlay");
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            //Debug.Log("OnBehaviourPause");
            var curTime = GetTimeInClip(playable);
            bool inClip = curTime >= 0 && curTime <= clip.duration;
            OnBehaviourPauseExt(playable, info, inClip);
        }

        protected virtual void OnBehaviourPauseExt(Playable playable, FrameData info, bool inClip)
        {
            //Debug.Log("OnBehaviourPauseExt");
        }
        
        public override void OnGraphStart(Playable playable)
        {
            
        }

        public override void PrepareData(Playable playable, FrameData info)
        {
            //Debug.Log("PrepareData");
            base.PrepareData(playable, info);
        }

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            //Debug.Log("PrepareFrame");
            base.PrepareFrame(playable, info);
        }

        public override void OnGraphStop(Playable playable)
        {
            //Debug.Log("OnGraphStop");
            base.OnGraphStop(playable);
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            //Debug.Log("OnPlayableDestroy");
            base.OnPlayableDestroy(playable);
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
            var track = clip.GetParentTrack().GetGroup();
            if (track != null)
            {
                return track.name;
            }

            return "";
        }
    }
    
}

#endif