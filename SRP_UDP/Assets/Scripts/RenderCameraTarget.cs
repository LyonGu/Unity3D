using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RenderCameraTarget : MonoBehaviour
{

    public int index;
    // Start is called before the first frame update
    void Awake()
    {

        var universalAdditionalCameraData = Camera.main.GetComponent<UniversalAdditionalCameraData>();
        universalAdditionalCameraData.SetRenderer(index);

    }

    // Update is called once per frame

}
