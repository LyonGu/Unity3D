using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChoosePanel : MonoBehaviour
{
    public static ChoosePanel instance;
    public GameObject kong;
    public bool Isdianji = false;
    public GameObject choosepanel;

    // Use this for initialization
    void Start ()
    {
        instance = this;
        choosepanel.gameObject.SetActive(false);
        choosepanel.transform.Find("btnfh").GetComponent<Button>().onClick.AddListener(fhGame);
        choosepanel.transform.Find("btn1").GetComponent<Button>().onClick.AddListener(PlayGame);
    }

    private void fhGame()
    {
        BeginPanel.instance.gameObject.SetActive(true);
        choosepanel.gameObject.SetActive(false);
    }

    private void PlayGame()
    {
        BeginPanel.instance.gameObject.SetActive(false);
        choosepanel.gameObject.SetActive(false);

        kong.GetComponent<CreateAnimals>().InitiLayer();  //调用初始化盒子函数
        PassPanel.instance.gameObject.SetActive(true);
        Isdianji = true;
    }


    // Update is called once per frame
    void FixedUpdate ()
    {

    }
}
