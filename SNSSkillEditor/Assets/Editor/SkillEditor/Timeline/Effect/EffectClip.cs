using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace SkillEditor.Timeline
{
    public class EffectClip : BaseClip<EffectBehaviourBase>, IPropertyPreview
    {
        static readonly HashSet<ParticleSystem> s_SubEmitterCollector = new HashSet<ParticleSystem>();
        
        public GameObject sourceGameObject;

        public uint particleRandomSeed;

        public override void OnCreate()
        {
            // can't be set in a constructor
            if (particleRandomSeed == 0)
                particleRandomSeed = (uint)Random.Range(1, 10000);
            
            CreateEffectGameObject();
        }

        public void CreateEffectGameObject(bool force = false)
        {
            if (sourceGameObject == null || force)
            {
                if(sourceGameObject != null)
                    DestroyImmediate(sourceGameObject);
                
                // var baseClip = timelineClip.asset;
                // var data = baseClip.GetType().GetField("data").GetValue(baseClip);
                // Effect effectData = (Effect) data;
                Effect effectData = (Effect) data;

                string path = effectData.effectPath;
                if (!string.IsNullOrEmpty(path))
                {
                    Object prefab = AssetDatabase.LoadAssetAtPath<Object>(path);
                    Transform effectRoot = SkillEditorManager.Instance.EffectRoot;
                    if (prefab != null && effectRoot != null)
                    {
                        sourceGameObject = PrefabUtility.InstantiatePrefab(prefab, effectRoot) as GameObject; //(GameObject)Object.Instantiate(prefab);
                        if (sourceGameObject != null)
                        {
                            sourceGameObject.SetActive(false);
                            sourceGameObject.transform.localScale = effectData.scale;
                            sourceGameObject.transform.localPosition += effectData.offset;
                            sourceGameObject.transform.eulerAngles = effectData.rotation;
                        }
                    }
                }
            }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            Playable root = Playable.Null;
            var playables = new List<Playable>();

            if (sourceGameObject != null)
            {
                //控制根节点开关的Behavior
                CreateActivationPlayable(sourceGameObject, graph, playables);
                
                //控制粒子效果的Behavior
                var particleSystems = sourceGameObject.GetComponentsInChildren<ParticleSystem>(true);
                SearchHierarchyAndConnectParticleSystem(particleSystems, graph, playables);
                
                //控制子节点开关、位移等Animation的Behavior
                CreateAnimationPlayable(sourceGameObject, graph, playables);
                
                root = ConnectPlayablesToMixer(graph, playables);
            }
            if (!root.IsValid())
                root = Playable.Create(graph);

            
            data.timeBegin = (float)timelineClip.start;
            data.time = (float)timelineClip.duration;
            // var skillTimelineAsset = (SkillTimelineAsset) timelineClip.GetParentTrack().timelineAsset;
            // skillTimelineAsset.MarkTimelineChange();

            return root;
        }
        
        static Playable ConnectPlayablesToMixer(PlayableGraph graph, List<Playable> playables)
        {
            var mixer = Playable.Create(graph, playables.Count);

            for (int i = 0; i != playables.Count; ++i)
            {
                ConnectMixerAndPlayable(graph, mixer, playables[i], i);
            }

            mixer.SetPropagateSetTime(true);

            return mixer;
        }
        
        static void ConnectMixerAndPlayable(PlayableGraph graph, Playable mixer, Playable playable,
            int portIndex)
        {
            graph.Connect(playable, 0, mixer, portIndex);
            mixer.SetInputWeight(playable, 1.0f);
        }
        
        void CreateActivationPlayable(GameObject root, PlayableGraph graph,
            List<Playable> outplayables)
        {
            var activation = EffectActivationBehaviour.Create(graph, root, timelineClip);
            if (activation.IsValid())
                outplayables.Add(activation);
        }

        void CreateAnimationPlayable(GameObject root, PlayableGraph graph,
            List<Playable> outplayables)
        {

            var anims = root.GetComponentsInChildren<UnityEngine.Animation>(true);
            if (anims != null && anims.Length > 0)
            {
                foreach (var anim in anims)
                {
                    var animBeh = EffectAnimationBehaviour.Create(graph, anim.gameObject, timelineClip);
                    if(animBeh.IsValid())
                        outplayables.Add(animBeh);
                }
            }
        }
        
        void SearchHierarchyAndConnectParticleSystem(IEnumerable<ParticleSystem> particleSystems, PlayableGraph graph,
            List<Playable> outplayables)
        {
            foreach (var particleSystem in particleSystems)
            {
                if (particleSystem != null)
                {
                    outplayables.Add(EffectParticleBehaviour.Create(graph, particleSystem, particleRandomSeed, timelineClip));
                }
            }
        }

        public void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            if (SkillEditorManager.Instance.EffectRoot != null)
            {
                GameObject effectRoot = SkillEditorManager.Instance.EffectRoot.gameObject;
                foreach (var trans in effectRoot.GetComponentsInChildren<Transform>(true))
                {
                    driver.AddFromName(trans.gameObject, "m_IsActive");
                }
                foreach (var ps in effectRoot.GetComponentsInChildren<ParticleSystem>(true))
                {
                    driver.AddFromName<ParticleSystem>(ps.gameObject, "randomSeed");
                    driver.AddFromName<ParticleSystem>(ps.gameObject, "autoRandomSeed");
                }
            }
        }

        public override void InitWithData()
        {
            base.InitWithData();
            var castData = (Effect) data;
            timelineClip.displayName = Path.GetFileNameWithoutExtension(castData.effectPath);
        }
    }
}