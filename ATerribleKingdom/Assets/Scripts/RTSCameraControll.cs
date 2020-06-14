using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSCameraControll : MonoBehaviour
{
    // Start is called before the first frame update

    public Transform transform;
    public float speed = 10.0f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (transform != null)
        {
            float h = Input.GetAxis("CameraHorizontal");
            float v = Input.GetAxis("CameraVertical");
            transform.position += new Vector3(h, 0, v) * (speed * Time.deltaTime);
        }
     
    }
}
