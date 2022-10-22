using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility;
using XNode;
using XNode.Examples.StateGraph;

public class CreateEnermyNode : GameStateNode
{
	[Input] public PlaneNode planeInput;
	[Input, HideInInspector] public int enermyCount;
	[Input] public WaypointCircuit splineInput;
	[Input, HideInInspector] public PositionNode positionNodeInput;
	[HideInInspector]
	public PositionNode positionNode;
	[HideInInspector]
	public PlaneNode _mPlane;
	int _mCurEnermyCount;
	[HideInInspector]
	public WaypointCircuit spline;
	public int curEnermyCount
	{
		get { return _mCurEnermyCount; }
		set {
			_mCurEnermyCount = value;
			if (curEnermyCount <= 0)
			{
				MoveNext();
			}
		}
	}
	
	protected override void Init() {
		base.Init();
	}

	public override object GetValue(NodePort port) {
		return null; 
	}

	public override void OnEnter()
	{
		isStating = true;
		_mPlane = GetInputValue<PlaneNode>("planeInput", this.planeInput);
		spline = GetInputValue<WaypointCircuit>("splineInput", this.splineInput);

				positionNode = GetInputValue<PositionNode>("positionNodeInput", this.positionNodeInput);
				_mCurEnermyCount = 0;
				for (int i = 0; i < positionNode.ishs.Count; i++)
				{
					if (positionNode.ishs[i])
					{
						curEnermyCount++;
					}
				}
				GameLevel.Inst.StartCoroutineDelayCreateEnermys(this);
		if (curEnermyCount == 0)
				{
			Debug.LogError("没有怪物");
		}
	}
	IEnumerator CreateEnermy()
	{
		yield return new WaitForSeconds(1f);
	}
	
}