using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using XNodeEditor;
[CustomNodeEditor(typeof(GameStateNode))]
public class GameStateNodeEditor : NodeEditor
{
    public override void OnHeaderGUI()
    {
        GUI.color = Color.white;
        GameStateNode node = target as GameStateNode;
        if (node.led) GUI.color = Color.blue;
        string title = target.name;
        GUILayout.Label(title, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
        GUI.color = Color.white;
    }
    public override void OnBodyGUI()
	{
		base.OnBodyGUI();
		BulletNode node = target as BulletNode;
	}
}
