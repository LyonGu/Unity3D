using UnityEditor;

namespace SkillEditor.Timeline
{
    [CustomEditor(typeof(GlowEffectClip))]
    public class GlowEffectClipInspector : BaseClipInspector<GlowEffectBehaviour>
    {
        public override void OnInspectorGUI()
        {
            ItemBase data = Target.data;
            if (data == null)
                return;

            var castData = (GlowEffect) data;

            UnityEngine.Color color = castData.color;
            color = EditorGUILayout.ColorField("Color", color);
            castData.color = new UnityEngine.Color(color.r, color.g, color.b, 1);

            UnityEngine.Color endColor = castData.endColor;
            endColor = EditorGUILayout.ColorField("endColor", endColor);
            castData.endColor = new UnityEngine.Color(endColor.r, endColor.g, endColor.b, 1);

            castData.easeTime = EditorGUILayout.FloatField("EaseTime", castData.easeTime);

            Target.data = castData;
            base.OnInspectorGUI();
        }
    }
}