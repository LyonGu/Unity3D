using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Mesh， Mesh Filter ， MeshRender
public class CreateMesh2 : MonoBehaviour
{
    public Texture2D texture;
    public MeshRenderer meshRender;
    public MeshFilter meshFilter;
    public Material material;
    private Mesh _mesh;
    [Range(0,1)]
    public float progress = 1.0f;
    
    //偏移量
    public float OffsetY;
    public float OffsetX;

    void Start()
    {
        // mesh meshFilter
        _mesh = new Mesh();
        _mesh.name = "CustomMesh";
        meshFilter.mesh = _mesh;
        
        material.SetTexture("_MainTex",texture);
        meshRender.material = material;

        InitMesh();
    }

    private void InitMesh()
    {

        //顶点信息 注意原点在中间的位置
        //当前位置乘以Matrix4x4.identity后，这里的顶点信息 最后参与计算，
        //其实可以用顶点数组来控制位置
        float vertProgress = progress * 2 -1;
        Vector3[] vec = new Vector3[4]
        {
            new Vector3(1* vertProgress + OffsetX+2.0f,1+ OffsetY,0), //右上 0
            new Vector3(1* vertProgress+ OffsetX+2.0f,-1+ OffsetY,0), //右下 1
            new Vector3(-1+ OffsetX,-1+ OffsetY,0), //左下 2
            new Vector3(-1+ OffsetX,1+ OffsetY,0) //左上 3
        };


        _mesh.vertices = vec;

        
        //UV信息 跟 顶点信息 一一对应
        Vector2[] uv = new Vector2[4]{
            new Vector2(2.0f * progress, 1),
            new Vector2(2.0f * progress, 0),
            new Vector2(0, 0),
            new Vector2(0, 1)
        };
        _mesh.uv = uv;
        
        //三角形索引 顶点的索引下标 需要顺时针
        int[] triangles = new int[2 * 3]{
            0, 1, 2, 　　2, 3, 0
        };

        _mesh.triangles = triangles;
    }
}
