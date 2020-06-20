using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier : MonoBehaviour
{
    // Start is called before the first frame update

    public SpriteRenderer renderer;

    private void Start()
    {
        SetSelected(false);
    }


    // Update is called once per frame
    public void SetSelected(bool isSelect)
    {
        Color oldColor = renderer.color;
        oldColor.a = isSelect ? 1.0f : 0.1f;
        renderer.color = oldColor;
    }

    private void OnDestroy()
    {
        SetSelected(false);
    }
}
