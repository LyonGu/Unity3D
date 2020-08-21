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
    void Start()
    {
        //Texture2D texture = new Texture2D(300, 300);
        //RenderTexture rt0 = RenderTexture.GetTemporary(300, 300, 0);
        //texture.ReadPixels(new Rect(0, 0, rt0.width, rt0.height), 0, 0);
        //texture.Apply();
        //Sprite sp = Sprite.Create(texture, new Rect(0,0,300,300), new Vector2(0.5f,0.5f));
        //bgImage.sprite = sp;

        //public static void Blit(Texture source, RenderTexture dest, Vector2 scale, Vector2 offset);
        _rt0 = RenderTexture.GetTemporary(300, 300, 0);
        Graphics.Blit(sprite.texture, _rt0, Vector2.one * 0.25f, Vector2.zero);

        rawimage.texture = _rt0;
    }

    // Update is called once per frame
  

    private void OnDestroy()
    {
        RenderTexture.ReleaseTemporary(_rt0);
    }
}
