using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using UnityEditor;
using System;

/*
 * 
 * CustomValueDrawer
 * 
 Instead of making a new attribute, and a new drawer, for a one-time thing, you can with this attribute, 
 make a method that acts as a custom property drawer. These drawers will out of the box have support for undo/redo and multi-selection.
     
     */

public class Odin_ex2 : MonoBehaviour
{
    public float From = 2, To = 7;

    //使用MyCustomDrawerStatic方法来绘制CustomDrawerStatic
    [CustomValueDrawer("MyCustomDrawerStatic")]
    public float CustomDrawerStatic;

    [CustomValueDrawer("MyCustomDrawerInstance")]
    public float CustomDrawerInstance;

    [CustomValueDrawer("MyCustomDrawerAppendRange")]
    public float AppendRange;

    [CustomValueDrawer("MyCustomDrawerArrayNoLabel")]
    public float[] CustomDrawerArrayNoLabel = new float[] { 3f, 5f, 6f };

    private static float MyCustomDrawerStatic(float value, GUIContent label)
    {
        return EditorGUILayout.Slider(label, value, 0f, 10f);
    }

    private float MyCustomDrawerInstance(float value, GUIContent label)
    {
        return EditorGUILayout.Slider(label, value, this.From, this.To);
    }

    private float MyCustomDrawerAppendRange(float value, GUIContent label, Func<GUIContent, bool> callNextDrawer)
    {
        SirenixEditorGUI.BeginBox();
        callNextDrawer(label);
        var result = EditorGUILayout.Slider(value, this.From, this.To);
        SirenixEditorGUI.EndBox();
        return result;
    }

    private float MyCustomDrawerArrayNoLabel(float value)
    {
        return EditorGUILayout.Slider(value, this.From, this.To);
    }
}
