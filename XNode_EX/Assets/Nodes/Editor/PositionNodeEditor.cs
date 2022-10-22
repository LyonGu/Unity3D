using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;
[CustomNodeEditor(typeof(PositionNode))]
public class PositionNodeEditor : NodeEditor
{
    public virtual void OnHeaderGUI()
    {
        GUILayout.Label("Position", NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
    }
	public bool isPos;
	int bornLine;
	public override void OnBodyGUI()
	{
		base.OnBodyGUI();
        
		PositionNode node = target as PositionNode;
        node.row =  EditorGUILayout.IntField("Row",node.m_row);
        node.line = EditorGUILayout.IntField("Line",node.m_lines);
        if (node.isCreateEnermyDic != null)
        {
            for (int i = 0; i < node.line; i++)
            {
                GUILayout.BeginHorizontal();
                for (int j = 0; j < node.row; j++)
                {
                    if (node.ishs.Count <= j + node.row * i)
                    {
                        node.ishs.Add(false);
                    }
                    node.ishs[j + node.row * i] = GUILayout.Toggle(node.ishs[j + node.row * i], "");
                }
                GUILayout.EndHorizontal();
            }
        }
        NodePort output = target.GetPort("positionNode");
		if (output != null) NodeEditorGUILayout.PortField(new GUIContent("PositionNode"), output, GUILayout.MinWidth(0));
	}
}
