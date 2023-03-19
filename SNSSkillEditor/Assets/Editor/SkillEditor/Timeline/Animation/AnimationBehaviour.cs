using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SkillEditor.Timeline
{
    [Serializable]
    public class AnimationBehaviour : BaseBehaviour
    {

        private Animator animator;

        private UnityEngine.AnimationClip _clip = null;

        private AnimationClipPlayable clipPlayable;

        private PlayableGraph _graph;

        UnityEngine.AnimationClip animationClip
        {
            get
            {
                Animation animData = (Animation) GetData();
                if (_clip == null)
                {
                    if (animator != null)
                    {
                        _clip = SkillEditorUtil.GetAnimationClipByStateName(animator, animData.animName);
                    }
                }

                if (_clip == null)
                {
                    Debug.LogError($"找不到动画 {animData.animName} !!!!!");
                }

                return _clip;
            }
        }

        // private List<float> absTimes = new List<float>();
        // private List<float> animTimes = new List<float>();
        //
        // private float GetAnimTime(float clipTime)
        // {
        //     var data = (Animation) GetData();
        //
        //     int count = data.speedScale.Count;
        //     float curSpeed = 1;
        //     for (var i = 0; i < count - 1; i++)
        //     {
        //         //该片段播放完整需要deltaAnimTime，那么对应的绝对时间就要话费deltaAbsTime
        //         float deltaAnimTime = data.speedScale[i + 1].timeBegin - data.speedScale[i].timeBegin;  
        //         float deltaAbsTime = deltaAnimTime * 1 / data.speedScale[i + 1].speed;
        //         animTimes.Add(deltaAnimTime);
        //         absTimes.Add(deltaAbsTime);
        //     }
        //     var lastSpeedScale = data.speedScale[count - 1];
        //     float lastAnimTime = (float)clip.duration - lastSpeedScale.timeBegin;
        //     float lastAbsTime = lastAnimTime * 1 / lastSpeedScale.speed;
        //     animTimes.Add(lastAnimTime);
        //     absTimes.Add(lastAbsTime);
        //
        //     float curAnimTime = 0;
        //     float curAbsTime = 0;
        //     int length = animTimes.Count;
        //     for (int i = 0; i < length; i++)
        //     {
        //         Debug.Log($"{i}=> absTime:{absTimes[i]}, animTime:{animTimes[i]}");
        //         if (curAbsTime + absTimes[i] < clipTime)
        //         {
        //             curAbsTime += absTimes[i];
        //             curAnimTime += animTimes[i];
        //             continue;
        //         }
        //         float rate = (clipTime - curAbsTime) / absTimes[i];
        //         float addAnimTime = animTimes[i] * rate;
        //         curAnimTime += addAnimTime;
        //         break;
        //     }
        //     Debug.Log($"clipTime:{clipTime}, curAnimTime:{curAnimTime}") ;
        //     return curAnimTime;
        //
        // }
        
        
        private AnimationCurve speedCurve = null;
        private AnimationCurve timeCurve = null;

        public void InitSpeedCurve()
        {
            var data = (Animation) GetData();
            speedCurve = new AnimationCurve();
            speedCurve.AddKey(new Keyframe(-0.0001f, 1));
            
            float lastTimePoint = 0;
            float lastSpeed = 1;
            float totalTime = 0;

            if (data.speedScale != null && data.speedScale.Count > 0)
            {
                for (int i = 0; i < data.speedScale.Count; i++)
                {
                    var scale = data.speedScale[i];
                    totalTime += (scale.timeBegin - lastTimePoint) / lastSpeed;
                    speedCurve.AddKey(new Keyframe(scale.timeBegin - 0.001f, lastSpeed));
                    speedCurve.AddKey(new Keyframe(scale.timeBegin, scale.speed));
                    lastSpeed = scale.speed;
                    lastTimePoint = scale.timeBegin;
                }
            }

            if(totalTime < clip.duration)
                speedCurve.AddKey(new Keyframe((float) clip.duration, lastSpeed));

            for (int i = 0; i < speedCurve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(speedCurve, i, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyRightTangentMode(speedCurve, i, AnimationUtility.TangentMode.Linear);
            }
        }

        public void InitTimeCurve()
        {
            var data = (Animation) GetData();
            timeCurve = new AnimationCurve();
            timeCurve.AddKey(new Keyframe(-0.0001f, 0));
            float lastTimePoint = 0;
            float lastSpeed = 1;
            float totalTime = 0;
            if (data.speedScale != null && data.speedScale.Count > 0)
            {
                for (int i = 0; i < data.speedScale.Count; i++)
                {
                    var scale = data.speedScale[i];
                    float time = (scale.timeBegin - lastTimePoint) / lastSpeed;
                    totalTime = totalTime + time;
                    timeCurve.AddKey(new Keyframe(totalTime, scale.timeBegin));
                    lastTimePoint = scale.timeBegin;
                    lastSpeed = scale.speed;
                }
            }

            if (totalTime < clip.duration)
            {
                float finalTime = ((float) clip.duration - lastTimePoint) / lastSpeed;
                timeCurve.AddKey(new Keyframe(totalTime + finalTime, (float) clip.duration));
            }
            // foreach (var timeCurveKey in timeCurve.keys)
            // {
            //     Debug.Log($"TimeCurve=> time:{timeCurveKey.time}, value:{timeCurveKey.value}");
            // }
            
            for (int i = 0; i < timeCurve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(timeCurve, i, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyRightTangentMode(timeCurve, i, AnimationUtility.TangentMode.Linear);
            }
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (animator == null)
            {
                var track = clip.GetParentTrack();
                var director = playable.GetGraph().GetResolver() as PlayableDirector;
                animator = (Animator)director.GetGenericBinding(track);
            }

            InitSpeedCurve();
            InitTimeCurve();
            if (animator != null)
            {
                if (animationClip != null)
                {
                    clipPlayable = AnimationPlayableUtilities.PlayClip(animator, animationClip, out _graph);
                    clipPlayable.Pause();
                }
            }
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (clipPlayable.IsValid())
                clipPlayable.Pause();
            if(_graph.IsValid())
                _graph.Destroy();
            
            SkillEditorManager.Instance.SetCurrentSpeed(clip, 1f);
        }

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            if (clipPlayable.IsValid())
            {
                var time = GetTimeInClip(playable);
                float realTime = timeCurve.Evaluate(time);
                clipPlayable.SetTime(realTime /** animationClip.length*/);
                
                var data = (Animation) GetData();
                if(data.otherElementScale)
                {
                    float speed = speedCurve.Evaluate(time);
                    SkillEditorManager.Instance.SetCurrentSpeed(clip, speed);
                }
            }
        }
    }
}