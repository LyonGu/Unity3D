
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraCtrls : MonoBehaviour {

    public Transform targetTran;

    private EasyJoystick joystick;
    private Vector3 camareRotateVec3 = Vector3.zero;
    private Vector3 camareTranlateVec3 = Vector3.zero;
    private Vector3 angleAxis = new Vector3(1,1,0);

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

    void On_JoystickDoubleTap(MovingJoystick move)
    {

    }

    void On_JoystickTap(MovingJoystick move)
    {

    }

    void On_JoystickTouchUp(MovingJoystick move)
    {

    }

    void On_JoystickMoveEnd(MovingJoystick move)
    {
        Debug.Log("On_JoystickMoveEnd=========");
    }

    void On_JoystickMove(MovingJoystick move)
    {
        Debug.Log("On_JoystickMove=========");

        Vector2 speed = joystick.speed;
        float joystickValueX = move.joystickValue.x;
        float joystickValueY = move.joystickValue.y;

        camareRotateVec3 = Vector3.zero;
        camareTranlateVec3 = Vector3.zero;
        if (move.joystickName == "Horizontal")
        {
            camareRotateVec3.y = joystickValueX;
            camareTranlateVec3.z = joystickValueY;
        }
        else if (move.joystickName == "Vertical")
        {
            camareRotateVec3.x = joystickValueY;
        }

        //方法1
        targetTran.Rotate(camareRotateVec3);
        targetTran.Translate(camareTranlateVec3);

        //方法2
        //Quaternion targetY = Quaternion.AngleAxis(joystickValueX, Vector3.up);
        //Quaternion targetX = Quaternion.AngleAxis(joystickValueY, Vector3.right);
        //targetTran.eulerAngles += camareRotateVec3;
        //targetTran.position += camareTranlateVec3;
       
        //move.joystickValue 包含了速度
        //Debug.Log("joystickAxis x:" + move.joystickAxis.x + "y:" + move.joystickAxis.y);
        //Debug.Log("joystickValue x:" + move.joystickValue.x + "y:" + move.joystickValue.y);
    }


    void On_JoystickMoveStart(MovingJoystick move)
    {
        Debug.Log("On_JoystickMoveStart=========");
    }

    void On_JoystickTouchStart(MovingJoystick move)
    {
        joystick = move.joystick;
        Debug.Log("On_JoystickTouchStart=========");
    }
}
