using UnityEngine;

public class Test : MonoBehaviour
{
    public Canvas canvas;

    private LineDrawer drawer;
    private RectTransform canvasTransform;

    void Start()
    {
        canvasTransform = canvas.transform as RectTransform;
        drawer = this.GetComponent<LineDrawer>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasTransform, Input.mousePosition,
                canvas.worldCamera, out Vector2 pos);
            drawer.AddPointer(pos);
        }
    }
}

