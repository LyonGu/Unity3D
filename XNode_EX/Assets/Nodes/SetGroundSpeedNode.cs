using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class SetGroundSpeedNode : GameStateNode
{
	public float speed = 0.5f;
	public float delayTime = 2;
	protected override void Init() {
		base.Init();
		
	}

	public override object GetValue(NodePort port) {
		return null; 
	}
	public override void OnEnter()
	{
			GroundManager.Inst.SetGroundSpeed(this);
	}
	public GroundNode nextGroundNode()
	{
		NodePort exitPort = GetOutputPort("exit");

		if (!exitPort.IsConnected)
		{
			Debug.LogWarning("Node isn't connected");
			return null;
		}
		foreach (var nodePot in exitPort.GetConnections())
		{
			GroundNode nodea = nodePot.node as GroundNode;
			if (nodea == null)
			{

			}
			else
				return nodea;
		}
		return null;
	}
}