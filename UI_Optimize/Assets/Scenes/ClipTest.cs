using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 挂于裁剪物体身上的脚本，用于设置裁剪区域_ClipRect [Vector4]
/// </summary>
public class ClipTest : MonoBehaviour
{
    private RectMask2D t;
    //作为裁剪区域的物体
    public GameObject go;
    Material mat;
    Vector2 localLbPos;
    Vector2 localrtPos;
    void Start()
    {
        //获取go物体的RectTransform四个角落点的世界坐标位置Vector3
        RectTransform rectTras = go.GetComponent<RectTransform>();
        Vector3[] worldPos = new Vector3[4];
        rectTras.GetWorldCorners(worldPos);

        //获取左下角位置
        mat = GetComponent<Image>().material;

        Vector3 lbPos = worldPos[0];
        Vector3 rtPos = worldPos[2];

        //获取其所在的Canvas
        Canvas canvas = GetComponentInParent<Canvas>();
        RectTransform rectTransCanvas = canvas.GetComponent<RectTransform>();

        //世界转屏幕
        lbPos = Camera.main.WorldToScreenPoint(lbPos);
        //屏幕转Canvas本地坐标点, 若不正确可将第三参数置null
        //Vector2 localLbPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransCanvas, lbPos, Camera.main, out localLbPos);

        //同理
        rtPos = Camera.main.WorldToScreenPoint(rtPos);
        //Vector2 localrtPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransCanvas, rtPos, Camera.main, out localrtPos);

        //设置Shader的裁剪区域
        //用于shader代码：UnityGet2DClipping(IN.worldPosition.xy, _CustomClipRect); //注意：IN.modelPosition是模型空间坐标
        mat.SetVector("_CustomClipRect", new Vector4(localLbPos.x, localLbPos.y, localrtPos.x, localrtPos.y));
        //以此达到裁剪目的, UnityGet2DClipping函数满足模型空间坐标在裁剪区域内就会返回1，否则返回0，可根据具体需求利用好这个返回值进行裁剪（例如乘以最终输出颜色alpha）
    }
}