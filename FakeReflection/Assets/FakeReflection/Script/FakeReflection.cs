using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class FakeReflection : MonoBehaviour
{
    public enum ConvolutionType
    {
        None = 0,
        Specular = 1,
        Diffuse = 2
    }

    public enum ForwardAxis
    {
        X_Axis,
        Y_Axis,
        Z_Axis,
    }

    public enum CubemapSize
    {
        Size1024x1024 = 1024,
        Size512x512 = 512,
        Size256x256 = 256,
        Size128x128 = 128,
        Size64x64 = 64
    }

    [HideInInspector]
    [SerializeField]
    public Vector3 center = Vector3.zero;

    [HideInInspector]
    [SerializeField]
    public Vector3 size = Vector3.one;

    [HideInInspector]
    [SerializeField]
    public Vector3 sizeScale = Vector3.one;

    [HideInInspector]
    [SerializeField]
    public bool twoSide = false;

    [HideInInspector]
    [SerializeField]
    public CubemapSize cubemapSize = CubemapSize.Size512x512;

    [HideInInspector]
    [SerializeField]
    public ForwardAxis forwardAxis = ForwardAxis.Y_Axis;

    [HideInInspector]
    [SerializeField]
    public LayerMask cullingMask = -1;

    [HideInInspector]
    [SerializeField]
    public bool isLocal = true;

    [HideInInspector]
    [SerializeField]
    public bool innerSimulation = false;

    [HideInInspector]
    [SerializeField]
    public ConvolutionType convolutionType = ConvolutionType.Specular;

    [HideInInspector]
    [SerializeField]
    [Range(0, 8)]
    public int roughness = 0;

    [HideInInspector]
    [SerializeField]
    public Cubemap cubemap = null;

    private int fakeReflectionPropId = 0;
    private int fakeReflectionCenterPropId = 0;
    private int fakeReflectionPosPropId = 0;
    private int fakeReflectionSizePropId = 0;
    private int fakeReflectionCullPropId = 0;
    private int roughnessPropId = 0;
    private int forwardAxisPropId = 0;

    private Material mtrl = null;

    private void Awake()
    {
        fakeReflectionPropId = Shader.PropertyToID("_FakeReflectionCube");
        fakeReflectionCenterPropId = Shader.PropertyToID("_FakeReflectionCenter");
        fakeReflectionSizePropId = Shader.PropertyToID("_FakeReflectionSize");
        fakeReflectionPosPropId = Shader.PropertyToID("_FakeReflectionPos");
        roughnessPropId = Shader.PropertyToID("_FakeReflectionRoughness");
        forwardAxisPropId = Shader.PropertyToID("_FakeReflectionForwardAxis");
        fakeReflectionCullPropId = Shader.PropertyToID("_Cull");

        MeshRenderer mr = GetComponent<MeshRenderer>();
        if(mr != null)
        {
            mtrl = mr.sharedMaterial;
        }
    }

    private void Update()
    {
        if(mtrl != null)
        {
            mtrl.SetTexture(fakeReflectionPropId, cubemap);
            mtrl.SetVector(fakeReflectionCenterPropId, transform.position + center);
            mtrl.SetVector(fakeReflectionSizePropId, new Vector3(size.x * sizeScale.x, size.y * sizeScale.y, size.z * sizeScale.z));
            mtrl.SetFloat(roughnessPropId, roughness);
            mtrl.SetVector(fakeReflectionPosPropId, transform.position);

            if(twoSide)
            {
                mtrl.SetInt(fakeReflectionCullPropId, (int)CullMode.Off);
            }
            else
            {
                mtrl.SetInt(fakeReflectionCullPropId, (int)CullMode.Back);
            }

            Vector3 forwardAxisV = Vector3.zero;
            if(forwardAxis == ForwardAxis.X_Axis)
            {
                forwardAxisV.x = 1;
            }
            else if(forwardAxis == ForwardAxis.Y_Axis)
            {
                forwardAxisV.y = 1;
            }
            else if(forwardAxis == ForwardAxis.Z_Axis)
            {
                forwardAxisV.z = 1;
            }
            mtrl.SetVector(forwardAxisPropId, forwardAxisV);

            if(isLocal)
            {
                mtrl.EnableKeyword("_FAKE_REFLECTION_LOCAL");
            }   
            else
            {
                mtrl.DisableKeyword("_FAKE_REFLECTION_LOCAL");
            }

            if(innerSimulation)
            {
                mtrl.EnableKeyword("_FAKE_REFLECTION_INNER");
            }
            else
            {
                mtrl.DisableKeyword("_FAKE_REFLECTION_INNER");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.DrawSphere(transform.position, 0.15f);
        Gizmos.DrawWireCube(transform.position + center, size * 2);
    }
}
