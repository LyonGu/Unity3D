using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIImage : Image
{
    protected override void Awake()
    {
        base.Awake();

        canvasRenderer = GetComponent<CanvasRenderer>();
    }

    public new CanvasRenderer canvasRenderer;

    public new int depth
    {
        get
        {
            return canvasRenderer.absoluteDepth;
        }
    }


    #region 点击事件相应注册部分

    protected override void OnTransformParentChanged()
    {
        base.OnTransformParentChanged();
        if (!raycastTarget)
            GraphicRegistry.UnregisterGraphicForCanvas(canvas, this);
    }


    protected override void OnCanvasHierarchyChanged()
    {
        base.OnCanvasHierarchyChanged();
        if (!raycastTarget)
            GraphicRegistry.UnregisterGraphicForCanvas(canvas, this);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (!raycastTarget)
            GraphicRegistry.UnregisterGraphicForCanvas(canvas, this);
    }

    #endregion；

}
