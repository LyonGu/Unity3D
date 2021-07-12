using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

public class Odin_ex5 : MonoBehaviour
{
    [ShowInInspector] //仅仅是展示，但没有序列化 dissable
    public int GUIDisabledProperty { get { return 10; } }

    [ShowInInspector, EnableGUI]
    public int GUIEnabledProperty { get { return 10; } }
}
