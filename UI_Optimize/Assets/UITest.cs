using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITest : MonoBehaviour
{
    // Start is called before the first frame update

    public RectTransform parentTrans;

    void Start()
    {
        Debug.Log("UITest Start===========");
        this.transform.SetParent(parentTrans, false);
    }

    // Update is called once per frame
    void Update()
    {
       // Debug.Log($"UITest Update==========={this.gameObject.activeSelf}");
    }

    private void OnEnable()
    {
        Debug.Log($"UITest OnEnable==========={this.gameObject.activeSelf}");
    }

    private void OnDisable()
    {
        Debug.Log($"UITest OnDisable==========={this.gameObject.activeSelf}");
    }
}
