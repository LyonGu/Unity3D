using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

/*
 Delays applying changes to properties while they still being edited in the inspector. 
 Similar to Unity's built-in Delayed attribute, but this attribute can also be applied to properties.
     */
public class Odin_ex3 : MonoBehaviour
{
    // Delayed and DelayedProperty attributes are virtually identical...
    [Delayed]
    [OnValueChanged("OnValueChanged")]
    public int DelayedField;

    // ... but the DelayedProperty can, as the name suggests, also be applied to properties.
    [ShowInInspector, DelayedProperty]
    [OnValueChanged("OnValueChanged")]
    public string DelayedProperty { get; set; }

    private void OnValueChanged()
    {
        Debug.Log("Value changed!");
    }
}
