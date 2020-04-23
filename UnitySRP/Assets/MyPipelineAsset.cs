using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering / My Pipeline")]
public class MyPipelineAsset : RenderPipelineAsset
{
    [SerializeField]
    bool dynamicBatching = false;
    [SerializeField]
    bool instancing = false;
    // Start is called before the first frame update
    protected override RenderPipeline CreatePipeline()
    {
        return new MyPipeline(dynamicBatching, instancing);
    }
}
