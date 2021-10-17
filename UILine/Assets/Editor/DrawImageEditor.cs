using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DrawImage), true)]
public class DrawImageEditor : UnityEditor.UI.ImageEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var tg = target as DrawImage;

        tg.offset = EditorGUILayout.Vector2Field("offset", tg.offset);
        tg.LineWidth = EditorGUILayout.FloatField("LineWidth", tg.LineWidth);
        //绘制几个执行按钮
        if (GUILayout.Button("测试画点"))
        {
            int x1 = Random.Range(-200, 200);
            int y1 = Random.Range(-300, 300);
            Debug.Log($"测试画点 {x1} {y1}");
            tg.AddRectPoint(new Vector2(x1,y1),tg.offset);

        }

        if (GUILayout.Button("测试画线"))
        {
            //int x1 = Random.Range(-200, 200);
            //int x2 = Random.Range(-200, 200);
            //int y1 = Random.Range(-300, 300);
            //int y2 = Random.Range(-300, 300);

            //Debug.Log($"测试画线 {x1} {y1}     {x2} {y2}");
            ////tg.DrawLine(new Vector2(x1, y1), new Vector2(x2, y2));

            //tg.DrawLineEx(new Vector2(x1, y1), new Vector2(x2, y2));


            //tg.DrawLine(new Vector2(90, -42), new Vector2(-170, 59));

            int count = tg.pointer.Count;
            if (count > 1)
            {
                for (int i = 0; i < tg.pointer.Count; i++)
                {
                    if (i + 1 >= count)
                        return;
                    tg.DrawLineEx(tg.pointer[i], tg.pointer[i+1]);
                }
            }
        }

    }
}
