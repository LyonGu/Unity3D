using UnityEditor;

namespace SkillEditor.Timeline
{
    [CustomEditor((typeof(TimeProgressTipClip)))]
    public class TimeProgressTipInspector : BaseClipInspector<TimeProgressTipBehaviour>
    {
        public override void OnInspectorGUI()
        {
            ItemBase data = Target.data;
            if (data == null)
                return;

            TimeProgressTip castData = (TimeProgressTip) data;

            castData.direction = (EnumConfig.Direction) EditorGUILayout.EnumPopup("Direction", castData.direction);
            castData.text = EditorGUILayout.TextField("text", castData.text);
            
            Target.data = castData;
            base.OnInspectorGUI();
        }
    }
}