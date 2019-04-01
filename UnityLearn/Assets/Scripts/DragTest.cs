using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragTest : MonoBehaviour,IDragHandler ,IPointerDownHandler {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void DragUpdate()
    {
        Debug.Log("DragUpdate=================");
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("OnDrag=================");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("OnPointerDown=================");
    }
}
