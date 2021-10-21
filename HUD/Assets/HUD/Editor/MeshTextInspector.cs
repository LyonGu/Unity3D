using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MeshText), true)]
public class MeshTextInspector : Editor
{
    public override void OnInspectorGUI()
    {
        MeshText meshText = target as MeshText;
        meshText.meshFilter = (MeshFilter)EditorGUILayout.ObjectField("MeshFilter", meshText.meshFilter, typeof(MeshFilter), true);
        meshText.meshRenderer = (MeshRenderer)EditorGUILayout.ObjectField("MeshRenderer", meshText.meshRenderer, typeof(MeshRenderer), true);
        meshText.color1 = EditorGUILayout.ColorField("Color 1", meshText.color1);
        meshText.color2 = EditorGUILayout.ColorField("Color 2", meshText.color2);
        meshText.uiAtlas = (UIAtlas)EditorGUILayout.ObjectField("UIAtlas", meshText.uiAtlas, typeof(UIAtlas), true);
        meshText.Text = EditorGUILayout.TextField("Text", meshText.Text);
        meshText.HAlignType = (MeshText.HorizontalAlignType)EditorGUILayout.IntPopup("HorizontalAlign", (int)meshText.HAlignType, new string[] { "左对齐", "居中对齐", "右对齐" }, new int[] { (int)MeshText.HorizontalAlignType.Left, (int)MeshText.HorizontalAlignType.Center, (int)MeshText.HorizontalAlignType.Right });
    }
}
