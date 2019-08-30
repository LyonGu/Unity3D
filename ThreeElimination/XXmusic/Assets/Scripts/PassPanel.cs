using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PassPanel : MonoBehaviour
{
    public static PassPanel instance;
    public Text test2;
    public Text test4;
    public Text test5;
    public Text test22;
    public Text test44;
    public Text test55;
    public Image photo2;
    public Image photo4;
    public Image photo5;
    public int number2;
    public int number4;
    public int number5;
    public bool Isshow2 = false;
    public bool Isshow4 = false;
    public bool Isshow5 = false;
    public GameObject exchange1;

    // Use this for initialization
    void Start ()
    {
        instance = this;
        this.gameObject.SetActive(false);
        this.transform.Find("Beginbtn").GetComponent<Button>().onClick.AddListener(BeginGame);
        this.transform.Find("Fhbtn").GetComponent<Button>().onClick.AddListener(FhGame);

        Initialization();
        PassText();
    }

    public void PassText()               //实时获取数据
    {
        test2.text = number2.ToString();
        test4.text = number4.ToString();
        test5.text = number5.ToString();
    }

    public void BeginGame()
    {
        UpdateAnimals();
        exchange1.GetComponent<ChangeAnimals>().qiunumber = 15;    //重新把步数更新为15步
        exchange1.GetComponent<ChangeAnimals>().QiuText();
        test2.GetComponent<Text>().color = Color.white;
        test22.GetComponent<Text>().color = Color.white;
        test4.GetComponent<Text>().color = Color.white;
        test44.GetComponent<Text>().color = Color.white;
        test5.GetComponent<Text>().color = Color.white;
        test55.GetComponent<Text>().color = Color.white;

        Initialization();
        PassText();
    }

    public void Initialization()    //初始化数据
    {
        number2 = 0;
        number4 = 0;
        number5 = 0;
        photo2.gameObject.SetActive(false);
        photo4.gameObject.SetActive(false);
        photo5.gameObject.SetActive(false);
    }

    public void FhGame()
    {
        SceneManager.LoadScene("BeginScenes");
        ChoosePanel.instance.choosepanel.gameObject.SetActive(true);
    }

    public void UpdateAnimals()     //重新开始更新动物
    {
        GameObject[] m2 = GameObject.FindGameObjectsWithTag("2");     //找到面板上所有Tag为2的动物放进m2数组
        GameObject[] m3 = GameObject.FindGameObjectsWithTag("3");
        GameObject[] m4 = GameObject.FindGameObjectsWithTag("4");
        GameObject[] m5 = GameObject.FindGameObjectsWithTag("5");
        GameObject[] m6 = GameObject.FindGameObjectsWithTag("6");

        foreach (var n2 in m2)
        {
            Destroy(n2);             //遍历删除数组m2里面的动物
        }
        foreach (var n3 in m3)
        {
            Destroy(n3);
        }
        foreach (var n4 in m4)
        {
            Destroy(n4);
        }
        foreach (var n5 in m5)
        {
            Destroy(n5);
        }
        foreach (var n6 in m6)
        {
            Destroy(n6);
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}
}
