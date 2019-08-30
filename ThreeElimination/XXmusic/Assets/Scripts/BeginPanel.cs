using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BeginPanel : MonoBehaviour
{
    public static BeginPanel instance;

    // Use this for initialization
    void Start ()
    {
        instance = this;
        this.transform.Find("btnBegin").GetComponent<Button>().onClick.AddListener(BeginGame);
    }
    private void BeginGame()
    {
        ChoosePanel.instance.choosepanel.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update ()
    {
       
    }
}
