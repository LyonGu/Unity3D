using UnityEditor;

namespace SkillEditor.Timeline
{
    [CustomEditor(typeof(CameraShakeClip))]
    public class CameraShakeClipInspector : BaseClipInspector<CameraShakeBehaviour>
    {
        public override void OnInspectorGUI()
        {
            ItemBase data = Target.data;
            if (data == null)
                return;

            var castData = (CameraShake) data;

            castData.amplitudeGain = EditorGUILayout.FloatField("Amplitude Gain", castData.amplitudeGain);
            castData.frequencyGain = EditorGUILayout.FloatField("Frequency Gain", castData.frequencyGain);

            Target.data = castData;
            
            base.OnInspectorGUI();
        }
    }
}