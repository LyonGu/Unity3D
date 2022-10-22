using UnityEngine;
using XNode;

public class PlaneNode : Node {
	public GameObject plane;
	[HideInInspector]
	public float delayTime;
	[Output] public PlaneNode planeResult;
	[Input] public BulletNode bulletNodeResult;
	public float delayShootTime;
	public BulletNode bulletNode
	{
		get {
			return GetInputValue<BulletNode>("bulletNodeResult", this.bulletNodeResult);
		}
	}
	protected override void Init() {
		base.Init();
		
	}

	public override object GetValue(NodePort port) {
		return this; 
	}
}