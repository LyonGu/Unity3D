using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

public class Odin_ex19 : MonoBehaviour
{
    [EnumPaging]
    public SomeEnum SomeEnumField;

    public enum SomeEnum
    {
        A, B, C
    }
}
