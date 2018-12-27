using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerPref_demo : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
        //存储人员信息
        if(Input.GetKeyDown(KeyCode.S))
        {
            PlayerPrefs.SetString("name", "hexinping");
            PlayerPrefs.SetInt("age",25);
            PlayerPrefs.SetFloat("salary",10000.0f);
            print("信息存储完毕，可以查询");
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            string strName = PlayerPrefs.GetString("name");
            int age = PlayerPrefs.GetInt("age");
            float salary = PlayerPrefs.GetFloat("salary");

            if (!string.IsNullOrEmpty(strName))
            {
                print("姓名:" + strName);
                if(age > 0)
                {
                    print("年龄:" + age);
                }

                if (salary > 0)
                {
                    print("薪水:" + salary);
                }
            }
        }

        if(Input.GetKeyDown(KeyCode.F))
        {
            if (PlayerPrefs.HasKey("name"))
            {
                print("name+++++" + PlayerPrefs.GetString("name"));
            }
            else
            {
                print("查无此人==========");
            }
            
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            if (PlayerPrefs.HasKey("name"))
            {
                PlayerPrefs.DeleteKey("name");
                PlayerPrefs.DeleteKey("age");
                PlayerPrefs.DeleteKey("salary");
            }

        }
	}
}
