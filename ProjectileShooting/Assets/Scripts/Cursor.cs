using UnityEngine;

public class Cursor : MonoBehaviour 
{    
	void Update () 
	{
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, float.MaxValue, 1 << LayerMask.NameToLayer("Ground")))
        {
	        //目标点位置
            transform.position = hit.point;
        }
	}
}
