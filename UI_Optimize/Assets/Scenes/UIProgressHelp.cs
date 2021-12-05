using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIProgressHelp : MonoBehaviour
{
    
    public Material mat;
    public Image img;
    [Range(0,361)]
    public float angle = 361;

    private Material targetMat;
    private void Awake()
    {
        targetMat = new Material(mat);
        img.material = targetMat;
    }

    private void Update()
    {
        targetMat.SetFloat("_Angle", angle);
    }

    private void SetUVRect()
    {
        Image img = GetComponent<Image>();
        if (img)
        {

            Vector4 uvRect = UnityEngine.Sprites.DataUtility.GetOuterUV(img.sprite);
            Debug.Log($"uvRect========={uvRect.x} {uvRect.y} {uvRect.z} {uvRect.w}");
            Rect originRect = img.sprite.rect;
            Rect textureRect = img.sprite.textureRect;
            float scaleX = textureRect.width / originRect.width;
            float scaleY = textureRect.height / originRect.height;
            img.material.SetVector("_UVRect", uvRect);
            img.material.SetVector("_UVScale", new Vector4(scaleX, scaleY, 0, 0));
        }
    }
    public void OnEnable()
    {
       
    }
}
