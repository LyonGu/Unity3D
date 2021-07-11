using UnityEditor;

[CustomEditor(typeof(InspectorListExample))]
public class InspectorListExampleEditor : Editor
{
    private SerializedProperty intArray;
    private SerializedProperty stringList;

    private void OnEnable()
    {
        intArray = serializedObject.FindProperty("intArray");
        stringList = serializedObject.FindProperty("stringList");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(intArray, true);
        EditorGUILayout.PropertyField(stringList, true);
        serializedObject.ApplyModifiedProperties();
    }
}