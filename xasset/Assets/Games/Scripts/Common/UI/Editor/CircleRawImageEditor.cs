//------------------------------------------------------------------------------
// Copyright (c) 2018-2020 Beijing Bytedance Technology Co., Ltd.
// All Right Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
//------------------------------------------------------------------------------

namespace HxpGame.UI
{
    using UnityEditor;
    using UnityEditor.UI;

    /// <summary>
    /// The editor for <see cref="CircleRawImage"/>.
    /// </summary>
    [CustomEditor(typeof(CircleRawImage), true)]
    [CanEditMultipleObjects]
    internal sealed class CircleRawImageEditor : RawImageEditor
    {
        private SerializedProperty segmentCount;
        private SerializedProperty fillPercent;

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            this.serializedObject.Update();
            EditorGUILayout.PropertyField(this.segmentCount);
            EditorGUILayout.PropertyField(this.fillPercent);
            this.serializedObject.ApplyModifiedProperties();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            var serObj = this.serializedObject;
            this.segmentCount = serObj.FindProperty("segmentCount");
            this.fillPercent = serObj.FindProperty("fillPercent");
        }
    }
}