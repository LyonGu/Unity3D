using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class GPUInstace_LightMap_Shadow_1 : MonoBehaviour
{
    
   
    ShadowCastingMode castShadows; //阴影选项
    
    private MaterialPropertyBlock propertyBlock;
    public Color color = Color.white;

    //这个变量类似于unity5.6材质属性的Enable Instance Variants勾选项
    public bool turnOnInstance = true;

    // Start is called before the first frame update
    void Start()
    {
        var mat = GetComponent<Renderer>().sharedMaterial;
        //全局开启
        mat.EnableKeyword("LIGHTMAP_ON");//开启lightmap

        color = new Color(Random.Range(0, 255)/255.0f, Random.Range(0, 255) / 255.0f, Random.Range(0, 255) / 255.0f, Random.Range(0, 255) / 255.0f);
        castShadows = ShadowCastingMode.On;

        propertyBlock = new MaterialPropertyBlock();

    }


    // Update is called once per frame
    void Update()
    {
        if (turnOnInstance)
        {
            castShadows = ShadowCastingMode.On;
            propertyBlock.SetColor("_Color", color);
            GetComponent<Renderer>().SetPropertyBlock(propertyBlock);
            
        }
    }
}
