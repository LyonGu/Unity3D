using UnityEngine;
using UnityEngine.Playables;

namespace SkillEditor.Timeline
{
    public class SkillAudioBehaviour : BaseBehaviour
    {
        private uint playingId;

        private bool playNextFrame = false;
        private bool hasPlayed = false;


        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var director = playable.GetGraph().GetResolver() as PlayableDirector;
            
            if (!hasPlayed && !playNextFrame && director.state == PlayState.Playing)
            {
                playNextFrame = true;
                return;
            }

            if (playNextFrame && !hasPlayed && director.state == PlayState.Playing)
            {
                var data = (SkillAudio) GetData();
                GameObject wwise = SkillEditorManager.Instance.WWise;
                if (wwise != null)
                {
                    // KWwise.Init();
                    // playingId = KWwise.PlayEvent(data.bankName, data.eventName, wwise);
                    hasPlayed = true;
                    playNextFrame = false;
                }
            }
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            hasPlayed = false;
            playNextFrame = false;
        }
    }
}