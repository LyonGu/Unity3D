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
    public RectTransform rectTrangle;

    private bool _isShowTips = false;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        targetD.SetActive(false);
        Tips.SetActive(false);
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
        if (_isShowTips != isShow)
        {
            _isShowTips = isShow;
            if (Tips != null)
                Tips.SetActive(isShow);
        }
       
    }

    public void SetRectTrangle(Vector2 screenPos, Vector2 size)
    {
        ShowRectTrangle(true);
        rectTrangle.position = screenPos; //overlay模式 屏幕坐标就是世界坐标
        rectTrangle.sizeDelta = size;
    }

    public void ShowRectTrangle(bool isShow)
    {
        rectTrangle.gameObject.SetActive(isShow);
    }
}
