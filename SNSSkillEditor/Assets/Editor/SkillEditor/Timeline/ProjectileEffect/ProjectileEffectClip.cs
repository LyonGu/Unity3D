using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

namespace SkillEditor.Timeline
{
    public class ProjectileEffectClip : BaseClip<ProjectileEffectBehaviour>, IPropertyPreview
    {
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
            var castData = (ProjectileEffect) data;
            timelineClip.displayName = Path.GetFileNameWithoutExtension(castData.effectPath);
        }

        public GameObject sourceObject;
        

        public override void OnCreate()
        {
            CreateEffectGameObject();
        }

        public void CreateEffectGameObject(bool force = false)
        {
            if (sourceObject == null || force)
            {
                if(sourceObject != null)
                    DestroyImmediate(sourceObject);
                var baseClip = timelineClip.asset;
                var data = baseClip.GetType().GetField("data").GetValue(baseClip);
                ProjectileEffect castData = (ProjectileEffect) data;
                
                Object prefab = AssetDatabase.LoadAssetAtPath<Object>(castData.effectPath);
                if (prefab == null)
                    return;
                
                Transform attachNode = null;
                var root = GetSelfModelRoot();
                if (root != null)
                {
                    attachNode = root.FirstOrDefault(t => t.name == castData.attachNodeName);
                    if (attachNode == null)
                        attachNode = root;
                }

                sourceObject = new GameObject(Path.GetFileNameWithoutExtension(castData.effectPath));
                sourceObject.transform.SetParent(SkillEditorManager.Instance.EffectRoot);
                if(attachNode != null)
                    sourceObject.transform.position = attachNode.position + castData.offset;
            }
        }
        
    }

}