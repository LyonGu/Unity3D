using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

public class Odin_ex13 : MonoBehaviour
{
    [ShowInInspector]
    private int myPrivateInt;

    [ShowInInspector]
    public int MyPropertyInt { get; set; }

    [ShowInInspector]
    public int ReadOnlyProperty
    {
        get { return this.myPrivateInt; }
    }

    [ShowInInspector]
    public static bool StaticProperty { get; set; }

    [SerializeField, HideInInspector]
    private int evenNumber;

    [ShowInInspector]
    public int EvenNumber
    {
        get { return this.evenNumber; }
        set { this.evenNumber = value + value % 2; }
    }


}
