using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseLockProperty : MonoBehaviour
{

    protected MaterialPropertyBlock _propertyBlock;

    void Awake()
    {
        _propertyBlock = new MaterialPropertyBlock();
        //intPropertys();
    }


    void Update()
    {
        GetComponent<Renderer>().GetPropertyBlock(_propertyBlock);
        onChangePropertys();
        GetComponent<Renderer>().SetPropertyBlock(_propertyBlock);
    }


    virtual public void onChangePropertys()
    { 
    
    }

    virtual public void intPropertys()
    { 
    
    }

}
