using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LockHPProperty : BaseLockProperty
{

    public float fillAmcount;

    public override void onChangePropertys()
    {
        _propertyBlock.SetFloat("_fillCount", fillAmcount);
    }
}
