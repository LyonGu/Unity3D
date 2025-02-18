﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

public class Odin_ex17 : MonoBehaviour
{
    public string ButtonName = "Dynamic button name";

    public bool Toggle;

    [Button("$ButtonName")]
    private void DefaultSizedButton()
    {
        this.Toggle = !this.Toggle;
    }

    [Button("@\"Expression label: \" + DateTime.Now.ToString(\"HH:mm:ss\")")]
    public void ExpressionLabel()
    {
        this.Toggle = !this.Toggle;
    }

    [Button("Name of button")]
    private void NamedButton()
    {
        this.Toggle = !this.Toggle;
    }

    [Button(ButtonSizes.Small)]
    private void SmallButton()
    {
        this.Toggle = !this.Toggle;
    }

    [Button(ButtonSizes.Medium)]
    private void MediumSizedButton()
    {
        this.Toggle = !this.Toggle;
    }

    [DisableIf("Toggle")]
    [HorizontalGroup("Split", 0.5f)]  //水平布局
    [Button(ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1)]
    private void FanzyButton1()
    {
        this.Toggle = !this.Toggle;
    }

    [HideIf("Toggle")] //Toggle为true时，隐藏
    [VerticalGroup("Split/right")]
    [Button(ButtonSizes.Large), GUIColor(0, 1, 0)]
    private void FanzyButton2()
    {
        this.Toggle = !this.Toggle;
    }

    [ShowIf("Toggle")] //Toggle为true时，显示
    [VerticalGroup("Split/right")]
    [Button(ButtonSizes.Large), GUIColor(1, 0.2f, 0)]
    private void FanzyButton3()
    {
        this.Toggle = !this.Toggle;
    }

    [Button(ButtonSizes.Gigantic)]
    private void GiganticButton()
    {
        this.Toggle = !this.Toggle;
    }

    [Button(90)]
    private void CustomSizedButton()
    {
        this.Toggle = !this.Toggle;
    }
}
