
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

public class RecieveMsg : MonoBehaviour {

    void OnEnable()
    {
        EasyJoystick.On_JoystickTouchStart += On_JoystickTouchStart;
        EasyJoystick.On_JoystickMoveStart += On_JoystickMoveStart;
        EasyJoystick.On_JoystickMove += On_JoystickMove;
        EasyJoystick.On_JoystickMoveEnd += On_JoystickMoveEnd;
        //EasyJoystick.On_JoystickTouchUp += On_JoystickTouchUp;
        //EasyJoystick.On_JoystickTap += On_JoystickTap;
        //EasyJoystick.On_JoystickDoubleTap += On_JoystickDoubleTap;
    }

    void OnDisable()
    {
        EasyJoystick.On_JoystickTouchStart -= On_JoystickTouchStart;
        EasyJoystick.On_JoystickMoveStart -= On_JoystickMoveStart;
        EasyJoystick.On_JoystickMove -= On_JoystickMove;
        EasyJoystick.On_JoystickMoveEnd -= On_JoystickMoveEnd;
        //EasyJoystick.On_JoystickTouchUp -= On_JoystickTouchUp;
        //EasyJoystick.On_JoystickTap -= On_JoystickTap;
        //EasyJoystick.On_JoystickDoubleTap -= On_JoystickDoubleTap;
    }

    void OnDestroy()
    {
        EasyJoystick.On_JoystickTouchStart -= On_JoystickTouchStart;
        EasyJoystick.On_JoystickMoveStart -= On_JoystickMoveStart;
        EasyJoystick.On_JoystickMove -= On_JoystickMove;
        EasyJoystick.On_JoystickMoveEnd -= On_JoystickMoveEnd;
        //EasyJoystick.On_JoystickTouchUp -= On_JoystickTouchUp;
        //EasyJoystick.On_JoystickTap -= On_JoystickTap;
        //EasyJoystick.On_JoystickDoubleTap -= On_JoystickDoubleTap;
    }

    void On_JoystickDoubleTap(MovingJoystick move) {
    
    }

    void On_JoystickTap(MovingJoystick move) { 
    
    }

    void On_JoystickTouchUp(MovingJoystick move) {
    
    }

    void On_JoystickMoveEnd(MovingJoystick move) {
        Debug.Log("On_JoystickMoveEnd=========");
    }

    void On_JoystickMove(MovingJoystick move) {
        Debug.Log("On_JoystickMove=========");


        //Debug.Log("joystickAxis x:" + move.joystickAxis.x + "y:" + move.joystickAxis.y);
        //Debug.Log("joystickValue x:" + move.joystickValue.x + "y:" + move.joystickValue.y);
    }


    void On_JoystickMoveStart(MovingJoystick move) {
        Debug.Log("On_JoystickMoveStart=========");
    }

    void On_JoystickTouchStart(MovingJoystick move) {
        Debug.Log("On_JoystickTouchStart=========");
    }

}
