using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputMgr : MonoBehaviour
{
    // Update is called once per frame
    private bool isDraging = false;
    private Vector2 startPos;
    private Vector2 endPos;
    void Update()
    {
        ProcessInpout();
    }

    private void ProcessInpout()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDraging = true;
            startPos = Input.mousePosition;
        }

        if (isDraging)
        {
            endPos = Input.mousePosition;

            Vector2 center = (startPos + endPos) / 2;
            Vector2 size = new Vector2(Mathf.Abs(endPos.x - startPos.x), Mathf.Abs(endPos.y - startPos.y));
            UIMgr.Instance.SetRectTrangle(center, size);
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDraging = false;
            UIMgr.Instance.ShowRectTrangle(false);
            
        }
    }
}
