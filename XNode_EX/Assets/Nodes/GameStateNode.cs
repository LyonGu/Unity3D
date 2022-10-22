using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public abstract class GameStateNode : Node {

	[Input, HideInInspector] public Empty enter;
	[Output, HideInInspector] public Empty exit;
	[HideInInspector]
	public bool isStating = false;
	public bool led
	{
		get
		{
			if (!Application.isPlaying)
			{
				isStating = false;
			}
			return isStating;
		}
	}
	protected override void Init() {
		base.Init();
		
	}
	public abstract void OnEnter();
	public override object GetValue(NodePort port) {
		return null; 
	}
	public virtual void MoveNext()
	{
		isStating = false;
		NodePort exitPort = GetOutputPort("exit");

        if (!exitPort.IsConnected)
        {
            Debug.LogWarning("Node isn't connected");
            return;
        }
        foreach (var nodePot in exitPort.GetConnections())
        {
            GameStateNode nodea = nodePot.node as GameStateNode;
            nodea.OnEnter();
        }
    }
    [Serializable]
	public class Empty { }
}