using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class BulletNode : Node {
	[Output] public BulletNode bulletNodeResult;
	public GameObject bullet;

	protected override void Init() {
		base.Init();

	}

	public override object GetValue(NodePort port) {
		return this; 
	}
}