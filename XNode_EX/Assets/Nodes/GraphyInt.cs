using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class GraphyInt : Node {
	public int enermyCount;
	[Output] public int enermyCountResult;
	// Use this for initialization
	protected override void Init() {
		base.Init();
		
	}

    // Return the correct value of an output port when requested
    public override object GetValue(NodePort port)
    {
        return enermyCount; // Replace this
    }
}