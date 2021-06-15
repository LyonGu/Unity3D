using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTMP : MonoBehaviour
{
    // Start is called before the first frame update

    public Transform parentTrans;
    
    void Start()
    {
        StartCoroutine("TestTmp");
    }

    IEnumerator TestTmp()
    {
        yield return null;

        yield return new WaitForSeconds(3);

        GameObject gameObject = Resources.Load<GameObject>("TMPGameObject");
        var targetObj = GameObject.Instantiate<GameObject>(gameObject);
        targetObj.transform.SetParent(parentTrans, false);
    }

    // Update is called once per frame
   
}
