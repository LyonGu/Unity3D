using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SkillEditor.Timeline
{
    [CustomEditor(typeof(ProjectileEffectClip))]
    public class ProjectileEffectClipInspector : BaseClipInspector<ProjectileEffectBehaviour>
    {
        public override void OnInspectorGUI()
        {
            ItemBase data = Target.data;
            if (data == null)
                return;

            ProjectileEffect castData = (ProjectileEffect) data;

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                Object go = EditorGUILayout.ObjectField(null, typeof(GameObject), false);
                if (go != null)
                {
                    castData.effectPath = AssetDatabase.GetAssetPath(go);
                }

                string path = EditorGUILayout.TextField("Effect Path", castData.effectPath);
                path = path.Replace("\\", "/");
                castData.effectPath = path;

                var clip = (ProjectileEffectClip) Target;
                AttachNodeHelper.AttachNode attachNodeName = AttachNodeHelper.GetNodeByName(castData.attachNodeName);

                attachNodeName = (AttachNodeHelper.AttachNode) EditorGUILayout.EnumPopup("Attach Node", attachNodeName);
                // castData.attachNodeName = attachNodeName != AttachNode.none ? attachNodeName.ToString() : "";                
                castData.attachNodeName = AttachNodeHelper.GetNameByNode(attachNodeName);
                
                castData.scale = EditorGUILayout.Vector3Field("Scale", castData.scale);
                castData.offset = EditorGUILayout.Vector3Field("Offset", castData.offset);
                castData.height = EditorGUILayout.FloatField("Height", castData.height);
                castData.gravity = EditorGUILayout.FloatField("Gravity", castData.gravity);

                Target.data = castData;
                base.OnInspectorGUI();
                if (clip.sourceObject != null)
                {
                    EditorGUILayout.ObjectField("objectInScene", clip.sourceObject, typeof(GameObject));
                    float y = clip.sourceObject.transform.position.y;
                    float time = (float)Math.Sqrt(2 * castData.height / castData.gravity) +
                                 (float)Math.Sqrt(2 * (y + castData.height) / castData.gravity);
                    time += (float)clip.timelineClip.start;
                    EditorGUILayout.TextField("Time", time.ToString());
                }

                string effectName = Path.GetFileNameWithoutExtension(path);
                if (Target != null && Target.timelineClip != null)
                    Target.timelineClip.displayName = effectName;

                if (check.changed)
                {
                    clip.CreateEffectGameObject(true);
                }
            }
        }
    }
}