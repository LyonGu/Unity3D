using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIProgressHelp : MonoBehaviour
{
    // Start is called before the first frame update

    public void OnEnable()
    {
        Image img = GetComponent<Image>();
        if (img)
        {
 
            Vector4 uvRect = UnityEngine.Sprites.DataUtility.GetOuterUV(img.overrideSprite);
            Debug.Log($"uvRect========={uvRect.x} {uvRect.y} {uvRect.z} {uvRect.w}");
            Rect originRect = img.sprite.rect;
            Rect textureRect = img.sprite.textureRect;
            float scaleX = textureRect.width / originRect.width;
            float scaleY = textureRect.height / originRect.height;
            img.material.SetVector("_UVRect", uvRect);
            img.material.SetVector("_UVScale", new Vector4(scaleX, scaleY, 0, 0));



            //img.material.SetVector("_UvRect", uvRect);
        }
    }
}
