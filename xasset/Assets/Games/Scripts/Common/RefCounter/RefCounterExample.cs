
using UnityEngine;

public class RefCounterExample : MonoBehaviour
{
    void Start()
    {
        var room = new Room();

        room.EnterPeople();
        room.EnterPeople();
        room.EnterPeople();

        room.LeavePeople();
        room.LeavePeople();
        room.LeavePeople();
    }
}

public class LightTest
{
    public void SwitchOn()
    {
        Debug.Log("开灯");
    }

    public void SwitchOff()
    {
        Debug.Log("关灯");
    }
}

public class Room : SimpleRC
{
    private LightTest mLight = new LightTest();

    public void EnterPeople()
    {
        Debug.Log("进入人了");
        Retain();
        if (RefCount == 1)
        {
            mLight.SwitchOn();
        }
    }

    public void LeavePeople()
    {
        Debug.Log("人出来了");
        Release();
    }

    protected override void OnZeroRef()
    {
        mLight.SwitchOff();
    }
}
