using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;

namespace SkillEditor.Timeline
{
    [CustomEditor(typeof(MoveClip))]
    public class MoveClipInspector : BaseClipInspector<MoveBehaviour>
    {
        private bool IsUnderAttackerGroup()
        {
            return Target.timelineClip.GetParentTrack().GetGroup().name.Equals("attacker");
        }

        private bool IsAttackerDirectionType(System.Enum value)
        {
            return value.Equals(EnumConfig.MoveDirectionType.RoleDir) ||
                   value.Equals(EnumConfig.MoveDirectionType.SkillPos) ||
                   value.Equals(EnumConfig.MoveDirectionType.Target);
        }

        public override void OnInspectorGUI()
        {
            ItemBase data = Target.data;
            if (data == null)
                return;

            Move castData = (Move) data;

            // MoveDirectionType
            // 分为两类，攻击者有三种，目标有两种。两者各是分开的
            //  Attacker
            //    RoleDir 朝角色当前方向移动xx距离
            //    SkillPos 技能释放点位置
            //    Target 朝着目标移动，（如果目标移动，不需要跟着移动
            //  Target
            //    ConnectionDir 攻击者和受击者的连线
            //    SkillDir 被攻击的技能方向
            //
            // 1. role当前朝向为正方向
            // 3.两者连线，以受击方为起点，朝向攻击方的射线方向为正方向
            // 4.两者连线，以受击方为起点，朝向攻击方的射线方向为正方向
            // 5.以技能方向的反方向为正方向
            //

            //设置默认值
            if (castData.directionType == 0)
            {
                castData.directionType = IsUnderAttackerGroup()
                    ? EnumConfig.MoveDirectionType.RoleDir
                    : EnumConfig.MoveDirectionType.ConnectionDir;
            }
            
            castData.directionType = (EnumConfig.MoveDirectionType)EditorGUILayout.EnumPopup(new GUIContent("Direction Type"), castData.directionType, (e) =>
            {
                if (IsUnderAttackerGroup())
                {
                    return IsAttackerDirectionType(e);
                }
                else
                {
                    return !IsAttackerDirectionType(e);
                }
            }, false);

            bool useDir = castData.directionType == EnumConfig.MoveDirectionType.RoleDir ||
                          castData.directionType == EnumConfig.MoveDirectionType.ConnectionDir ||
                          castData.directionType == EnumConfig.MoveDirectionType.SkillDir;
            if (useDir)
            {
                castData.distance = EditorGUILayout.FloatField("Distance", castData.distance);
            }
            
            if (castData.directionType == EnumConfig.MoveDirectionType.Target)
            {
                castData.offset = EditorGUILayout.FloatField("Offset", castData.offset);
            }
            
            GUILayout.Space(5);
            GUILayout.BeginVertical("box");
            UnityEngine.AnimationClip obj = AssetDatabase.LoadAssetAtPath<UnityEngine.AnimationClip>(castData.animClipPath);
            obj = (UnityEngine.AnimationClip)EditorGUILayout.ObjectField(obj, typeof(UnityEngine.AnimationClip), false);
            if (obj != null)
            {
                castData.animClipPath = AssetDatabase.GetAssetPath(obj);
                // if (FindCurve(obj, out var curve))
                // {
                //     EditorGUILayout.CurveField("原始位移曲线", curve);
                // }
            }
            castData.animClipPath = EditorGUILayout.TextField("Clip路径", castData.animClipPath);
            castData.curve = EditorGUILayout.CurveField("动画曲线", castData.curve);
            
            if (obj != null)
            {
                if (GUILayout.Button("应用Clip上对应的曲线"))
                {
                    castData.curve = ModifyCurve(obj);
                }
                if (GUILayout.Button($"应用Clip的时长{obj.length}s"))
                {
                    castData.time = obj.length;
                    Target.timelineClip.duration = obj.length;
                }
            }

            GUILayout.EndVertical();
            GUILayout.Space(5);

            // castData.directionType = (EnumConfig.MoveDirectionType) EditorGUILayout.EnumPopup("Direction Type", castData.directionType);
            // bool useDir = castData.directionType == EnumConfig.MoveDirectionType.SkillDirection ||
            //               castData.directionType == EnumConfig.MoveDirectionType.Connection;
            // if (useDir)
            // {
            //     castData.distance = EditorGUILayout.FloatField("Distance", castData.distance);
            // }
            //
            Target.data = castData;
            base.OnInspectorGUI();
        }

        private AnimationCurve ModifyCurve(UnityEngine.AnimationClip clip)
        {
            if (!FindCurve(clip, out AnimationCurve curve))
            {
                return new AnimationCurve(new Keyframe(0,0), new Keyframe(1, 1));
            }
            float startTime = curve.keys[0].time;
            float startValue = curve.keys[0].value;
            float finalTime = curve.keys[curve.keys.Length - 1].time; //https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/ranges-indexes
            float finalValue = curve.keys[curve.keys.Length - 1].value;
            float deltaTime = Math.Abs(finalTime - startTime);
            float deltaValue = Math.Abs(finalValue - startValue);

            if (deltaValue <= float.Epsilon)
            {
                Debug.LogError("Z轴位移距离文件为0, 位移文件有问题。默认使用AnimationCurve.Linear");
                return AnimationCurve.Linear(0,0, 1, 1);
            }
            
            Debug.Log($"totalTime:{deltaTime}, totalValue:{deltaValue}");
            AnimationCurve ret = new AnimationCurve();
            foreach (var curveKey in curve.keys)
            {
                var insertKeyFrame = curveKey; // value copy
                insertKeyFrame.time /= deltaTime;
                insertKeyFrame.value /= deltaValue;
                insertKeyFrame.inTangent *= (deltaTime / deltaValue);
                insertKeyFrame.outTangent *= (deltaTime / deltaValue);
                ret.AddKey(insertKeyFrame);
                Debug.Log($"source: time:{curveKey.time}, value:{curveKey.value}\r\n target: time:{insertKeyFrame.time}, value:{insertKeyFrame.value}");
            }
            return ret;
        }

        private bool FindCurve(UnityEngine.AnimationClip clip, out AnimationCurve curve)
        {
            var bindings = AnimationUtility.GetCurveBindings(clip);
            foreach (var editorCurveBinding in bindings)
            {
                if (editorCurveBinding.propertyName.Equals("m_LocalPosition.z") && editorCurveBinding.path.Equals("root"))
                {
                    curve = AnimationUtility.GetEditorCurve(clip, editorCurveBinding);
                    return true;
                }
            }
            curve = null;
            Debug.LogError($"尝试寻找Clip:[{clip.name}]上root节点的m_LocalPosition.z曲线，但没有找到。");
            return false;
        }
        
    }
}