
/***
 *
 *  Title: "Guardian" 项目
 *         描述：
 *
 *  Description:
 *        功能：
 *       
 *
 *  Date: 2019
 * 
 *  Version: 1.0
 *
 *  Modify Recorder:
 *     
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyFirstTouch : MonoBehaviour {

    // Subscribe to events
    void OnEnable()
    {
        EasyTouch.On_TouchStart += On_TouchStart;
    }
    // Unsubscribe
    void OnDisable()
    {
        EasyTouch.On_TouchStart -= On_TouchStart;
    }
    // Unsubscribe
    void OnDestroy()
    {
        EasyTouch.On_TouchStart -= On_TouchStart;
    }
    // Touch start event
    public void On_TouchStart(Gesture gesture)
    {
        //获取屏幕坐标
        Debug.Log(GetType() + "Touch in " + gesture.position);
    }
}
