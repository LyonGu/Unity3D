using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CreateCommandBuffer : MonoBehaviour
{

    private Camera _RenderCamera;
    private CommandBuffer _CommandBuffer;

    public Material material;

    private Matrix4x4 matWorld = Matrix4x4.identity;

    private Mesh _mesh;
    public float OffsetX = 0;
    public float OffsetY = 0;
    
    private void InitMesh()
    {
        
        //顶点信息 注意原点在中间的位置
        //当前位置乘以Matrix4x4.identity后，这里的顶点信息 最后参与计算，
        //其实可以用顶点数组来控制位置
        Vector3[] vec = new Vector3[4]
        {
            new Vector3(1 + OffsetX,1+ OffsetY,0), //右上 0
            new Vector3(1 + OffsetX,-1+ OffsetY,0), //右下 1
            new Vector3(-1+ OffsetX,-1+ OffsetY,0), //左下 2
            new Vector3(-1+ OffsetX,1+ OffsetY,0) //左上 3
        };
        _mesh.vertices = vec;
        
        //UV信息 跟 顶点信息 一一对应
        Vector2[] uv = new Vector2[4]{
            new Vector2(1, 1),
            new Vector2(1, 0),
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
    void Start()
    {
        
        _mesh = new Mesh();
        _mesh.name = "CommandBufferMesh";

        InitMesh();
        
        _RenderCamera = Camera.main;
        _CommandBuffer = new CommandBuffer();
        _CommandBuffer.name = "MyCustomCommandBuffer";
        _RenderCamera.AddCommandBuffer(CameraEvent.AfterImageEffects, _CommandBuffer);
        
    }

    // Update is called once per frame
    void Update()
    {
//        if(_CommandBuffer == null)
//            _CommandBuffer = new CommandBuffer();
//        if (_RenderCamera != null)
//        {
//            _RenderCamera.RemoveCommandBuffer(CameraEvent.AfterImageEffects, _CommandBuffer);
//        }
//        _CommandBuffer.Clear();
//        
//        _CommandBuffer.DrawMesh(_mesh,this.transform.localToWorldMatrix, material);
//        _RenderCamera.AddCommandBuffer(CameraEvent.AfterImageEffects, _CommandBuffer);

        if(_CommandBuffer == null)
            _CommandBuffer = new CommandBuffer();
        _CommandBuffer.Clear();
        _CommandBuffer.DrawMesh(_mesh,this.transform.localToWorldMatrix, material);
        
//        _CommandBuffer.DrawMeshInstanced();
    }
}
