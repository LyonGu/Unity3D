using UnityEditor;

namespace SkillEditor.Timeline
{
    [CustomEditor(typeof(AlertClip))]
    public class AlertClipInspector : BaseClipInspector<AlertBehaviour>
    {
        public override void OnInspectorGUI()
        {
            ItemBase data = Target.data;
            if (data == null)
                return;

            var castData = (Alert) data;

            castData.shape = (EnumConfig.shape)EditorGUILayout.EnumPopup("Shape", castData.shape);
            castData.length = EditorGUILayout.FloatField("Length", castData.length);
            castData.width = EditorGUILayout.FloatField("Width", castData.width);
            castData.angle = EditorGUILayout.FloatField("Angle", castData.angle);
            castData.shiftDistance = EditorGUILayout.FloatField("Shift Distance", castData.shiftDistance);

            Target.data = castData;

            base.OnInspectorGUI();
        }
    }
}