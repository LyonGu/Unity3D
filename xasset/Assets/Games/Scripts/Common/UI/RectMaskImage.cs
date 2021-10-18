using System.Collections;
using System.Collections.Generic;


namespace HxpGame.UI
{
    
using UnityEngine;

using UnityEngine.Sprites;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class RectMaskImage : Image
{

    [SerializeField]
    [Range(0, 0.5f)]
    [Tooltip("左边距比例")]
    private float RatioLeft = 0.25f;

    [SerializeField]
    [Range(0, 0.5f)]
    [Tooltip("右边距比例")]
    private float RatioRight = 0.25f;

    [SerializeField]
    [Range(0, 0.5f)]
    [Tooltip("上边距比例")]
    private float RatioTop = 0.25f;

    [SerializeField]
    [Range(0, 0.5f)]
    [Tooltip("下边距比例")]
    private float RatioBtm = 0.25f;

    private UIVertex[] verts = new UIVertex[4];

#if UNITY_EDITOR
    private void Update()
    {
        this.SetVerticesDirty();
    }
#endif


    protected override void OnPopulateMesh(VertexHelper vh)
    {

        vh.Clear();
        var pivot = this.rectTransform.pivot;
        var rect = this.rectTransform.rect;
        var tw = rect.width;
        var th = rect.height;

        var halfTw = tw * 0.5f;
        var halfTh = th * 0.5f;

        var uv = this.overrideSprite != null ?
          DataUtility.GetOuterUV(this.overrideSprite) : Vector4.zero;
        var uvCenterX = (uv.x + uv.z) * 0.5f;
        var uvCenterY = (uv.y + uv.w) * 0.5f;
        var uvScaleX = (uv.z - uv.x) / tw;
        var uvScaleY = (uv.w - uv.y) / th;

        var LeftBtmPos = new Vector2(tw * RatioLeft, th * RatioBtm);
        var LeftTopPos = new Vector2(tw * RatioLeft, th * (1 - RatioTop));
        var RightTopPos = new Vector2(tw * (1 - RatioRight), th * (1 - RatioTop));
        var RightBtmPos = new Vector2(tw * (1 - RatioRight), th * RatioBtm);


        //UIVertex[] verts = new UIVertex[4];

        var position0 = new Vector3(LeftBtmPos.x - halfTw, LeftBtmPos.y - halfTh);
        verts[0].position = position0;
        verts[0].color = this.color;
        verts[0].uv0 = new Vector2(uvCenterX + position0.x * uvScaleX, uvCenterY + position0.y * uvScaleY);

        var position1 = new Vector3(LeftTopPos.x - halfTw, LeftTopPos.y - halfTh);
        verts[1].position = position1;
        verts[1].color = this.color;
        verts[1].uv0 = new Vector2(uvCenterX + position1.x * uvScaleX, uvCenterY + position1.y * uvScaleY);

        var position2 = new Vector3(RightTopPos.x - halfTw, RightTopPos.y - halfTh);
        verts[2].position = position2;
        verts[2].color = this.color;
        verts[2].uv0 = new Vector2(uvCenterX + position2.x * uvScaleX, uvCenterY + position2.y * uvScaleY);

        var position3 = new Vector3(RightBtmPos.x - halfTw, RightBtmPos.y - halfTh);
        verts[3].position = position3;
        verts[3].color = this.color;
        verts[3].uv0 = new Vector2(uvCenterX + position3.x * uvScaleX, uvCenterY + position3.y * uvScaleY);

        vh.AddUIVertexQuad(verts);

    }
}
}
