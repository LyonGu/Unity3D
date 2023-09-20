using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{

    public Transform PlayerMoveTargetTransform;

    public AIBase ai;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var h = Input.GetAxis("Horizontal");
        var v = Input.GetAxis("Vertical");
        if (Mathf.Abs(h)<=0.05f && Mathf.Abs(v)<=0.05f)
        {
            ai.enabled = false;
            PlayerMoveTargetTransform.position = ai.position;
        }
        else
        {
            //用射线计算目标点TODO
            Vector3 targetPos = PlayerMoveTargetTransform.position + new Vector3(h, 0, v) * ai.maxSpeed * Time.deltaTime;
            PlayerMoveTargetTransform.position = targetPos;
            ai.enabled = true;
        }

        
    }
}
