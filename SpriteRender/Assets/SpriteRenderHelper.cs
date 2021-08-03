using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteRenderHelper : MonoBehaviour
{

    public SpriteRenderer spriteRenderer;
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer.lightProbeUsage = 0;
        spriteRenderer.reflectionProbeUsage = 0;
    }

}
