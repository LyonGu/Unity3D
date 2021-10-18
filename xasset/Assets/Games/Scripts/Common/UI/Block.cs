//------------------------------------------------------------------------------
// Copyright (c) 2018-2020 Beijing Bytedance Technology Co., Ltd.
// All Right Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
//------------------------------------------------------------------------------

namespace HxpGame.UI
{
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Invisible block, used to block the ray cast for UI.
    /// </summary>
    [AddComponentMenu("UI/Block")]
    public sealed class Block : Graphic, ICanvasRaycastFilter
    {
        /// <inheritdoc/>
        public override Texture mainTexture => null;

        /// <inheritdoc/>
        public override Material materialForRendering => null;

        /// <inheritdoc/>
        public bool IsRaycastLocationValid(
            Vector2 screenPoint, Camera eventCamera)
        {
            return true;
        }

        /// <inheritdoc/>
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
    }
}