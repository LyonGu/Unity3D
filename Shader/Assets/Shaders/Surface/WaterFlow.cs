using UnityEngine;
using System.Collections;

public class WaterFlow : MonoBehaviour
{

    public float m_SpeedU = 0.1f;
    public float m_SpeedV = 0.1f;

    private Material _material;

    void Awake()
    {
        Renderer  render = GetComponent<Renderer>();
        _material = render.materials[0];
    }

    // Update is called once per frame
    void Update()
    {
        float newOffsetU = Time.time * m_SpeedU;
        float newOffsetV = Time.time * m_SpeedV;
        _material.mainTextureOffset = new Vector2(newOffsetU, newOffsetV);
    }
}
