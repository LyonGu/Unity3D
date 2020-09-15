using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/* 
 * shader上开启GPUInstance后dc立马变成一批了
    gpu instancing可以在材质属性不同时减少draw call，弥补了动态合批的不足
  */
[ExecuteInEditMode]
public class GPUInstancing : MonoBehaviour
{

    public Vector2 center = new Vector2(0.5f, 0.5f);

    private MaterialPropertyBlock propertyBlock;

    void Start()
    {
        propertyBlock = new MaterialPropertyBlock();
    }

    // Update is called once per frame
    void Update()
    {
        propertyBlock.SetFloat("_CenterX", center.x);
        propertyBlock.SetFloat("_CenterY", center.y);
        GetComponent<Renderer>().SetPropertyBlock(propertyBlock);
    }
}
