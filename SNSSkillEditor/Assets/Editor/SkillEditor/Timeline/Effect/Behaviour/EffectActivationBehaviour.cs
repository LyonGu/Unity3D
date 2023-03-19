using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SkillEditor.Timeline
{
    public class EffectActivationBehaviour : EffectBehaviourBase
    {
        enum InitialState
        {
            Unset,
            Active,
            Inactive
        }
        
        public GameObject gameObject = null;
        InitialState m_InitialState;
        
        public static ScriptPlayable<EffectActivationBehaviour> Create(PlayableGraph graph, GameObject gameObject, TimelineClip timelineClip)
        {
            if (gameObject == null)
                return ScriptPlayable<EffectActivationBehaviour>.Null;

            var handle = ScriptPlayable<EffectActivationBehaviour>.Create(graph);
            var playable = handle.GetBehaviour();
            playable.gameObject = gameObject;
            playable.clip = timelineClip;
            
            return handle;
        }
        
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (gameObject == null)
                return;

            gameObject.SetActive(true);
            UpdatePosAndRot(true);
        }
        
        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            // OnBehaviourPause can be called if the graph is stopped for a variety of reasons
            //  the effectivePlayState will test if the pause is due to the clip being out of bounds
            if (gameObject != null && info.effectivePlayState == PlayState.Paused)
            {
                gameObject.SetActive(false);
            }
        }
        
        public override void ProcessFrame(Playable playable, FrameData info, object userData)
        {
            if (gameObject != null)
            {
                gameObject.SetActive(true);
                UpdatePosAndRot();
            }
        }
        
        public override void OnGraphStart(Playable playable)
        {
            if (gameObject != null)
            {
                if (m_InitialState == InitialState.Unset)
                    m_InitialState = gameObject.activeSelf ? InitialState.Active : InitialState.Inactive;
            }
        }
        
        public override void OnPlayableDestroy(Playable playable)
        {
            if (gameObject == null || m_InitialState == InitialState.Unset)
                return;

            gameObject.SetActive(false);
        }
        
        private void UpdatePosAndRot(bool force = false)
        {
            Effect effectData = (Effect) GetData();

            bool bExecute = false;
            if (force)
            {
                bExecute = true;
            }
            else
            {
                if (effectData.isFollow)
                    bExecute = true;
            }
            if (!bExecute) return;
            
            Transform trans = null;
            switch (effectData.attach)
            {
                case EnumConfig.attach.self_root:
                    trans = GetSelfModelRoot();
                    break;
                case EnumConfig.attach.other_root:
                    trans = GetOtherModelRoot();
                    break;
                case EnumConfig.attach.attach_node_self:
                    trans = GetSelfModelRoot();
                    trans = trans.FirstOrDefault(t => t.name.Equals(effectData.attachNodeName));
                    break;
                case EnumConfig.attach.attach_node_other:
                    trans = GetOtherModelRoot();
                    trans = trans.FirstOrDefault(t => t.name.Equals(effectData.attachNodeName));
                    break;
            }

            if (trans != null)
            {
                UnityEngine.Vector3 scaledOffset = UnityEngine.Vector3.Scale(effectData.offset, GetSelfModelRoot().localScale);
                gameObject.transform.position = trans.position + scaledOffset;
                gameObject.transform.rotation = trans.rotation * Quaternion.Euler(effectData.rotation);
            }
        }
        
        
    }
}