using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility;
using XNode;

public class SplineNode : Node {
	[Output] public WaypointCircuit splineResult;
	public WaypointCircuit spline;
	protected override void Init() {
		base.Init();

	}

	public override object GetValue(NodePort port) {
		return spline; 
	}
}