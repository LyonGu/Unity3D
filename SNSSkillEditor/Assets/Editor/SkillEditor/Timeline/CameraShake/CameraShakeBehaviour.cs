using Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

namespace SkillEditor.Timeline
{
    public class CameraShakeBehaviour : BaseBehaviour
    {

        private CinemachineVirtualCamera _virtualCamera;

        CinemachineVirtualCamera VirtualCamera
        {
            get
            {
                if (_virtualCamera == null)
                {
                    var cinemachineCamera = Camera.main.GetComponent<CinemachineBrain>();
                    _virtualCamera = (CinemachineVirtualCamera)cinemachineCamera.ActiveVirtualCamera;
                }

                return _virtualCamera;
            }
        }
        private CinemachineBasicMultiChannelPerlin _perlin;

        CinemachineBasicMultiChannelPerlin Perlin
        {
            get
            {
                if (_perlin == null)
                {
                    if (VirtualCamera != null)
                    {
                        _perlin = VirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
                    }
                }

                return _perlin;
            }
            set
            {
                _perlin = value;
            }
        }


        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (!SkillEditorUtil.IsGameRunning())
            {
                CameraShake cameraShakeData = (CameraShake)GetData();
                if (Perlin != null)
                {
                    Perlin.m_AmplitudeGain = cameraShakeData.amplitudeGain;
                    Perlin.m_FrequencyGain = cameraShakeData.frequencyGain;
                    if (!Application.isPlaying)
                    {
                        CinemachineBrain.SoloCamera = VirtualCamera;
                    }
                }
            }
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (!SkillEditorUtil.IsGameRunning())
            {
                if (Perlin != null)
                {
                    Perlin.m_AmplitudeGain = 0;
                    Perlin.m_FrequencyGain = 0;
                    if (!Application.isPlaying)
                    {
                        CinemachineBrain.SoloCamera = null;
                    }
                }
            }
        }
    }
}