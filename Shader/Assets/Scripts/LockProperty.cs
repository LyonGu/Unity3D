using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LockProperty : BaseLockProperty
{

    public Color _EmissiveColor;
    public Color _AmbientColor;
    public float _MySliderValue;

    public override void onChangePropertys()
    {
        _propertyBlock.SetColor("_AmbientColor", _AmbientColor);
        _propertyBlock.SetColor("_AmbientColor", _AmbientColor);
        _propertyBlock.SetFloat("_MySliderValue", _MySliderValue);
    }
}
