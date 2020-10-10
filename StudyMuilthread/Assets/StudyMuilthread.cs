using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading; // 创建线程需要用到的命名空间
using System;

public class StudyMuilthread : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
        //开启线程后，主线程和子线程同时被cpu分配时间片，然后执行对应逻辑
        Thread t = new Thread(PrintNumbers);
        t.Name = "hxpLearn";
        t.Start(); //start并不是立即调用函数

        PrintNumbers();
    }

    static void PrintNumbers()
    {
        // 使用Thread.CurrentThread.ManagedThreadId 可以获取当前运行线程的唯一标识，通过它来区别线程
        Debug.Log($"线程:{Thread.CurrentThread.ManagedThreadId} {Thread.CurrentThread.Name}开始打印...");
     
        for (int i = 0; i < 10; i++)
        {
            Debug.Log($"线程:{Thread.CurrentThread.ManagedThreadId} {Thread.CurrentThread.Name} 打印:{i}");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
