using UnityEngine;
using System.Collections;
using UnityEditor;

public class ExampleClass : MonoBehaviour
{
    public LineTextureMode textureMode = LineTextureMode.Stretch;
    public float tileAmount = 1.0f;
    private LineRenderer lr;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        lr.material = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Particle.mat");

        // Set some positions
        Vector3[] positions = new Vector3[3];
        positions[0] = new Vector3(-2.0f, -1.0f, 0.0f);
        positions[1] = new Vector3(0.0f, -0.5f, 0.0f);
        positions[2] = new Vector3(2.0f, -1.0f, 0.0f);
        lr.positionCount = positions.Length;
        lr.SetPositions(positions);
    }

    void Update()
    {
        lr.textureMode = textureMode;
        lr.material.SetTextureScale("_MainTex", new Vector2(tileAmount, 1.0f));
    }

    void OnGUI()
    {
        textureMode = GUI.Toggle(new Rect(25, 25, 200, 30), textureMode == LineTextureMode.Tile, "Tiled") ? LineTextureMode.Tile : LineTextureMode.Stretch;

        if (textureMode == LineTextureMode.Tile)
        {
            GUI.Label(new Rect(25, 60, 200, 30), "Tile Amount");
            tileAmount = GUI.HorizontalSlider(new Rect(125, 65, 200, 30), tileAmount, 0.1f, 4.0f);
        }
    }
}