using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaskUI : MonoBehaviour
{

    public Image mask;

    // Start is called before the first frame update
    void Start()
    {
        mask.material = new Material(mask.material);
        mask.material.SetFloat("_Stencil",0);
        mask.material.SetFloat("_StencilOp", 2);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
