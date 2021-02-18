using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CustomPointDown : MonoBehaviour, IPointerDownHandler , ICanvasRaycastFilter
{
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("CustomPointDown OnPointerDown=========== ");
    }



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        bool isContain = RectTransformUtility.RectangleContainsScreenPoint(this.transform as RectTransform, screenPoint, eventCamera);
        Debug.Log($"CustomPointDown IsRaycastLocationValid {isContain}=========== ");
        return isContain;
    }
}
