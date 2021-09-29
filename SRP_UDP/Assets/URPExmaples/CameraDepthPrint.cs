using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDepthPrint : MonoBehaviour
{
    public Camera Camera1;
    public Camera Camera2;
    public Camera Camera3;
    // Start is called before the first frame update
    void Start()
    {
        float depth1 = Camera1.depth;
        float depth2 = Camera2.depth;
        float depth3 = Camera3.depth;
        Debug.Log($"=============={depth1} {depth2} {depth3}");
    }

    // Update is called once per frame

}
