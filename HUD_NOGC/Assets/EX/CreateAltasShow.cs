using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//https://www.pianshen.com/article/5694576351/
public class CreateAltasShow : MonoBehaviour
{
    // Start is called before the first frame update
    public Image image;
    void Start()
    {
        var rect = image.rectTransform.rect;
        var tw = rect.width;
        var th = rect.height;

        var halfTw = tw * 0.5f;
        var halfTh = th * 0.5f;
        //UVRect(0.5,0.0,0.8,0.6)
        //==> 水平Uv的范围为0.5~0.8 (第一个和第三个表示水平UV)
        //==> 竖直UV的范围为0.0~0.6 (第二个和第四个表示水平UV)
        //(0.5,0) (0.8,0) (0.8,0.6) (0.5,0.6)
        Vector4 UVRect = UnityEngine.Sprites.DataUtility.GetOuterUV(image.sprite);
        var uvCenterX = (UVRect.x + UVRect.z) * 0.5f;
        var uvCenterY = (UVRect.y + UVRect.w) * 0.5f;
        
        var uvScaleX = (UVRect.z - UVRect.x) / tw;
        var uvScaleY = (UVRect.w - UVRect.y) / th;
        
        //curVertice 顶点的位置
//        uiVertex.uv0 = new Vector2(uvCenterX + curVertice.x * uvScaleX, uvCenterY + curVertice.y * uvScaleY);
        
        Vector4 InnerUVRect = UnityEngine.Sprites.DataUtility.GetInnerUV(image.sprite); //跟GetOuterUV返回一样
        Rect originRect = image.sprite.rect; //原始大小(0,0,37,37)，originRect.x originRect.y (0,0) originRect.width,originRect.height (37,37)
        
        //包括sprite在图集中的偏移量， textureRect.x, textureRect.y
        //包括sprite在图集中显示大小， textureRect.width, textureRect.height
        Rect textureRect = image.sprite.textureRect;  //(64,0,37,37)
        float scaleX = textureRect.width / originRect.width;
        float scaleY = textureRect.height / originRect.height;
//        image.material.SetVector("_UVRect", UVRect);
//        image.material.SetVector("_UVScale", new Vector4(scaleX, scaleY, 0, 0));
   

        Vector2 padding = UnityEngine.Sprites.DataUtility.GetPadding(image.sprite);
        
        Vector2 MinSize = UnityEngine.Sprites.DataUtility.GetMinSize(image.sprite);

        int a = 10;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
