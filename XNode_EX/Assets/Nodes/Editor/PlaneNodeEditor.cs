using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNodeEditor;
[CustomNodeEditor(typeof(PlaneNode))]
public class PlaneNodeEditor : NodeEditor
{
	public override void OnBodyGUI()
	{
        base.OnBodyGUI();
        PlaneNode node = target as PlaneNode;
        if (node.plane!=null&&node.plane.transform.GetChild(0) != null)
        {
            GUILayout.Label(node.plane.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite.texture, GUILayout.MaxWidth(60), GUILayout.MaxHeight(60));

        }
    }
}
