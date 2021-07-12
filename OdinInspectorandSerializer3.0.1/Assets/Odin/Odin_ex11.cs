using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

public class Odin_ex11 : MonoBehaviour
{
    [Required]
    public GameObject MyGameObject;

    [Required("Custom error message.")] //使用自定义常量
    public Rigidbody MyRigidbody;

    [InfoBox("Use $ to indicate a member string as message.")]
    [Required("$DynamicMessage")] //使用自定义变量
    public GameObject GameObject;

    public string DynamicMessage = "Dynamic error message";
}
