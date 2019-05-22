using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PositonConvert : MonoBehaviour {


    public Transform cube1;
    public Transform cube2;

    public Canvas canvas;
    public Text text;
	// Use this for initialization
	void Start () {
        Debug.Log("position1 :"+ cube1.position);
        Debug.Log("position2 :" + cube2.position);
        Debug.Log("localposition1 :" + cube1.localPosition);
        Debug.Log("localposition2 :" + cube2.localPosition);

        worldToScreenInUICamera();
	}


    void worldToScreenInUICamera()
    {


        Vector3 wPos = cube1.position;

        //转换成屏幕坐标
        Vector3 screenPos = Camera.main.WorldToScreenPoint(wPos);
        RectTransform rectTrans = canvas.GetComponent<RectTransform>();

        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, screenPos, canvas.worldCamera, out localPos);

        text.transform.localPosition = new Vector3(localPos.x, localPos.y, 0);

    
    }
	// Update is called once per frame
	void Update () {
		
	}
}
