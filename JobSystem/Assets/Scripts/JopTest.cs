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

struct IncrementByDeltaTimeJob: IJobParallelFor
{
    public NativeArray<float> values;
    public float deltaTime;

    public void Execute (int index)
    {
        float temp = values[index];
        temp += deltaTime;
        values[index] = temp;
    }
}

// 将两个浮点值相加的作业
public struct MyParallelJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float> a;
    [ReadOnly] public NativeArray<float> b;
    public NativeArray<float> result;

    public void Execute(int i)
    {
        result[i] = a[i] + b[i];
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
        myJob.Schedule(); //调用 Schedule 会将该job放入job队列中，以便在适当的时间执行。一旦作业已调度，就不能中断作业
        
        // // 调度作业
        // JobHandle handle = myJob.Schedule();
        // handle.Complete(); //只要有Complete就是在主线程执行
        
    }


    //Job的依赖
    //Note: All of a job’s dependencies must be scheduled on the same control thread as the job itself.
    //注意：作业的所有依赖项必须安排在与作业本身相同的控制线程上
    private void JobDependen()
    {
        //secondJob依赖firstJob
        MyJob firstJob = new MyJob();
        MyJob secondJob = new MyJob();
        JobHandle firstJobHandle = firstJob.Schedule();
        secondJob.Schedule(firstJobHandle);
        
        
        
//         合并依赖项
//             如果一个作业具有许多依赖项，则可以使用 JobHandle.CombineDependencies 方法合并这些依赖项。CombineDependencies 可以将依赖项传递给 Schedule 方法。
//
//         NativeArray<JobHandle> handles = new NativeArray<JobHandle>(numJobs, Allocator.TempJob);
//
// // 使用来自多个调度作业的 `JobHandles` 填充 `handles`...
//
//         JobHandle jh = JobHandle.CombineDependencies(handles)


/*
 *
 * Jobs do not start executing when you schedule them. If you are waiting for the job in the control thread,
 * and you need access to the NativeContainer data that the job is using, you can call the method JobHandle.Complete.
 * This method flushes the jobs from the memory cache and starts the process of execution
 *
 * 当您安排作业时，作业不会开始执行。
 * 如果您正在控制线程中等待作业，并且需要访问作业正在使用的 NativeContainer 数据，则可以调用方法 JobHandle.Complete。
 * 该方法从内存缓存中刷新作业并启动执行过程
 * 
 */
        // JobHandle.Complete 有点强制执行job的意思
    }

    //ParallelFor 作业
    //在调度作业时，只能有一个作业正在执行一项任务。在游戏中，通常希望对大量对象执行相同的操作。有一个称为 IJobParallelFor 的单独作业类型可以处理此问题。
    //注意：“ParallelFor”作业是 Unity 中对于任何实现 IJobParallelFor 接口的结构的统称。
    /*
     *
     * ParallelFor 作业使用一个数据 NativeArray 作为其数据源。ParallelFor 作业在多个核心上运行。每个核心有一个作业，每个作业处理一部分工作量。
     * IJobParallelFor 的行为类似于 IJob，但其并非调用单个 Execute 方法，而是对数据源中的每一项都调用一次 Execute 方法。
     * Execute 方法中有一个整数参数。该索引用于访问和操作作业实现中的数据源的单个元素。
     * 
     */
    
    
    /*
     *调度 ParallelFor 作业
        在调度 ParallelFor 作业时，必须指定要拆分的 NativeArray 数据源的长度。
        如果结构中有多个 NativeArray，Unity C# 作业系统无法知道您希望将哪个用作数据源。长度还告诉 C# 作业系统应该有多少个 Execute 方法。

        在后台，ParallelFor 作业的调度更加复杂。
        在调度 ParallelFor 作业时，C# 作业系统将工作分成多个批次以便在多个核心之间分配任务。每个批次包含一小部分 Execute 方法。
        然后，针对每个 CPU 核心，C# 作业系统会在 Unity 本机作业系统中调度最多一个作业，并向该本机作业传递一些需要完成的批次。
     *
     *
     * 要优化该过程，必须指定批次计数。批次计数可以控制您获得的作业数量，以及线程之间重新分配工作的细化程度。
     * 批次计数较低（例如 1）可以使线程之间的工作分布更均匀。这样确实会带来一些开销，所以有时候增加批次计数会更好。
     * 一种有效的策略是从 1 开始并增加批次计数直到性能增益可忽略不计
     */
    private void ParallelFor()
    {
        NativeArray<float> a = new NativeArray<float>(2, Allocator.TempJob);

        NativeArray<float> b = new NativeArray<float>(2, Allocator.TempJob);

        NativeArray<float> result = new NativeArray<float>(2, Allocator.TempJob);

        a[0] = 1.1f;
        b[0] = 2.2f;
        a[1] = 3.3f;
        b[1] = 4.4f;

        MyParallelJob jobData = new MyParallelJob();
        jobData.a = a;  
        jobData.b = b;
        jobData.result = result;

// 调度作业，为结果数组中的每个索引执行一个 Execute 方法，且每个处理批次只处理一项
        JobHandle handle = jobData.Schedule(result.Length, 1);

// 等待作业完成
        handle.Complete();

// 释放数组分配的内存
        a.Dispose();
        b.Dispose();
        result.Dispose();
    }


    /*
     *
     *ParallelForTransform 作业是另一种 ParallelFor 作业；专为操作变换组件而设计。

注意：ParallelForTransform 作业是 Unity 中对于任何实现 IJobParallelForTransform 接口的作业的统称。
     * 
     */
    private void ParallelForTransform()
    {
        
    }

    private void Notice()
    {
        /*
         * 不要从作业访问静态数据
         *
         * 刷新已调度的批次
When you want your jobs to start executing, then you can flush the scheduled batch with JobHandle.ScheduleBatchedJobs.
 Note that calling this method can negatively impact performance(负面影响). Not flushing the batch delays the scheduling until the control thread waits for the result. In all other cases use JobHandle.Complete to start the execution process.
注意：在实体组件系统 (ECS) 中会隐式刷新批次，因此没必要调用 JobHandle.ScheduleBatchedJobs。
         * 
         */
        
        
        /*
         *
         *不要尝试更新 NativeContainer 内容
            由于缺少 ref 返回值，因此无法直接更改 NativeContainer 的内容。例如，nativeArray[0]++; 与 var temp = nativeArray[0]; temp++; 等效，不会更新 nativeArray 中的值。

            正确的做法是，必须将索引中的数据复制到本地临时副本，修改该副本，然后将其存回，如下所示：

            MyStruct temp = myNativeArray[i];
            temp.memberVariable = 0;
            myNativeArray[i] = temp;
         * 
         */
        
        /*
         *
         *不要在作业中分配托管内存
                在作业中分配托管内存非常慢，而且作业无法使用 Unity Burst 编译器来提高性能。
                Burst 是一种新的基于 LLVM 的后端编译器技术，可以简化您的工作。此编译器获取 C# 作业并生成高度优化的机器代码，从而利用目标平台的特定功能。
         * 
         */
        
        /*
         *调试作业 Run方法立刻执行
                Jobs have a Run function that you can use in place of Schedule to immediately execute the job on the control thread. 
                You can use this for debugging purposes.
         * 
         */
    }

    private void OnDestroy()
    {
        if (_result.IsCreated)
        {
            _result.Dispose();
        }
    }
}
