using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using XNodeEditor;
using static XNodeEditor.NodeEditor;

[CustomNodeEditor(typeof(CreateEnermyNode))]
public class CreateEnermyNodeEditor : GameStateNodeEditor
{

	public override void OnBodyGUI()
	{
		GUILayout.BeginHorizontal();
		NodePort enter = target.GetPort("enter");
		if (enter != null) NodeEditorGUILayout.PortField(new GUIContent("Enter"), enter, GUILayout.MinWidth(0));
		NodePort output = target.GetPort("exit");
		if (output != null) NodeEditorGUILayout.PortField(new GUIContent("Exit"), output, GUILayout.MinWidth(0));
		GUILayout.EndHorizontal();
		base.OnBodyGUI();
		
		CreateEnermyNode node = target as CreateEnermyNode;
		GameStateGraph graph = node.graph as GameStateGraph;
		NodePort positionInput = target.GetPort("positionNodeInput");
				if (positionInput != null) NodeEditorGUILayout.PortField(new GUIContent("Poses"), positionInput);
		GUILayout.BeginHorizontal();
		GUILayout.Label("CurEnermyCount:" + node.curEnermyCount.ToString());
		GUILayout.EndHorizontal();
	}
}
