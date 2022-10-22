using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class GameStartNode : GameStateNode {
	protected override void Init() {
		base.Init();
		
	}
	public override object GetValue(NodePort port) {
		return null; 
	}


	public void GameStart()
	{
		MoveNext();
	}
    public override void OnEnter()
    {
        throw new NotImplementedException();
    }

    [Serializable]
	public class Empty { }
}