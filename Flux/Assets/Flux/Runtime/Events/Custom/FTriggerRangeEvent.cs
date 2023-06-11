using UnityEngine;
namespace Flux
{
    
    [System.Serializable]
    public class CubeRange
    {
        public Vector3 pos;
        public Vector3 rotation; //其实是欧拉角
        public Vector3 size = Vector3.one;
    }
    
    //碰撞器范围Track
    [FEvent("Custom/碰撞器范围Track", typeof(FTriggerRangeTrack))]
    public class FTriggerRangeEvent: FEvent
    {
        public CubeRange cubeRange;
        public string ackId = "0";
    }
}