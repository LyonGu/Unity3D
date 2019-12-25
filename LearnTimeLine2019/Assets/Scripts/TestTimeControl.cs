using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class TestTimeControl : MonoBehaviour, ITimeControl
{
    // Start is called before the first frame update
    public void OnControlTimeStart()
     {
        // 在Clip开始时执行
        Debug.Log("start============");
     }
      public void OnControlTimeStop()
     {
        // 在Clip结束时执行
        Debug.Log("end============");
     }
      public void SetTime(double time)
     {
        // 在Clip每一帧执行，参数是当前clip内的时间
        //Debug.Log("time============"+time);
     }
}
