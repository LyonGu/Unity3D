using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

public class Odin_ex18 : MonoBehaviour
{
    [ButtonGroup]
    private void A()
    {
    }

    [ButtonGroup]
    private void B()
    {
    }

    [ButtonGroup]
    private void C()
    {
    }

    [ButtonGroup]
    private void D()
    {
    }

    [Button(ButtonSizes.Large)]
    [ButtonGroup("My Button Group")]
    private void E()
    {
    }

    [GUIColor(0, 1, 0)]
    [ButtonGroup("My Button Group")]
    private void F()
    {
    }
}
