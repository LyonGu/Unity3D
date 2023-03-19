using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SkillEditor.Timeline
{
    public class EffectAnimationBehaviour : EffectBehaviourBase
    {
        
        public GameObject gameObject = null;
        public UnityEngine.Animation anim;
        
        const float kUnsetTime = float.MaxValue;
        float m_LastPlayableTime = kUnsetTime;
        float m_LastAnimaionTime = kUnsetTime;
        
        public static ScriptPlayable<EffectAnimationBehaviour> Create(PlayableGraph graph, GameObject gameObject,
            TimelineClip timelineClip)
        {
            if (gameObject == null)
                return ScriptPlayable<EffectAnimationBehaviour>.Null;

            var handle = ScriptPlayable<EffectAnimationBehaviour>.Create(graph);
            var playable = handle.GetBehaviour();
            playable.gameObject = gameObject;
            playable.clip = timelineClip;
            playable.anim = gameObject.GetComponent<UnityEngine.Animation>();
            return handle;
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            anim.enabled = false;
                m_LastPlayableTime = kUnsetTime;
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            m_LastPlayableTime = kUnsetTime;
        }

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            if (anim == null || !gameObject.activeInHierarchy)
            {
                m_LastPlayableTime = kUnsetTime;
                return;
            }

            var time = GetTimeInClip(playable);
            if(m_LastPlayableTime > time)
                Simulate(time, true);
            else if(m_LastPlayableTime < time)
                Simulate(time - m_LastPlayableTime, false);

            m_LastPlayableTime = time;
        }

        private void Simulate(float time, bool restart)
        {
            var data = (Effect) GetData();
            // if (data.isAttachAnim)
            // {
            //     var speed = SkillEditorCtrl.GetCurrentSpeed(clip);
            //     time *= speed;
            // }
            
            if (restart)
            {
                anim.clip.SampleAnimation(gameObject, 0);
                m_LastAnimaionTime = 0;
            }
            anim.clip.SampleAnimation(gameObject, m_LastAnimaionTime + time);
            m_LastAnimaionTime = m_LastAnimaionTime + time;
        }
    }
}