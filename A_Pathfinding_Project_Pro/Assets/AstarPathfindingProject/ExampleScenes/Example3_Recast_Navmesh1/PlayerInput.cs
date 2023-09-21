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
            //用射线计算目标点
            var dir = new Vector3(h, 0, v).normalized;
            var offset = ai.maxSpeed * Time.deltaTime;
            var targetTPos = PlayerMoveTargetTransform.position +  dir * offset;
            var raySourcePos = targetTPos + Vector3.up * 3;
            RaycastHit hit;
            bool positionFound = false;
            if (Physics.Raycast(raySourcePos, Vector3.down, out hit, Mathf.Infinity, 1<<LayerMask.NameToLayer("Default"))) {
                targetTPos = hit.point;
                positionFound = true;
            }

            if (positionFound)
            {
                var curAIPos = ai.position;
                if (Vector3.Distance(curAIPos, targetTPos) < 1.0f)
                {
                    PlayerMoveTargetTransform.position = targetTPos;
                }
                ai.enabled = true;
            }
            
        }

        
    }
}
