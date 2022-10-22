using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNodeEditor;
[CustomNodeEditor(typeof(BulletNode))]
public class BulletNodeEditor : NodeEditor
{
	public override void OnBodyGUI()
	{
		base.OnBodyGUI();
		BulletNode node = target as BulletNode;
		if (node.bullet != null&& node.bullet.GetComponent<SpriteRenderer>()!=null)
		{
			GUILayout.Box(node.bullet.GetComponent<SpriteRenderer>().sprite.texture);
		}
		
		
	}
}
