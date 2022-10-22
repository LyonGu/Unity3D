using System.Collections.Generic;
using UnityEngine;
using XNode;

public class PositionNode : Node {
    [HideInInspector]
    public List<bool> ishs=new List<bool>();
	public Dictionary<int, bool> isCreateEnermyDicResult= new Dictionary<int, bool>();
	public int posCount = 14;
	[Output, HideInInspector]
	public PositionNode positionNode;
    [HideInInspector]
    public Dictionary<int, bool> isCreateEnermyDic = new Dictionary<int, bool>();
    List<Vector3> _mPosDic ;
    [HideInInspector]
    public int m_lines = 8;
    public int line
    {
        get { return m_lines; }
        set
        {
            if (m_lines == value)
                return;
                m_lines = value;
            
            ishs = new List<bool>();
            Debug.LogError("重置");
        }
    }
    [HideInInspector]
    public int m_row = 8;
    public int row
    {
        get { return m_row; }
        set {
            if (m_row == value)
                return;
            m_row = value;
            ishs = new List<bool>();
        }
    }
    public List<Vector3> posDic
    {
        get {
            if (_mPosDic == null || _mPosDic.Count == 0)
            {
                _mPosDic = new List<Vector3>();
                float posX = -GameInfo.screenX;
                float posY = GameInfo.screenY;
                float width = GameInfo.screenX / ((int)((row + 1) / 2));
                for (int i = 0; i < line; i++)
                {
                    for (int j = 0; j < row; j++)
                    {
                        
                        if (j == 0)
                        {
                            posX = -GameInfo.screenX - ((row & 1) == 0 ? width / 2 : 0);
                        }
                        
                        posX += width;
                        posY = GameInfo.screenY+ 1f * (line-1-i)+0.5f;
                        _mPosDic.Add( new Vector3(posX, posY, 0));
                    }
                }
            }
            return _mPosDic;
        }
    }
    protected override void Init() {
		base.Init();
        _mPosDic = null;
    }

	public override object GetValue(NodePort port) {
		return this; 
	}
}