using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SkillEditor.Timeline
{
    public class EffectParticleBehaviour : EffectBehaviourBase
    {
        
        const float kUnsetTime = float.MaxValue;
        float m_LastPlayableTime = kUnsetTime;
        float m_LastParticleTime = kUnsetTime;
        uint m_RandomSeed = 1;
        
        public static ScriptPlayable<EffectParticleBehaviour> Create(PlayableGraph graph, ParticleSystem component, uint randomSeed, TimelineClip timelineClip)
        {
            if (component == null)
                return ScriptPlayable<EffectParticleBehaviour>.Null;

            var handle = ScriptPlayable<EffectParticleBehaviour>.Create(graph);
            handle.GetBehaviour().Initialize(component, randomSeed, timelineClip);
            return handle;
        }
        
        public ParticleSystem particleSystem { get; private set; }
        
        public void Initialize(ParticleSystem ps, uint randomSeed, TimelineClip timelineClip)
        {
            clip = timelineClip;
            m_RandomSeed = Math.Max(1, randomSeed);
            particleSystem = ps;
            SetRandomSeed(particleSystem, m_RandomSeed);
        }
        
        static void SetRandomSeed(ParticleSystem particleSystem, uint randomSeed)
        {
            if (particleSystem == null)
                return;

            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (particleSystem.useAutoRandomSeed)
            {
                particleSystem.useAutoRandomSeed = false;
                particleSystem.randomSeed = randomSeed;
            }

            for (int i = 0; i < particleSystem.subEmitters.subEmittersCount; i++)
            {
                SetRandomSeed(particleSystem.subEmitters.GetSubEmitterSystem(i), ++randomSeed);
            }
        }
        
        public override void PrepareFrame(Playable playable, FrameData data)
        {
            if (particleSystem == null || !particleSystem.gameObject.activeInHierarchy)
            {
                m_LastPlayableTime = kUnsetTime;
                return;
            }

            var time = (float)playable.GetTime();
            var particleTime = particleSystem.time;
            
            if (m_LastPlayableTime > time || !Mathf.Approximately(particleTime, m_LastParticleTime))
                Simulate(time, true);
            else if (m_LastPlayableTime < time)
                Simulate(time - m_LastPlayableTime, false);

            m_LastPlayableTime = time;
            m_LastParticleTime = particleSystem.time;
        }
        
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            m_LastPlayableTime = kUnsetTime;
        }
        
        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            m_LastPlayableTime = kUnsetTime;
        }
        
        private void Simulate(float time, bool restart)
        {
            const bool withChildren = false;
            const bool fixedTimeStep = false;
            float maxTime = Time.maximumDeltaTime;

            var data = (Effect) GetData();
            // if (data.isAttachAnim)
            // {
            //     var speed = SkillEditorCtrl.GetCurrentSpeed(clip);
            //     time *= speed;
            // }
               

            if (restart)
                particleSystem.Simulate(0, withChildren, true, fixedTimeStep);
            
            while (time > maxTime)
            {
                particleSystem.Simulate(maxTime, withChildren, false, fixedTimeStep);
                time -= maxTime;
            }

            if (time > 0)
                particleSystem.Simulate(time, withChildren, false, fixedTimeStep);
        }
        
    }
}