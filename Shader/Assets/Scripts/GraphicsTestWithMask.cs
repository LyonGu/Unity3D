using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphicsTestWithMask : MonoBehaviour
{
    // Start is called before the first frame update

    public Image bgImage;
    private RenderTexture _rt0;

    public Sprite sprite;
    public RawImage rawimage;
    private Material _material;
    public RawImage maskRawimage;
    private Texture2D maskTexture;
    float TXW; //512 原始图长
    float TXH; //512 原始图宽
    int WCount; //水平格子数量
    int HCount; //竖直格子数量
    int drawIndex = 0;

    public class GridData
    {
        public Vector2 center = Vector2.zero;
        public int index;
        public List<Vector2> posList = new List<Vector2>();
    }

    private List<GridData> GridList = new List<GridData>();  //存储所有原始图的格子划分区域坐标
    private List<Vector2> useGridList = new List<Vector2>();
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
        //for (int i = 0; i < 1; i++)
        //{
        //    float posx = 64 + tarW * i;
        //    for (int j = 0; j < 1; j++)
        //    {
        //        float posy = 64 + tarH * j;
        //        Vector4 uvRange = GetUVRangeByTargetTexture(posx, posy);
        //        _material.SetVector("_UVRange", uvRange);
        //        _material.SetTexture("_ShowTex", sprite.texture);
        //        Graphics.Blit(rawimage.texture, _rt0, _material);
        //        rawimage.texture = _rt0;
        //    }
        //}
        List<float> posXList = new List<float>()
        {
            100, 100, 200,300,200
        };

        List<float> posYList = new List<float>()
        {
            100, 200, 200,150,100
        };

        TXW = rectTrans.rect.width; //512
        TXH = rectTrans.rect.height; //512
        maskTexture = new Texture2D((int)TXW, (int)TXH);
        for (int i = 0; i < 512; i++)
        {
            for (int j = 0; j < 512; j++)
            {
                //默认初始化为全白色
                maskTexture.SetPixel(i, j, Color.white);
            }
        }
        maskTexture.Apply();
        maskRawimage.texture = maskTexture;

        //利用Mask描绘区域
        _material.SetTexture("_ShowTex", sprite.texture);
        _material.SetTexture("_MaskTex", maskTexture);
        //Graphics.Blit(rawimage.texture, _rt0, _material);
        //rawimage.texture = _rt0;


        /*
           使用不同的方格一帧一帧画

            1 初始化格子数据放入list
            2 间隔修改mask图
        */

        float gridW = 51.2f;
        float gridH = 51.2f;
        float halfGridW = gridW * 0.5f;
        float halfGridH = gridH * 0.5f;

        float tWCount = TXW / gridW;
        float tHCount = TXH / gridH;
        WCount = (int)tWCount;
        HCount = (int)tHCount;
        int count = 0;
        for (int i = 0; i < WCount; i++)
        {
            for (int j = 0; j < HCount; j++)
            {
                //先得到中心点
                float centerX = halfGridW + j * gridW;
                float centerY = halfGridH + i * gridH;

                //得到四个角的点
                Vector2 pos1 = new Vector2(centerX - halfGridW, centerY - halfGridH);
                Vector2 pos2 = new Vector2(centerX - halfGridW, centerY + halfGridH);
                Vector2 pos3 = new Vector2(centerX + halfGridW, centerY + halfGridH);
                Vector2 pos4 = new Vector2(centerX + halfGridW, centerY - halfGridH);

                GridData data = new GridData();
                data.center.x = centerX;
                data.center.y = centerY;
                data.index = count;
                data.posList.Add(pos1);
                data.posList.Add(pos2);
                data.posList.Add(pos3);
                data.posList.Add(pos4);
                count++;

                GridList.Add(data);
            }
        }
    }


    void DrawGridToMaskTex(List<Vector2> posList)
    {
        if (maskTexture == null)
        {
            maskTexture = new Texture2D((int)TXW, (int)TXH);
        }

        for (int i = 0; i < (int)TXW; i++)
        {
            for (int j = 0; j < (int)TXH; j++)
            {
                bool isOk = IsInPolygon1(i, j, posList);
                if (isOk)
                {
                    //默认是全白的，只有在区域内才变黑，这样也能保留之前的区域（区域之间不会重合）
                    maskTexture.SetPixel(i, j, Color.black);
                }
            }
        }

        maskTexture.Apply();
        maskRawimage.texture = maskTexture;

        //利用Mask描绘区域
        _material.SetTexture("_ShowTex", sprite.texture);
        _material.SetTexture("_MaskTex", maskTexture);
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
        _material.SetTexture("_MaskTex", null);
        if (_rt0 != null)
        {
            RenderTexture.ReleaseTemporary(_rt0);
        }

    }

    private void Update()
    {
        if (Time.frameCount % 4 == 0)
        {
            if (drawIndex < GridList.Count)
            {
                var gridData = GridList[drawIndex];
                DrawGridToMaskTex(gridData.posList);
                drawIndex++;
            }

        }
    }


    public  bool IsInPolygon(float targetX, float targetY, List<float> posXList, List<float> posYList)
    {

        if (posXList.Count == 0 || posYList.Count == 0)
        {
            return false;
        }

        if (posXList.Count != posYList.Count)
        {
            return false;
        }

        int counter = 0;
        int i;
        float xinters;
        float p1_x = 0;
        float p1_y = 0;
        float p2_x = 0;
        float p2_y = 0;
        int pointCount = posXList.Count;
        p1_x = posXList[0];
        p1_y = posYList[0];
        for (i = 1; i <= pointCount; i++)
        {
            p2_x = posXList[i % pointCount];
            p2_y = posYList[i % pointCount];
            if (targetY > Mathf.Min(p1_y, p2_y)//校验点的Y大于线段端点的最小Y
                && targetY <= Mathf.Max(p1_y, p2_y))//校验点的Y小于线段端点的最大Y
            {
                if (targetX <= Mathf.Max(p1_x, p2_x))//校验点的X小于等线段端点的最大X(使用校验点的左射线判断).
                {
                    if (p1_y != p2_y)//线段不平行于X轴
                    {
                        xinters = (targetY - p1_y) * (p2_x - p1_x) / (p2_y - p1_y) + p1_x;
                        if (p1_x == p2_x || targetX <= xinters)
                        {
                            counter++;
                        }
                    }
                }

            }
            p1_x = p2_x;
            p1_y = p2_y;
        }

        if (counter % 2 == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public bool IsInPolygon1(float targetX, float targetY, List<Vector2> posList)
    {

 

        int counter = 0;
        int i;
        float xinters;
        float p1_x = 0;
        float p1_y = 0;
        float p2_x = 0;
        float p2_y = 0;
        int pointCount = posList.Count;
        p1_x = posList[0].x;
        p1_y = posList[0].y;
        for (i = 1; i <= pointCount; i++)
        {
            p2_x = posList[i % pointCount].x;
            p2_y = posList[i % pointCount].y;
            if (targetY > Mathf.Min(p1_y, p2_y)//校验点的Y大于线段端点的最小Y
                && targetY <= Mathf.Max(p1_y, p2_y))//校验点的Y小于线段端点的最大Y
            {
                if (targetX <= Mathf.Max(p1_x, p2_x))//校验点的X小于等线段端点的最大X(使用校验点的左射线判断).
                {
                    if (p1_y != p2_y)//线段不平行于X轴
                    {
                        xinters = (targetY - p1_y) * (p2_x - p1_x) / (p2_y - p1_y) + p1_x;
                        if (p1_x == p2_x || targetX <= xinters)
                        {
                            counter++;
                        }
                    }
                }

            }
            p1_x = p2_x;
            p1_y = p2_y;
        }

        if (counter % 2 == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

}

