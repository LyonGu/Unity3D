using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LatebindImage : Image
{
    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        var _sprite = overrideSprite;
        if(_sprite != null && _sprite.texture == null || Mathf.Approximately(color.a, 0))
        {
            toFill.Clear();
        }
        else base.OnPopulateMesh(toFill);
    }

    
        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();
            if (!raycastTarget)
                GraphicRegistry.UnregisterGraphicForCanvas(canvas, this);
        }
 
 
        protected override void OnCanvasHierarchyChanged()
        {
            base.OnCanvasHierarchyChanged();
            if(!raycastTarget)
                GraphicRegistry.UnregisterGraphicForCanvas(canvas, this); 
        }
 
        protected override void OnEnable()
        {
            base.OnEnable();
            if (!raycastTarget)
                GraphicRegistry.UnregisterGraphicForCanvas(canvas, this);
        }

}
