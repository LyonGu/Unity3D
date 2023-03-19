using UnityEditor;

namespace SkillEditor.Timeline
{
    [CustomEditor((typeof(SkillEventClip)))]
    public class SkillEventClipInspector : BaseClipInspector<SkillEventBehaviour>
    {
        public override void OnInspectorGUI()
        {
            ItemBase data = Target.data;
            if (data == null)
                return;

            SkillEvent castData = (SkillEvent) data;

            castData.skillEventType =
                (EnumConfig.SkillEventType) EditorGUILayout.EnumPopup("Event Type", castData.skillEventType);
            castData.param = EditorGUILayout.TextField("Param", castData.param);
            
            Target.data = castData;
            base.OnInspectorGUI();
        }
    }
}