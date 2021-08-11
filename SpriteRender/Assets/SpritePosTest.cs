using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpritePosTest : MonoBehaviour
{
    // Start is called before the first frame update

    public RectTransform bgTran;
    public RectTransform contentTran;

    private Vector3 offset;
    void Start()
    {

        var bgPosition = bgTran.position;
        var contenPosition = contentTran.position;
        Debug.Log($"{bgTran.position}   {bgTran.anchoredPosition}");
        Debug.Log($"{contentTran.position}   {contentTran.anchoredPosition}");

        offset.x = contenPosition.x - bgPosition.x;
        offset.y = contenPosition.y - bgPosition.y;
        offset.z = contenPosition.z - bgPosition.z;

    }

    // Update is called once per frame
    void Update()
    {
        float x = contentTran.position.x - offset.x;
        float y = contentTran.position.y - offset.y;
        float z = contentTran.position.z - offset.z;

        bgTran.position = new Vector3(x, y, z);
    }
}
