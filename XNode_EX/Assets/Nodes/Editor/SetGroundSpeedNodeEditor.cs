using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using XNodeEditor;
[CustomNodeEditor(typeof(SetGroundSpeedNode))]
public class SetGroundSpeedNodeEditor : GameStateNodeEditor
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
	}
}
