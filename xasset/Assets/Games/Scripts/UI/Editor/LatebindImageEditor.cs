using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

[CustomEditor(typeof(LatebindImage), true)]
public class LatebindImageEditor : UnityEditor.UI.ImageEditor
{
    private SerializedProperty invisibleIfSpriteNull;

    protected override void OnEnable()
    {
        base.OnEnable();
        this.invisibleIfSpriteNull = serializedObject.FindProperty("invisibleIfSpriteNull");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var tg = target as LatebindImage;
        if (tg == null)
            return;

        serializedObject.Update();
        EditorGUILayout.PropertyField(this.invisibleIfSpriteNull);
        serializedObject.ApplyModifiedProperties();
//        using (Oasis.Unity.GUIPanel.Layout("详细信息"))
//        {
//            if (tg.sprite != null)
//            {
//                EditorGUILayout.ObjectField("图集", tg.sprite.texture, typeof(Texture2D), true);
//                GUI.enabled = false;
//                EditorGUILayout.RectField("textureRect", tg.sprite.textureRect);
//                EditorGUILayout.RectField("Rect", tg.sprite.rect);
//                EditorGUILayout.FloatField("pixelsPerUnity", tg.sprite.pixelsPerUnit);
//                GUI.enabled = true;
//            }
//
//        }
    }
}
