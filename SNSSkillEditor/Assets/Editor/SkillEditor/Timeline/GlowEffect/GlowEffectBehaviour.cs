using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

namespace SkillEditor.Timeline
{
    public class GlowEffectBehaviour : BaseBehaviour
    {
        private readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor"); // URP Lit.Shader

        private MaterialPropertyBlock block;
        private Renderer[] renders;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (block == null)
            {
                block = new MaterialPropertyBlock();
            }
            if (renders == null)
            {
                var rootTrans = GetSelfModelRoot();
                renders = rootTrans.GetComponentsInChildren<Renderer>();
                // foreach (var renderer in renders)
                // {
                //     renderer.sharedMaterial.EnableKeyword("_EMISSION");
                // }
            }
        }

        //回头需要注意一下Pause的调用时机
        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            var curTime = GetTimeInClip(playable);
            if (curTime >= 0 && curTime <= clip.duration)
            {
                // var data = (GlowEffect) GetData();
                // SetColor(data.color);
            }
            else
            {
                SetColor(Color.black);
            }
        }
        
        public override void PrepareFrame(Playable playable, FrameData info)
        {
            Color colorThisFrame = CalcColor(GetTimeInClip(playable));
            SetColor(colorThisFrame);
        }

        private Color CalcColor(float timeInClip)
        {
            var data = (GlowEffect) GetData();
            if (data.easeTime <= 0)
            {
                return data.color;
            }
            
            //Ver1
            // int count = (int) Mathf.Floor(timeInClip / data.easeTime);
            // bool bStart2End = (count % 2) == 0;
            // float time = timeInClip % data.easeTime;
            // if (!bStart2End)
            //     time = data.easeTime - time;
            // Color ret = Color.Lerp(data.color, data.endColor, time / data.easeTime);
            // Debug.Log($"timeInClip:{timeInClip};bStart2End:{bStart2End};time:{time};ret:{ret}");
            
            //Ver2
            float time = (timeInClip % (2 * data.easeTime));
            Color startColor, endColor;
            float rate;
            if (time < data.easeTime)
            {
                startColor = data.color;
                endColor = data.endColor;
                rate = time / data.easeTime;
            }
            else
            {
                startColor = data.endColor;
                endColor = data.color;
                rate = (time - data.easeTime) / data.easeTime;
            }
            Color ret = Color.Lerp(startColor, endColor, rate);
            return ret;
        }
        
        private void SetColor(Color color)
        {
            if (block != null)
            {
                block.SetColor(EmissionColorId, color);
            }
            if (renders != null)
            {
                foreach (var renderer in renders)
                {
                    renderer?.SetPropertyBlock(block);
                }
            }
        }
        
        public override void OnPlayableDestroy(Playable playable)
        {
            base.OnPlayableDestroy(playable);
            if (renders != null)
            {
                foreach (var renderer in renders)
                {
                    renderer?.SetPropertyBlock(null);
                }
            }
            block = null;
            renders = null;
        }
        
        
    }
}