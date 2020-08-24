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
        //_material = rawimage.material;
        //_rt0 = RenderTexture.GetTemporary(300, 300, 0);
        //_material.SetFloat("_UVX", 0.5f);
        //_material.SetFloat("_UVY", 0.5f);
        //_material.SetTexture("_ShowTex", sprite.texture);
        //Graphics.Blit(rawimage.texture, _rt0, _material);
        //rawimage.texture = _rt0;

        //_material.SetFloat("_UVX", 0.5f);
        //_material.SetFloat("_UVY", 1.0f);
        //_material.SetTexture("_ShowTex", sprite.texture);
        //Graphics.Blit(rawimage.texture, _rt0, _material);
        //rawimage.texture = _rt0;


        //测试 图集 : 不能使用图集

        //Texture originTexture = bgImage.sprite.texture;

        //var textureName = originTexture.name;

        //Debug.Log($"textureName is {textureName}");

        //var originTextureTT = new Texture2D(300, 300);
        //var rW = sprite.rect.width;
        //var rH = sprite.rect.height;
        //var textureRect = sprite.rect;
        //var targetTex = new Texture2D((int)rW, (int)rH);
        //var pixels = bgImage.sprite.texture.GetPixels(
        //    (int)textureRect.x,
        //    (int)textureRect.y,
        //    (int)textureRect.width,
        //    (int)textureRect.height);
        //targetTex.SetPixels(pixels);
        //targetTex.Apply();

        //_material = rawimage.material;
        //_rt0 = RenderTexture.GetTemporary(300, 300, 0);
        //_material.SetFloat("_UVX", 0.5f);
        //_material.SetFloat("_UVY", 0.5f);
        //_material.SetTexture("_ShowTex", targetTex);
        //Graphics.Blit(originTextureTT, _rt0, _material);
        //rawimage.texture = _rt0;
        //rawimage.texture = targetTex;

        //float posx = 64;
        //float posy = 64;
        //_material = rawimage.material;
        //_rt0 = RenderTexture.GetTemporary(512, 512, 0);
        //Vector4 uvRange = GetUVRangeByTargetTexture(posx, posy);
        //_material.SetVector("_UVRange", uvRange);
        //_material.SetTexture("_ShowTex", sprite.texture);
        //Graphics.Blit(rawimage.texture, _rt0, _material);
        //rawimage.texture = _rt0;

        //posx = 64 + 128;
        //posy = 64;
        //uvRange = GetUVRangeByTargetTexture(posx, posy);
        //_material.SetVector("_UVRange", uvRange);
        //_material.SetTexture("_ShowTex", sprite.texture);
        //Graphics.Blit(rawimage.texture, _rt0, _material);
        //rawimage.texture = _rt0;

        //posx = 64 + 128 * 2;
        //posy = 64;
        //uvRange = GetUVRangeByTargetTexture(posx, posy);
        //_material.SetVector("_UVRange", uvRange);
        //_material.SetTexture("_ShowTex", sprite.texture);
        //Graphics.Blit(rawimage.texture, _rt0, _material);
        //rawimage.texture = _rt0;

        // scale 缩放可以
        RectTransform rectTrans = this.GetComponent<RectTransform>();

        Texture2D targetTex = sprite.texture;
        var tarW = targetTex.width;
        var tarH = targetTex.height;

        _material = rawimage.material;
        _rt0 = RenderTexture.GetTemporary((int)rectTrans.rect.width, (int)rectTrans.rect.height, 0);
        for (int i = 0; i < 4; i++)
        {
            float posx = 64 + tarW * i;
            for (int j = 0; j < 4; j++)
            {
                float posy = 64 + tarH * j;
                Vector4 uvRange = GetUVRangeByTargetTexture(posx, posy);
                _material.SetVector("_UVRange", uvRange);
                _material.SetTexture("_ShowTex", sprite.texture);
                Graphics.Blit(rawimage.texture, _rt0, _material);
                rawimage.texture = _rt0;
            }
        }

        

    }


    Vector4 GetUVRangeByTargetTexture(float posx, float posy)
    {
        var originTexture = rawimage.texture;
        var originTW = originTexture.width;
        var originTH = originTexture.height;

        var targetTexture = sprite.texture;
        Vector2 targetTexturePos = new Vector2(posx, posy); //相对于原始图左下角的位置
        var tarTW = targetTexture.width;
        var tarTH = targetTexture.height;

        var targLeftBtm = targetTexturePos + new Vector2(-tarTW * 0.5f, -tarTH * 0.5f);
        var targRightTop = targetTexturePos + new Vector2(tarTW * 0.5f, tarTH * 0.5f);

        var uv0 = new Vector2(targLeftBtm.x / originTW, targLeftBtm.y / originTH);
        var uv2 = new Vector2(targRightTop.x / originTW, targRightTop.y / originTH);
        return new Vector4(uv0.x, uv2.x, uv0.y, uv2.y);

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

