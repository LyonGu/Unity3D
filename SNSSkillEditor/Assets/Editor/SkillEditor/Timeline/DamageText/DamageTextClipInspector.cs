using UnityEditor;

namespace SkillEditor.Timeline
{
    [CustomEditor(typeof(DamageTextClip))]
    public class DamageTextClipInspector : BaseClipInspector<DamageTextBehaviour>
    {
        public override void OnInspectorGUI()
        {
            ItemBase data = Target.data;
            if (data == null)
                return;

            var castData = (DamageText) data;

            castData.damageTextType = (EnumConfig.damageTextType)EditorGUILayout.EnumPopup("Damage Text Type", castData.damageTextType);
            castData.rate = EditorGUILayout.FloatField("比率", castData.rate);

            Target.data = castData;
            
            base.OnInspectorGUI();
        }
    }
}