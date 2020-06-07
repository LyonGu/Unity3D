using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingalBeCalled : MonoBehaviour
{
    // Start is called before the first frame update

    public void TestSingalCall(string str)
    {
        Debug.Log($"timeLine发射的信号接收到了=========={str}");
    }
}
