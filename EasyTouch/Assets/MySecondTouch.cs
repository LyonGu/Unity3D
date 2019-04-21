
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

public class MySecondTouch : MonoBehaviour {

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
        Debug.Log(GetType()+"Touch in " + gesture.position);
        // Verification that the action on the object
        if (gesture.pickObject == gameObject)
        {

            Renderer render = gameObject.GetComponent<Renderer>();
            Material matermal = render.material;
            matermal.color = new Color(Random.Range(0.0f, 1.0f),Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
        }
           
    }
}
