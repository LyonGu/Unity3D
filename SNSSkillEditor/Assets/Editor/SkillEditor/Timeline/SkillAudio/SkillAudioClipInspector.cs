using UnityEditor;

namespace SkillEditor.Timeline
{
    [CustomEditor(typeof(SkillAudioClip))]
    public class SkillAudioClipInspector : BaseClipInspector<SkillAudioBehaviour>
    {
        public override void OnInspectorGUI()
        {
            ItemBase data = Target.data;
            if (data == null)
                return;

            var castData = (SkillAudio) data;

            castData.bankName = EditorGUILayout.TextField("Bank Name", castData.bankName);
            castData.eventName = EditorGUILayout.TextField("Event Name", castData.eventName);

            Target.data = castData;
            base.OnInspectorGUI();
        }
    }
}