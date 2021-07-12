using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

public class Odin_ex10 : MonoBehaviour
{
    [ReadOnly] //只读，属性为dissable状态
    public string MyString = "This is displayed as text";

    [ReadOnly]
    public int MyInt = 9001;

    [ReadOnly]
    public int[] MyIntList = new int[] { 1, 2, 3, 4, 5, 6, 7, };
}
