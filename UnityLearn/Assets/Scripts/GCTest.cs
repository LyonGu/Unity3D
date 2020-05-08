using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;


public class MyGo : MonoBehaviour
{
     byte[] data = new byte[83000];
}
public class GCTest : MonoBehaviour
{

    public Transform objs;
    public Text txt;
    private Process proc;
    PerformanceCounter pf1;

    public Button btnAdd;
    public Button btnDel;

    // Start is called before the first frame update
    void Start()
    {

        btnAdd.onClick.AddListener(OnClckAdd);
        btnDel.onClick.AddListener(OnClckDel);
        proc = Process.GetCurrentProcess();
        pf1 = new PerformanceCounter("Process", "Working Set - Private", proc.ProcessName);   //第二个参数就是得到只有工作集
    }

    void OnClckAdd()
    {
        for (int i = 0; i< 20; ++i)        
        {
            var go = new GameObject();
            go.AddComponent<MyGo>();
            go.transform.SetParent(objs);
        }
    }

    void OnClckDel()
    {
        for (int i = objs.childCount - 1; i >= 0; i--)
        {
            GameObject.DestroyImmediate(objs.GetChild(i).gameObject);
        }
        System.GC.Collect();

    }


    // Update is called once per frame
    float timer = 0;
    void Update()
    {
        if (timer > 0.5f)
        {
            timer = 0;

            //var workingSet = Environment.WorkingSet / (1024 * 1024);
            //Console.WriteLine("{0}:{1}  {2:N}KB", ps.ProcessName, "私有工作集    ", pf1.NextValue() / 1024);
            //UnityEngine.Debug.Log("xxx=="+ pf1.NextValue());
            
           
            float value = pf1.NextValue() / 1024;
            txt.text = ((int)(value)).ToString();
        }
        timer += Time.deltaTime;

    }
}
