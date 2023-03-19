using UnityEditor;

namespace SkillEditor.Timeline
{
    [CustomEditor(typeof(ModelShakeClip))]
    public class ModelShakeClipInspector : BaseClipInspector<ModelShakeBehaviour>
    {
        public override void OnInspectorGUI()
        {
            ItemBase data = Target.data;
            if (data == null)
                return;

            var castData = (ModelShake) data;

            UnityEngine.Vector3 range = castData.range;
            range = EditorGUILayout.Vector3Field("Range", range);
            castData.range = range;

            castData.angularFrequency = EditorGUILayout.FloatField("Angular Frequency", castData.angularFrequency);
            castData.damping = EditorGUILayout.FloatField("Damping", castData.damping);

            Target.data = castData;
            
            base.OnInspectorGUI();
        }
    }
}