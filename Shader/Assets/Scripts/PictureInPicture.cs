using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PictureInPicture : MonoBehaviour
{
    // Start is called before the first frame update

    public RawImage rawImage;

    private void OnEnable()
    {
        var material = rawImage.material;
        material.SetTexture("_MainTex", rawImage.texture);
    }

    // Update is called once per frame

}
