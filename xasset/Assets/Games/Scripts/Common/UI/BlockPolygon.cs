//------------------------------------------------------------------------------
// Copyright (c) 2018-2020 Beijing Bytedance Technology Co., Ltd.
// All Right Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
//------------------------------------------------------------------------------

//设置自定义点击区域，配合GameListener使用
namespace HxpGame.UI
{
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// The image with polygon block.
    /// </summary>
    [AddComponentMenu("UI/Block Polygon")]
    [RequireComponent(typeof(PolygonCollider2D))]
    public sealed class BlockPolygon : Graphic, ICanvasRaycastFilter
    {
        private PolygonCollider2D polygon = null;

        /// <inheritdoc/>
        public override Texture mainTexture => null;

        /// <inheritdoc/>
        public override Material materialForRendering => null;

        private PolygonCollider2D Polygon
        {
            get
            {
                if (this.polygon == null)
                {
                    this.polygon = this.GetComponent<PolygonCollider2D>();
                }

                return this.polygon;
            }
        }

        /// <inheritdoc/>
        public bool IsRaycastLocationValid(
            Vector2 screenPoint, Camera eventCamera)
        {
            if (eventCamera != null)
            {
                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    this.rectTransform,
                    screenPoint,
                    eventCamera,
                    out var worldPoint))
                {
                    return this.Polygon.OverlapPoint(worldPoint);
                }

                return false;
            }

            return this.Polygon.OverlapPoint(screenPoint);
        }

        /// <inheritdoc/>
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }

#if UNITY_EDITOR
        /// <inheritdoc/>
        protected override void Reset()
        {
            base.Reset();
            this.transform.localPosition = Vector3.zero;
            float w = (this.rectTransform.sizeDelta.x * 0.5f) + 0.1f;
            float h = (this.rectTransform.sizeDelta.y * 0.5f) + 0.1f;
            this.Polygon.points = new Vector2[]
            {
                new Vector2(-w, -h),
                new Vector2(w, -h),
                new Vector2(w, h),
                new Vector2(-w, h),
            };
        }
#endif
    }
}
