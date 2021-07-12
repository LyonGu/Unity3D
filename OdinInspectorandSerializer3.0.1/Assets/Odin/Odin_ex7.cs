using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

public class Odin_ex7 : MonoBehaviour
{
    [Title("Wide Colors")]
    [HideLabel] //隐藏label显示
    [ColorPalette("Fall")]
    public Color WideColor1;

    [HideLabel]
    [ColorPalette("Fall")]
    public Color WideColor2;

    [Title("Wide Vector")]
    [HideLabel]
    public Vector3 WideVector1;

    [HideLabel]
    public Vector4 WideVector2;

    [Title("Wide String")]
    [HideLabel]
    public string WideString;

    [Title("Wide Multiline Text Field")]
    [HideLabel]
    [MultiLineProperty] //多行
    public string WideMultilineTextField = "";
}
