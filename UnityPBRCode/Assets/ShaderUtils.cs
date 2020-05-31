using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderUtils : MonoBehaviour
{
    // Start is called before the first frame update



    public enum SHADETYPE { 
        MAINTEX_ON,
        LIGINHT_ON,
        NORMAL_ON,
    }

    public SHADETYPE shaderType = SHADETYPE.MAINTEX_ON;

    private Material _mat;
    void Start()
    {
        _mat = this.gameObject.GetComponent<Renderer>().sharedMaterial;
    }


    private void Update()
    {

        switch (shaderType)
        {
            case SHADETYPE.MAINTEX_ON:
                _mat.EnableKeyword("_MainTexOn");
                _mat.DisableKeyword("_LightOn");
                _mat.DisableKeyword("_NormalOn");
                break;
            case SHADETYPE.LIGINHT_ON:
                _mat.EnableKeyword("_LightOn");
                _mat.DisableKeyword("_NormalOn");
                _mat.DisableKeyword("_MainTexOn");
                break;
            case SHADETYPE.NORMAL_ON:
                _mat.EnableKeyword("_NormalOn");
                _mat.DisableKeyword("_LightOn");
                _mat.DisableKeyword("_MainTexOn");
                break;
      
        }
       
    }
    // Update is called once per frame

}
