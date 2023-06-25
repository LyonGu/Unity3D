using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;


// 将两个浮点值相加的作业
public struct MyJob : IJob
{
    public float a;
    public float b;
    public NativeArray<float> result;

    public void Execute()
    {
        result[0] = a + b;
        Debug.Log($"【Job】 Thread Id is {Thread.CurrentThread.ManagedThreadId}  result {result[0]}");
        // result.Dispose(); //不能在Job里释放NativeContainer
    }

}

public class JopTest : MonoBehaviour
{
    private NativeArray<float> _result;
    void Start()
    {
       
        Debug.Log($"Thread Id is {Thread.CurrentThread.ManagedThreadId}");
        MyJob myJob = new MyJob();
        myJob.a = 1;
        myJob.b = 2;
        NativeArray<float> result = new NativeArray<float>(1, Allocator.Persistent);
        _result = result;
        myJob.result = result;
        
        // NativeArray<float> result = new NativeArray<float>(1, Allocator.TempJob);
        // myJob.result = result;
        
        // myJob.Schedule().Complete(); //执行并且等待job完成，在主线程执行
        myJob.Schedule();
        // result.Dispose();
    }

    private void OnDestroy()
    {
        if (_result.IsCreated)
        {
            _result.Dispose();
        }
    }
}
