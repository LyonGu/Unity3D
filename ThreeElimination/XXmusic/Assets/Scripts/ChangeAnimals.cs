using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeAnimals : MonoBehaviour
{
    public Transform[] exchange = new Transform[2];    //交换动物的数组
    public int i = 0;
    public Vector3 n;
    public Transform m;
    public Text qiuqiutext;
    public int qiunumber;       //可用步数

    // Use this for initialization
    void Start()
    {
        qiunumber = 15;
        QiuText();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void QiuText()          //实时更新面板上的步数
    {
        qiuqiutext.text = qiunumber.ToString();
    }

    public void exchangei()
    {
        i++;
        if (i == 2)
        {
            i = 0;
        }
    }

    public void ExchangeAnimals()      //交换动物
    {
        m = exchange[0].parent;
        exchange[0].parent = exchange[1].parent;      //交换父对象
        exchange[1].parent = m;

        exchange[0].position = exchange[0].parent.position;
        exchange[1].position = exchange[1].parent.position;   //让动物跟着父对象移动

        var allAnimals = FindObjectsOfType<XXanimals>();
        foreach (var item in allAnimals)
        {
            item.horizontal = new List<RaycastHit2D>();      //更新水平数组的动物
            item.vertical = new List<RaycastHit2D>();       //更新垂直数组的动物
        }

        qiunumber = qiunumber - 1;           //每交换一次动物让步数减一

        if (qiunumber <= 0)
        {
            qiunumber = 0;
            FailPanel.instance.gameObject.SetActive(true);
        }
        QiuText();
    }

    public void Clear()
    {
        exchange[0].GetChild(0).GetComponent<SpriteRenderer>().sortingLayerName = "Default";
        exchange[1].GetChild(0).GetComponent<SpriteRenderer>().sortingLayerName = "Default";
        exchange = new Transform[2];      //交换完成清空数组
    }
}
