using UnityEngine;
using XNode;

public class GroundNode : GameStateNode {
	public bool isInit = false;
	public Transform ground;
	public float speed = 5;
	public float delayTime = 1;
	public bool isMoveLastScene=false;
	protected override void Init() {
		base.Init();
		
	}

	public override object GetValue(NodePort port) {
		return null; 
	}

    public override void OnEnter()
    {
		if (isInit)
		{
			GroundManager.Inst.GroundInit(this);
			NodePort exitPort = GetOutputPort("exit");
			if(exitPort.IsConnected)
			GroundManager.Inst.SetGround(this);
		}
			
		else
		{
			GroundManager.Inst.SetGround(this);
		}

	}
	public bool NextGround()
	{
		NodePort exitPort = GetOutputPort("exit");
		return exitPort.IsConnected;
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
			
			}else
			return nodea;
		}
		return null;
	}
}