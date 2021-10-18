//------------------------------------------------------------------------------
// Copyright (c) 2018-2020 Beijing Bytedance Technology Co., Ltd.
// All Right Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
//------------------------------------------------------------------------------

namespace HxpGame.UI
{
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// It used to draw a raw image in circle.
    /// </summary>
    [AddComponentMenu("UI/Circle Raw Image")]
    public sealed class CircleRawImage : RawImage
    {
        [SerializeField]
        [Range(4, 360)]
        private int segmentCount = 36;

        [SerializeField]
        [Range(-100, 100)]
        private int fillPercent = 100;

        /// <summary>
        /// Gets or sets the segment count.
        /// </summary>
        public int SegmentCount
        {
            get => this.segmentCount;

            set
            {
                if (this.segmentCount != value)
                {
                    this.segmentCount = value;
                    this.SetVerticesDirty();
#if UNITY_EDITOR
                    EditorUtility.SetDirty(this.transform);
#endif
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            var pivot = this.rectTransform.pivot;
            var rect = this.rectTransform.rect;
            var outer = -pivot.x * rect.width;

            float tw = this.rectTransform.rect.width;
            float th = this.rectTransform.rect.height;

            var angleByStep = (this.fillPercent / 100f * (Mathf.PI * 2f)) / this.segmentCount;
            var currentAngle = 0.0f;
            var prev = Vector2.zero;
            for (int i = 0; i < this.segmentCount + 1; ++i)
            {
                var c = Mathf.Cos(currentAngle);
                var s = Mathf.Sin(currentAngle);

                var pos0 = prev;
                var pos1 = new Vector2(outer * c, outer * s);
                var pos2 = Vector2.zero;
                var pos3 = Vector2.zero;

                prev = pos1;

                var uv0 = new Vector2(
                    (pos0.x / tw) + 0.5f,
                    (pos0.y / th) + 0.5f);
                var uv1 = new Vector2(
                    (pos1.x / tw) + 0.5f,
                    (pos1.y / th) + 0.5f);
                var uv2 = new Vector2(
                    (pos2.x / tw) + 0.5f,
                    (pos2.y / th) + 0.5f);
                var uv3 = new Vector2(
                    (pos3.x / tw) + 0.5f,
                    (pos3.y / th) + 0.5f);

                var verts = new UIVertex[]
                {
                    new UIVertex { color = this.color, position = pos0, uv0 = uv0 },
                    new UIVertex { color = this.color, position = pos1, uv0 = uv1 },
                    new UIVertex { color = this.color, position = pos2, uv0 = uv2 },
                    new UIVertex { color = this.color, position = pos3, uv0 = uv3 },
                };

                vh.AddUIVertexQuad(verts);

                currentAngle += angleByStep;
            }
        }
    }
}
