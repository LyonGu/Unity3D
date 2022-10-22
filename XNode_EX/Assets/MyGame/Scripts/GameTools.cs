using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static  class GameTools 
{
    public static bool IsInRect(Rect rect,Vector2 pos)
    {
        return rect.Contains(pos);
    }
    public static bool IsInScreen(Vector2 pos)
    {
        Rect rect = new Rect(-GameInfo.screenX, -GameInfo.screenY, GameInfo.screenX*2, GameInfo.screenY * 2);
        return rect.Contains(pos);
    }
    public static Vector2 ToVector2(this Vector3 _vector3, params object[] _values)
    {
        Vector2 v2 = new Vector2(_vector3.x, _vector3.y);
        return v2;
    }
    public static void LookAt2D(this Transform trans, Vector2 oriPos, Vector2 targetPos)
    {
        Vector2 v = targetPos - oriPos;
        trans.up = v;
    }
    public static void LookAt2D(this Transform trans, Transform target)
    {
        Vector2 v = target.position.ToVector2() - trans.position.ToVector2();
        trans.up = v;
    }
    public static void LookAt2D(this Transform trans, Vector2 targetPos)
    {
        Vector2 v = targetPos - trans.position.ToVector2();
        trans.up = v;
    }
}
