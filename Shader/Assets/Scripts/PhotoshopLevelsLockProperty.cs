using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotoshopLevelsLockProperty : BaseLockProperty
{


    public float _inBlack;
    public float _inGamma;
    public float _inWhite;

    public float _outWhite;
    public float _outBlack;

    public float _isAlphaTest;

    public override void intPropertys()
    {
        
    }
    public override void onChangePropertys()
    {

        _propertyBlock.SetFloat("_inBlack", _inBlack);
        _propertyBlock.SetFloat("_inGamma", _inGamma);
        _propertyBlock.SetFloat("_inWhite", _inWhite);

        _propertyBlock.SetFloat("_outWhite", _outWhite);
        _propertyBlock.SetFloat("_outBlack", _outBlack);

        _propertyBlock.SetFloat("_isAlphaTest", _isAlphaTest);
    }
}
