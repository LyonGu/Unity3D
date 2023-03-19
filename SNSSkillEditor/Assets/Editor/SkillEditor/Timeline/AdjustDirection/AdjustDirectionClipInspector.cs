using UnityEditor;

namespace SkillEditor.Timeline
{
    [CustomEditor(typeof(AdjustDirectionClip))]
    public class AdjustDirectionClipInspector : BaseClipInspector<AdjustDirectionBehaviour>
    {
        public override void OnInspectorGUI()
        {
            ItemBase data = Target.data;
            if (data == null)
                return;

            var castData = (AdjustDirection) data;

            castData.dirTarget = (EnumConfig.dirTarget)EditorGUILayout.EnumPopup("Direction Target", castData.dirTarget);
            castData.angle = EditorGUILayout.FloatField("Angle", castData.angle);
            castData.tweening = EditorGUILayout.Toggle("Tweening", castData.tweening);

            Target.data = castData;

            base.OnInspectorGUI();
        }
    }
}