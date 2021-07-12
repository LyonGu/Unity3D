using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

public class Odin_ex21 : MonoBehaviour
{
    // Inline Buttons:
    [InlineButton("A")]
    public int InlineButton;

    [InlineButton("A")]
    [InlineButton("B", "Custom Button Name")]
    public int ChainedButtons;

    private void A()
    {
        Debug.Log("A");
    }

    private void B()
    {
        Debug.Log("B");
    }
}
