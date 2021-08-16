using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Profiling;
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
    //PerformanceCounter pf1;

    public Button btnAdd;
    public Button btnDel;


    public Transform testTransform;

    // Start is called before the first frame update
    void Start()
    {

        btnAdd.onClick.AddListener(OnClckAdd);
        btnDel.onClick.AddListener(OnClckDel);
        proc = Process.GetCurrentProcess();
        //pf1 = new PerformanceCounter("Process", "Working Set - Private", proc.ProcessName);   //第二个参数就是得到只有工作集


        //验证
        //int childCount = testTransform.childCount;

        //for (int i = 0; i < childCount; i++)
        //{
        //    var t = testTransform.GetChild(i);
        //    UnityEngine.Debug.Log($"name is {t.name}");
        //}


        //GetChild(testTransform);




    }
    private static void GetChild(Transform trans)
    {
        int childCount = trans.childCount;
        for (int i = 0; i < childCount; i++)
        {
            var t = trans.GetChild(i);
            //UnityEngine.Debug.Log($"name is {t.name}");
            if (t.childCount > 0)
            {
                GetChild(t);
            }
        }
        //foreach (Transform node in trans)
        //{
        //    //UnityEngine.Debug.Log($"name is {node.name}");
        //    if (node.childCount > 0)
        //    {
        //        GetChild(node.transform);
        //    }
        //}
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
            
           
            //float value = pf1.NextValue() / 1024;
            //txt.text = ((int)(value)).ToString();
        }
        timer += Time.deltaTime;

        //Profiler.BeginSample("TransForm array1");
        //Transform[] transformsAry = testTransform.GetComponentsInChildren<Transform>(true);
        //int length = transformsAry.Length;
        //for (int i = 0; i < length; i++)
        //{
        //    var t = transformsAry[i];
        //    //UnityEngine.Debug.Log($"name is {t.name}");
        //}
        //Profiler.EndSample();

        //Profiler.BeginSample("TransForm array2");
        //GetChild(testTransform);
        //Profiler.EndSample();

    }
}
