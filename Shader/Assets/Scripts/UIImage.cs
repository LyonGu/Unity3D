using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIImage : MonoBehaviour
{

    MaterialPropertyBlock _propertyBlock;

    public Shader grayShader;

    private void Awake()
    {
        _propertyBlock = new MaterialPropertyBlock();
    }
    // Start is called before the first frame update
    void Start()
    {
        // var render = GetComponent<Renderer>();
        // _propertyBlock.SetColor("_Color", Color.red);
        // render.SetPropertyBlock(_propertyBlock);
        //var material = this.GetComponent<Image>().material;
        //material.SetFloat("_GrayOff", 0);

        var grayNewMaterail = new Material(grayShader);
        this.GetComponent<Image>().material = grayNewMaterail;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
