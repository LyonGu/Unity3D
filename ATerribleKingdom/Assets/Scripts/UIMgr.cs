using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMgr : MonoBehaviour
{

    private static UIMgr instance;

    public static UIMgr Instance { get => instance; private set => instance = value; }

    public GameObject targetD;
    public Text CharName;
    public Text Content;
    public GameObject Tips;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        targetD.SetActive(false);
    }

    public void SetDialog(string charaterName, string content)
    {
        targetD.SetActive(true);
        CharName.text = charaterName;
        Content.text = content;
    }

    public void HideDialog()
    {
        targetD.SetActive(false);
    }

    public void ShowTips(bool isShow)
    {
        if(Tips!=null)
            Tips.SetActive(isShow);
    }
}
