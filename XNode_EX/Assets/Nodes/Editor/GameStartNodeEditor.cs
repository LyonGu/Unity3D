using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using XNodeEditor;
[CustomNodeEditor(typeof(GameStartNode))]
public class GameStartNodeEditor : NodeEditor
{
	public override void OnBodyGUI()
	{
		base.OnBodyGUI();
		BulletNode node = target as BulletNode;
		NodePort output = target.GetPort("exit");
		if (output != null) NodeEditorGUILayout.PortField(new GUIContent("Exit"), output, GUILayout.MinWidth(0));


	}
}
