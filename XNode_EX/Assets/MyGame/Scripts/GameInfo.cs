using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInfo
{
    static float _mScreenX;
    public static float screenX
    {
        get
        {
            if (_mScreenX == 0&& Camera.main!=null)
            {
                _mScreenX = -Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).x;
            }
            
            return _mScreenX;
        }
    }
    static float _mScreenY;
    public static float screenY
    {
        get
        {
            if (_mScreenY == 0)
            {
                _mScreenY = -Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).y;
            }
            return _mScreenY;
        }
    }
}
