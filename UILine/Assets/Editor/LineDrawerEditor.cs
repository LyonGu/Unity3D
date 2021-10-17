using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LineDrawer), true)]
public class LineDrawerEditor : UnityEditor.UI.ImageEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var tg = target as LineDrawer;
        tg.lineWidth = EditorGUILayout.FloatField("lineWidth", tg.lineWidth);
        tg.offset = EditorGUILayout.Vector2Field("offset", tg.offset);

        //绘制几个执行按钮
        if (GUILayout.Button("测试画点"))
        {
            int x1 = Random.Range(-200, 200);
            int y1 = Random.Range(-300, 300);
            Debug.Log($"测试画点 {x1} {y1}");
            tg.AddRectPoint(new Vector2(x1, y1), tg.offset);

        }

        if (GUILayout.Button("测试画线"))
        {
            int startIndex = tg.GetFirstLinePoint();
            if (startIndex >= 0)
            {
                Debug.Log($"测试画点 startIndex {startIndex}");
                tg.DrawLine(startIndex, startIndex+1);
            }
        }

    }
}
