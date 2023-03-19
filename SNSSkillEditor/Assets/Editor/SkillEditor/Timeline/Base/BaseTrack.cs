using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SkillEditor.Timeline
{
    [BindSkillTrack(typeof(ItemBase))]
    public abstract class BaseTrack<T> : TrackAsset where T : BaseBehaviour, new()
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            SkillTimelineAsset timelineAsset = go.GetComponent<PlayableDirector>().playableAsset as SkillTimelineAsset;

            var clips = GetClips();
            foreach (var clip in clips)
            {
                var baseClip = clip.asset as BaseClip<T>;
                if (baseClip != null)
                {
                    bool createFromEditor = InitData(baseClip);
                    if (createFromEditor) clip.duration = 1;
                    timelineAsset.RecordItemBase(this.GetGroup()?.name, baseClip.data);
                    baseClip.timelineClip = clip;
                    baseClip.OnCreate();
                }
            }

            return base.CreateTrackMixer(graph, go, inputCount);
        }

        //创建Clip数据有两种方式，一种是读Json然后反射赋值，另一种是编辑器下AddClip，这样就会走InitData逻辑。
        private bool InitData(BaseClip<T> baseClip)
        {
            if (baseClip.data != null)
            {
                return false;
            }
            Type type = GetType();
            var attribute = type.GetCustomAttribute<BindSkillTrackAttribute>();
            if (attribute != null)
            {
                var realTrackType = attribute.SkillTrackType;
                baseClip.data = (ItemBase)Activator.CreateInstance(realTrackType);
                // var groupTrack = this.GetGroup();
                // if (groupTrack != null)
                // {
                //     // SkillEditorManager.Instance.SelectedSkillStage[groupTrack.name].Add(baseClip.data);
                //     SkillEditorManager.Instance.AddClipToSelectedSkillStage(groupTrack.name, baseClip.data);
                // }
            }
            else
            {
                Debug.LogError("技能Track应该有绑定数据类型，缺少BindSkillTrackAttribute！");
            }
            return true;
        }
    }

    public class BindSkillTrackAttribute : Attribute
    {
        public Type SkillTrackType { get; }

        public BindSkillTrackAttribute(Type skillTrackType)
        {
            this.SkillTrackType = skillTrackType;
        }
    }
}