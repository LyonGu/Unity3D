using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class TimeLineControlBehaviour : PlayableBehaviour
{
    public MarkerType markType;
    public string markerName;
    public ConditionType contition = ConditionType.Always;
    public override void OnGraphStart (Playable playable)
    {
        
    }

    public bool CheckCondition( )
    {
        switch (contition)
        {
            case ConditionType.Always:
                return true;
            case ConditionType.AllMonsterDead:
                return GameMgr.Instance.IsAllMonsterDead();
            
        }
        return true;
    }
}

public enum MarkerType:byte
{
    Mark,
    JumpToMark,

}

public enum ConditionType
{ 
    Always,
    AllMonsterDead,
}
