﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GPUInstace_LightMap_Shadow : MonoBehaviour
{
    //草材质用到的mesh
    Mesh mesh;
    Material mat;
    public GameObject m_prefab;
    Matrix4x4[] matrix;
    ShadowCastingMode castShadows; //阴影选项
    public int InstanceCount = 10;

    //树的预制体由树干和树叶两个mesh组成
    MeshFilter[] meshFs;
    Renderer[] renders;

    private MaterialPropertyBlock propertyBlock;
    public Color color = Color.white;

    //这个变量类似于unity5.6材质属性的Enable Instance Variants勾选项
    public bool turnOnInstance = true;

    // Start is called before the first frame update
    void Start()
    {
        if (m_prefab == null)
            return;
        //全局开启
        Shader.EnableKeyword("LIGHTMAP_ON");//开启lightmap

        var mf = m_prefab.GetComponent<MeshFilter>();
        if (mf)
        {
            mesh = m_prefab.GetComponent<MeshFilter>().sharedMesh;
            mat = m_prefab.GetComponent<Renderer>().sharedMaterial;
        }
        //如果一个预制体 由多个mesh组成，则需要绘制多少次
        if (mesh == null)
        {
            meshFs = m_prefab.GetComponentsInChildren<MeshFilter>();
        }
        if (mat == null)
        {
            renders = m_prefab.GetComponentsInChildren<Renderer>();
        }
        matrix = new Matrix4x4[InstanceCount];

        castShadows = ShadowCastingMode.On;

        propertyBlock = new MaterialPropertyBlock();
       

        //随机生成位置与缩放
        for (int i = 0; i < InstanceCount; i++)
        {
            ///   random position
            float x = Random.Range(-50, 50);
            float y = Random.Range(-3, 3);
            float z = Random.Range(-50, 50);
            matrix[i] = Matrix4x4.identity;   ///   set default identity
            //设置位置
            matrix[i].SetColumn(3, new Vector4(x, 0.5f, z, 1));  /// 4th colummn: set   position
            //设置缩放
            //matrix[i].m00   = Mathf.Max(1, x);
            //matrix[i].m11   = Mathf.Max(1, y);
            //matrix[i].m22   = Mathf.Max(1, z);
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (turnOnInstance)
        {
            propertyBlock.SetColor("_Color", color);
            //m_prefab.GetComponent<Renderer>().SetPropertyBlock(propertyBlock);
            castShadows = ShadowCastingMode.On;
            if (mesh)
            {
                Graphics.DrawMeshInstanced(mesh, 0, mat, matrix, matrix.Length, propertyBlock, castShadows, true, 0, null);
            }
            else
            {
                for (int i = 0; i < meshFs.Length; ++i)
                {
                    Graphics.DrawMeshInstanced(meshFs[i].sharedMesh, 0, renders[i].sharedMaterial, matrix, matrix.Length, propertyBlock, castShadows, true, 0, null);
                }
            }
        }
    }
}
