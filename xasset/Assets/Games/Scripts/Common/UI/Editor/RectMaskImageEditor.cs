namespace HxpGame.UI
{
    
using UnityEditor;
using UnityEditor.UI;

/// <summary>
/// The editor for <see cref="RectMaskImage"/>.
/// </summary>
[CustomEditor(typeof(RectMaskImage), true)]
[CanEditMultipleObjects]
internal sealed class RectMaskImageEditor : ImageEditor
{
    private SerializedProperty RatioLeft;
    private SerializedProperty RatioRight;
    private SerializedProperty RatioTop;
    private SerializedProperty RatioBtm;

    /// <inheritdoc/>
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        this.serializedObject.Update();
        EditorGUILayout.PropertyField(this.RatioLeft);
        EditorGUILayout.PropertyField(this.RatioRight);
        EditorGUILayout.PropertyField(this.RatioTop);
        EditorGUILayout.PropertyField(this.RatioBtm);
        this.serializedObject.ApplyModifiedProperties();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        var serObj = this.serializedObject;
        this.RatioLeft = serObj.FindProperty("RatioLeft");
        this.RatioRight = serObj.FindProperty("RatioRight");
        this.RatioTop = serObj.FindProperty("RatioTop");
        this.RatioBtm = serObj.FindProperty("RatioBtm");
    }
}


}