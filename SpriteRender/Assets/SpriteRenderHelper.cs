using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteRenderHelper : MonoBehaviour
{

    public SpriteRenderer spriteRenderer;

    public Camera UICamera;
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer.lightProbeUsage = 0;
        spriteRenderer.reflectionProbeUsage = 0;

        Debug.Log($"WorldPosition Z  {this.transform.position.z}   {this.gameObject.name}");
        Debug.Log($"LocalPosition Z  {this.transform.localPosition.z}   {this.gameObject.name}");
    }

    //应该放到一个单例中，update，不要放到每个实例的
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red, 20);
            if (Physics.Raycast(ray, out var hit))
            {
                Debug.Log($"Hit something  {hit.collider.gameObject.name}");
            }

        }
        

    }

}
