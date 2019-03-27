using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockProperty : BaseLockProperty
{

    public Color _EmissiveColor;
    public Color _AmbientColor;
    public float _MySliderValue;

    public override void intPropertys()
    {
        _EmissiveColor = new Color32(165,118,118,255);  
        _AmbientColor = new Color32(154, 77, 77, 255);
        _MySliderValue = 2.5F;
    }
    public override void onChangePropertys()
    {
        _propertyBlock.SetColor("_AmbientColor", _AmbientColor);
        _propertyBlock.SetColor("_AmbientColor", _AmbientColor);
        _propertyBlock.SetFloat("_MySliderValue", _MySliderValue);
    }
}
