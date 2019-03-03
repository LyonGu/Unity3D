/***
 *
 *  Title:
 *         道具组-->使游戏对象旋转
 *
 *  Description:
 *        功能：[本脚本的主要功能描述] 
 *
 *  Date: 2017
 * 
 *  Version: 1.0
 *
 *  Modify Recorder:
 *     
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObj:MonoBehaviour
{
    // 自传速度
    public float FloRotationSpeed = 1F;

    void Update()
    {
        this.transform.Rotate(Vector3.up * FloRotationSpeed, Space.World);
    }
}//Class_end
