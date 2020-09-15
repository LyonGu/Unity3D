using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePlay : MonoBehaviour
{
    // Start is called before the first frame update

    public Image uiImage;

    //public string[] ddd;
    void Start()
    {
        
    }


    private void OnTriggerEnter(Collider other)
    {
        string name = other.transform.gameObject.name;
        Debug.Log($"name is {name}");

       
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            RectTransform tran = uiImage.rectTransform;
            var size = tran.sizeDelta;
            var rect = tran.rect;

            Debug.Log($"sizeDelta w: {size.x}, h: {size.y}");
            Debug.Log($"rect w: {rect.width}, h: {rect.height}");
        }
    }
}
