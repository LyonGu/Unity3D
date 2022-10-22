using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNodeEditor;
[CustomNodeEditor(typeof(SplineNode))]
public class SplineNodeEditor : NodeEditor
{
	public override void OnBodyGUI()
	{
		base.OnBodyGUI();
		SplineNode node = target as SplineNode;
		if (node.spline!=null&& node.spline.GetComponent<Spline>()!=null)
		{
			GUILayout.Box(node.spline.GetComponent<Spline>().pathTexture);
		}
		
		
	}
}
