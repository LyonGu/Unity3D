using UnityEngine;
using UnityEngine.UI;

public class VertIndexAsUV1 : BaseMeshEffect
{
    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
            return;

        UIVertex vert = new UIVertex();
        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref vert, i);
            vert.uv1.x = (i >> 1);
            vert.uv1.y = ((i >> 1) ^ (i & 1));
            Debug.Log($"vert.uv1====={vert.uv1.x} {vert.uv1.y}");
            vh.SetUIVertex(vert, i);
        }
    }
}
