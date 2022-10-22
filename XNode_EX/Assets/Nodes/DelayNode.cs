using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class DelayNode : GameStateNode {
	public float delayTime;

	protected override void Init() {
		base.Init();		
	}

	public override object GetValue(NodePort port) {
		return null; 
	}
    public override void OnEnter()
    {
		isStating = true;
		GameLevel.Inst.EnterDelayNode(delayTime, this);
	}
}