using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickManager : MonoBehaviour
{
    // Start is called before the first frame update

    public bool IsPhysics2DCheck = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var mousePos = Input.mousePosition;
            Debug.Log($"mousePos ==== {mousePos.x}  {mousePos.y} {mousePos.z}  {Time.frameCount}  {this.gameObject.name}");

            //var worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //bool isHit = Physics.Raycast(ray.origin, ray.direction, out RaycastHit hitInfo, 100);
            //if (isHit)
            //{
            //    Debug.Log($"Hit something  {hitInfo.collider.gameObject.name}");
            //}

            //Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red, 20);




   

           

            if (IsPhysics2DCheck)
            {
                var worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 mouserPos2D = new Vector2(worldPos.x, worldPos.y);

                RaycastHit2D hit = Physics2D.Raycast(mouserPos2D, Vector2.zero); //Physics2D 只能向上或者向下打射线 这里不使用
                Debug.DrawLine(worldPos, Vector3.up * 1000, Color.red, 10);
                if (hit.collider != null)
                {
                    //var hitGameName = hit.collider.gameObject.name;
                    Debug.Log($"Hit something  {hit.collider.gameObject.name}");
                }
            }
            else
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                //Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red, 20);
                if (Physics.Raycast(ray, out var hit))
                {
                    Debug.Log($"Hit something  {hit.collider.gameObject.name}");
                }
            }
        }

        //Vector3 forward = transform.TransformDirection(Vector3.forward) * 10;
        //Debug.DrawRay(transform.position, forward, Color.green);

        //Debug.DrawRay(transform.position, forward, Color.red, 1000);
    }
}
