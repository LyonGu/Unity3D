using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTMP : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject testPrefab;
    public Transform parentTranform;
    void Start()
    {
        StartCoroutine(CreateTMP());
    }


    IEnumerator CreateTMP()
    {
        yield return new WaitForSeconds(5);
        GameObject t = GameObject.Instantiate(testPrefab);
        t.transform.SetParent(parentTranform, false);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
