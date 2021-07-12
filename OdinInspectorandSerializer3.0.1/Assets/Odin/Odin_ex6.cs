using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

public class Odin_ex6 : MonoBehaviour
{
    [GUIColor(0.3f, 0.8f, 0.8f, 1f)]
    public int ColoredInt1;

    [GUIColor(1.0f, 0.8f, 0.8f, 1f)]
    public int ColoredInt2;

    [ButtonGroup]  //按钮组
    [GUIColor(0, 1, 0)]
    private void Apply()
    {
        Debug.Log("Apply=======");
    }

    [ButtonGroup]  //按钮组
    [GUIColor(1, 0.6f, 0.4f)]
    private void Cancel()
    {
        Debug.Log("Cancel=======");
    }

    [ButtonGroup]  //按钮组
    [GUIColor(1, 1.0f, 0.4f)]
    private void Confirm()
    {
        Debug.Log("Confirm=======");
    }

    [InfoBox("You can also reference a color member to dynamically change the color of a property.")]
    [GUIColor("GetButtonColor")]  //从GetButtonColor方法里返回颜色值
    [Button(ButtonSizes.Gigantic)]
    private static void IAmFabulous()
    {
    }

    private static Color GetButtonColor()
    {
        Sirenix.Utilities.Editor.GUIHelper.RequestRepaint();
        return Color.HSVToRGB(Mathf.Cos((float)UnityEditor.EditorApplication.timeSinceStartup + 1f) * 0.225f + 0.325f, 1, 1);
    }

    [Button(ButtonSizes.Large)]
    [GUIColor("@Color.Lerp(Color.red, Color.green, Mathf.Sin((float)EditorApplication.timeSinceStartup))")]
    private static void Expressive()
    {
    }
}
