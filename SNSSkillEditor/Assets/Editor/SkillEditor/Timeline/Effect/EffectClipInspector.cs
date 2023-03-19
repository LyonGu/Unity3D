using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SkillEditor.Timeline
{
    [CustomEditor(typeof(EffectClip))]
    public class EffectClipInspector : BaseClipInspector<EffectBehaviourBase>
    {
        public override void OnInspectorGUI()
        {
            ItemBase data = Target.data;
            if (data == null)
                return;

            Effect castData = (Effect) data;

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                GameObject effectObj = AssetDatabase.LoadAssetAtPath<GameObject>(castData.effectPath);
                effectObj = (GameObject)EditorGUILayout.ObjectField(effectObj, typeof(GameObject), false);
                if (effectObj != null)
                {
                    castData.effectPath = AssetDatabase.GetAssetPath(effectObj);
                }

                EditorGUILayout.BeginHorizontal();
                //castData.effectId = EditorGUILayout.IntField("Effect Id", castData.effectId);
                string path = EditorGUILayout.TextField("Effect Path", castData.effectPath);
                path = path.Replace("\\", "/");
                castData.effectPath = path;
                if (GUILayout.Button("Refresh", GUILayout.Width(80)))
                {
                    var t = (EffectClip) Target;
                    t.CreateEffectGameObject();
                }

                EditorGUILayout.EndHorizontal();
                castData.attach = (EnumConfig.attach) EditorGUILayout.EnumPopup("Attach", castData.attach);

                var effectClip = (EffectClip) Target;

                if (castData.attach == EnumConfig.attach.attach_node_self ||
                    castData.attach == EnumConfig.attach.attach_node_other)
                {
                    AttachNodeHelper.AttachNode nodeName;

                    nodeName = AttachNodeHelper.GetNodeByName(castData.attachNodeName);
                    nodeName = (AttachNodeHelper.AttachNode)EditorGUILayout.EnumPopup("Attach Node", nodeName);
                    // castData.attachNodeName = nodeName != AttachNode.none ? nodeName.ToString() : "";
                    castData.attachNodeName = AttachNodeHelper.GetNameByNode(nodeName);
                }
                else
                {
                    castData.attachNodeName = "";
                }

                castData.scale = EditorGUILayout.Vector3Field("Scale", castData.scale);
                castData.offset = EditorGUILayout.Vector3Field("Offset", castData.offset);
                castData.rotation = EditorGUILayout.Vector3Field("Rotation", castData.rotation);
                castData.isFollow = EditorGUILayout.Toggle("Follow", castData.isFollow);

                castData.isBallistic = EditorGUILayout.Toggle("弹道", castData.isBallistic);
                if (castData.isBallistic)
                {
                    castData.attachTarget = (EnumConfig.attach)EditorGUILayout.EnumPopup(new GUIContent("Attach Target"), castData.attachTarget,
                        (e) =>
                        {
                            return e.Equals(EnumConfig.attach.other_root) || e.Equals(EnumConfig.attach.attach_node_other);
                        }, false);
                    
                    //一些已经是self_root或者attach_node_self的，做一下转化
                    if (castData.attachTarget == EnumConfig.attach.self_root)
                        castData.attachTarget = EnumConfig.attach.other_root;
                    if (castData.attachTarget == EnumConfig.attach.attach_node_self)
                        castData.attachTarget = EnumConfig.attach.attach_node_other;
                    
                    if (castData.attachTarget == EnumConfig.attach.attach_node_other)
                    {
                        AttachNodeHelper.AttachNode nodeName = AttachNodeHelper.GetNodeByName(castData.attachTargetNodeName);
                        nodeName = (AttachNodeHelper.AttachNode)EditorGUILayout.EnumPopup("Attach Node", nodeName);
                        castData.attachTargetNodeName = AttachNodeHelper.GetNameByNode(nodeName);
                    }
                }

                castData.destroyWhenOwnerDie = EditorGUILayout.Toggle("人物死亡后销毁", castData.destroyWhenOwnerDie);
                castData.destroyWhenSkillCancel = EditorGUILayout.Toggle("技能取消后销毁", castData.destroyWhenSkillCancel);

                Target.data = castData;
                base.OnInspectorGUI();
                if (effectClip.sourceGameObject != null)
                {
                    EditorGUILayout.ObjectField("Hierarchy-Effect", effectClip.sourceGameObject, typeof(GameObject));
                }

                string effectName = Path.GetFileNameWithoutExtension(path);
                if (Target != null && Target.timelineClip != null)
                    Target.timelineClip.displayName = effectName;

                if (check.changed)
                {
                    effectClip.CreateEffectGameObject(true);
                }
            }
        }
    }
}