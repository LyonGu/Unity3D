using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(CommponentSaveData))]
public class CommponentSaveDataEditor : Editor
{
    CommponentSaveData data;

    public CompoentState testState = CompoentState.Born;


    // test
    //private ReorderableList m_NameList;

    void OnEnable()
    {
        //获取当前编辑自定义Inspector的对象
        data = (CommponentSaveData)target;
        //m_NameList = new ReorderableList(serializedObject,
        //    serializedObject.FindProperty("compList"),
        //    true, true, true, true);

        //m_NameList.drawElementCallback = DrawNameElement;

        //m_NameList.drawHeaderCallback = (Rect rect) =>
        //{
        //    GUI.Label(rect, "compList");
        //};
    }
    //private void DrawNameElement(Rect rect, int index, bool selected, bool focused)
    //{
    //    SerializedProperty itemData = m_NameList.serializedProperty.GetArrayElementAtIndex(index);

    //    rect.y += 2;
    //    rect.height = EditorGUIUtility.singleLineHeight;
    //    EditorGUI.PropertyField(rect, itemData, GUIContent.none);
    //}
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Save"))
        {
            data.SaveToDatas();
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        testState = (CompoentState) EditorGUILayout.EnumPopup("测试状态", testState);

        EditorGUILayout.Space();
        if (GUILayout.Button("PlaySate"))
        {
            data.SetState(testState);
        }

        //serializedObject.Update();
        //m_NameList.DoLayoutList();
        //serializedObject.ApplyModifiedProperties();




        //showWeapons = EditorGUILayout.Foldout(showWeapons, "Weapons");
        //if (showWeapons)
        //{
        //     EditorGUILayout.FloatField("Weapon 1 Damage",1);
        //     EditorGUILayout.FloatField("Weapon 2 Damage", 2);
        //}
        //if (GUILayout.Button("Test"))
        //{
        //    data.TestData();
        //}
    }
}
