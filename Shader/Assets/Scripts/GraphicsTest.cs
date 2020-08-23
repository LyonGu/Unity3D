using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphicsTest : MonoBehaviour
{
    // Start is called before the first frame update

    public Image bgImage;
    private RenderTexture _rt0;

    public Sprite sprite;
    public RawImage rawimage;
    private Material _material;
    void Start()
    {
        //Texture2D texture = new Texture2D(300, 300);
        //RenderTexture rt0 = RenderTexture.GetTemporary(300, 300, 0);
        //texture.ReadPixels(new Rect(0, 0, rt0.width, rt0.height), 0, 0);
        //texture.Apply();
        //Sprite sp = Sprite.Create(texture, new Rect(0,0,300,300), new Vector2(0.5f,0.5f));
        //bgImage.sprite = sp;

        //public static void Blit(Texture source, RenderTexture dest, Vector2 scale, Vector2 offset);
        //_rt0 = RenderTexture.GetTemporary(300, 300, 0);
        //Graphics.Blit(sprite.texture, _rt0, Vector2.one * 0.25f, Vector2.zero);

        //rawimage.texture = _rt0;



        //Texture2D originTex = bgImage.sprite.texture;

        //Texture2D targetTex = sprite.texture;

        ////BitmapData data = targetTex.LockBits(new Rectangle(0, 0, targetTex.width, targetTex.height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
        ////System.Drawing.Bitmap t1 = new System.Drawing.Bitmap();

        //var tarW = targetTex.width;
        //var tarH = targetTex.height;

        //Texture2D texture = new Texture2D(128, 128);
        //for (int i = 0; i < tarW; i++)
        //{
        //    for (int j = 0; j < tarH; j++)
        //    {

        //        Color c = targetTex.GetPixel(i, j);
        //        texture.SetPixel(i, j, c);

        //    }
        //}
        //texture.Apply();
        ////blic static Sprite Create(Texture2D texture, Rect rect, Vector2 pivot);
        //Sprite bgNewSprite = Sprite.Create(texture, new Rect(0,0,texture.width, texture.height), new Vector2(0.5f,0.5f));

        //bgImage.sprite = bgNewSprite;

        // 测试结果：可以不断 Graphics.Blit
        _material = rawimage.material;
        _rt0 = RenderTexture.GetTemporary(300, 300, 0);
        _material.SetFloat("_UVX", 0.5f);
        _material.SetFloat("_UVY", 0.5f);
        _material.SetTexture("_ShowTex", sprite.texture);
        Graphics.Blit(rawimage.texture, _rt0, _material);
        rawimage.texture = _rt0;

        _material.SetFloat("_UVX", 0.5f);
        _material.SetFloat("_UVY", 1.0f);
        _material.SetTexture("_ShowTex", sprite.texture);
        Graphics.Blit(rawimage.texture, _rt0, _material);
        rawimage.texture = _rt0;

    }

    // Update is called once per frame


    private void OnDestroy()
    {
        if (_rt0 != null)
        {
            RenderTexture.ReleaseTemporary(_rt0);
        }

    }


}

