using UnityEngine;

public abstract class GameInst<T> : MonoBehaviour where T : MonoBehaviour, new()
{
    static T _inst;
    public static T Inst
    {
        get
        {
            if (_inst == null)
            {
                _inst = FindObjectOfType(typeof(T)) as T;
            }
            return _inst;
        }
    }


}
public abstract class DataInst<T> where T : new()
{
    static T _inst;
    public static T Inst
    {
        get
        {
            if (_inst == null)
            {
                _inst = new T();
            }
            return _inst;
        }
    }


}