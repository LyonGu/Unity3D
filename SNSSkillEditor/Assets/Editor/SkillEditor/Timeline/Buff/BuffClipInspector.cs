using UnityEditor;

namespace SkillEditor.Timeline
{
    [CustomEditor(typeof(BuffClip))]
    public class BuffClipInspector : BaseClipInspector<BuffBehaviour>
    {
        public override void OnInspectorGUI()
        {
            ItemBase data = Target.data;
            AddBuff buffData = (AddBuff) data;
            buffData.buffId = EditorGUILayout.IntField("BuffId", buffData.buffId);
            base.OnInspectorGUI();
        }
    }
}