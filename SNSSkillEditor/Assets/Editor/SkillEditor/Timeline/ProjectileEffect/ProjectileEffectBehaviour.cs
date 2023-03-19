using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

namespace SkillEditor.Timeline
{
    public class ProjectileEffectBehaviour : BaseBehaviour
    {

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            // if (!Application.isPlaying)
            {
                // var data = (ProjectileEffect) GetData();
                // var conf = new GameProjectileHeightFixedParabolaConfig();
                // conf.height = data.height;
                // conf.gravity = data.gravity;
                // conf.prefab = data.effectPath;
                //
                //
                // var root = GetSelfModelRoot();
                // var attachNode = root.FirstOrDefault(t => t.name == data.attachNodeName);
                // if (attachNode == null)
                //     attachNode = root;
                //
                // var source = new GameTransform();
                // source.Position = attachNode.position + data.offset;
                // source.Scale = data.scale;
                //
                // var dest = new GameTransform();
                //
                // // var target = GameObject.Find("ProjectileTarget");
                // var target = GetOtherModelRoot().gameObject;
                // dest.Position = target.transform.position;
                // dest.Scale = UnityEngine.Vector3.one;
                //
                // GameProjectileManager.Instance.CreateProjectile(conf, source, dest, null);
            }
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {

        }
    }
}