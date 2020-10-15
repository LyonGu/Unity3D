using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using static System.Threading.Thread;
public class StudyThreadPool : MonoBehaviour
{
    // 创建带一个参数的委托类型
    private delegate string RunOnThreadPool(out int threadId);


    void Start()
    {

        //使用线程池支持APM，现在不常用了
        //RunTreadPoolByAPM();

        //向线程池里的函数传递参数
        //RunThreadPoolParams();

        //对比使用普通线程和使用线程池有什么不同
        //RunCompareThreadPoolAndNormalThread();

        //使用CancellationToken和CancellationTokenSource来实现任务的取消
        //RunCancleThread();


        //线程池超时处理 ThreadPool.RegisterWaitForSingleObject
        RunThreadPoolTimeout();
    }

    #region 测试接口

    void RunTreadPoolByAPM()
    {
        int threadId = 0;
        RunOnThreadPool poolDelegate = Test;

        var t = new Thread(() => Test(out threadId));
        t.Start();
        t.Join();
        Debug.Log($"手动创建线程 Id: {threadId}");

        Debug.Log("poolDelegate============BeginInvoke");
        // 使用APM方式 进行异步调用  异步调用会使用线程池中的线程
        // 优先执行委托函数Test，在任务处理完成后调用Callback， 这里的任务处理完是调用了EndInvoke  

        ////BeginInvoke(委托函数需要参数，委托函数执行完的回调函数，用户自定义的状态给回调函数) Callback也在是线程池里线程里工作
        IAsyncResult r = poolDelegate.BeginInvoke(out threadId, Callback, "委托异步调用");
        r.AsyncWaitHandle.WaitOne();
        Debug.Log("poolDelegate============EndInvoke");
        // 获取异步调用结果 ==》 任务处理完
        //EndInvoke调用完后会执行了Callback方法，但是跟主线程的之后的逻辑没有优先顺序，跟cpu分配时间有关
        string result = poolDelegate.EndInvoke(out threadId, r);

        Debug.Log($"------------------1:");
        Debug.Log($"Thread - 线程池工作线程Id: {threadId}");
        Debug.Log($"poolDelegate result======{result}");
        Debug.Log($"------------------2:");


    }


    void RunThreadPoolParams()
    {

        const int x = 1;
        const int y = 2;
        const string lambdaState = "lambda state 2";

        // 直接将方法传递给线程池，会直接执行，不用调用start或者run之类的方法
        ThreadPool.QueueUserWorkItem(AsyncOperation);
        Sleep(TimeSpan.FromSeconds(1));

        // 直接将方法传递给线程池 并且 通过state传递参数
        ThreadPool.QueueUserWorkItem(AsyncOperation, "async state");
        Sleep(TimeSpan.FromSeconds(1));

        // 使用Lambda表达式将任务传递给线程池 并且通过 state传递参数
        ThreadPool.QueueUserWorkItem(state =>
        {
            Debug.Log($"Operation state: {state}");
            Debug.Log($"工作线程 id: {CurrentThread.ManagedThreadId} 是否为线程池线程: {CurrentThread.IsThreadPoolThread}");
            Sleep(TimeSpan.FromSeconds(2));
        }, "lambda state");

        // 使用Lambda表达式将任务传递给线程池 通过 **闭包** 机制传递参数
        ThreadPool.QueueUserWorkItem(_ =>
        {
            Debug.Log($"Operation state: {x + y}, {lambdaState}");
            Debug.Log($"工作线程 id: {CurrentThread.ManagedThreadId} 是否为线程池线程: {CurrentThread.IsThreadPoolThread}");
            Sleep(TimeSpan.FromSeconds(2));
        }, "lambda state");
    }


    void RunCompareThreadPoolAndNormalThread()
    {
        /*
            可见使用原始的创建线程执行，速度非常快。只花了2秒钟，但是创建了500多个线程，而使用线程池相对来说比较慢，花了9秒钟，但是只创建了很少的线程，
            为操作系统节省了线程和内存空间，但花了更多的时间
            */

        const int numberOfOperations = 500;
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        UseThreads(numberOfOperations);
        sw.Stop();
        Debug.Log($"使用线程执行总用时: {sw.ElapsedMilliseconds}");

        sw.Reset();
        sw.Start();
        UseThreadPool(numberOfOperations);
        sw.Stop();
        Debug.Log($"使用线程池执行总用时: {sw.ElapsedMilliseconds}");
    }

    void RunCancleThread()
    {
        // 使用CancellationToken来取消任务  取消任务直接返回
        using (var cts = new CancellationTokenSource())
        {
            CancellationToken token = cts.Token;
            ThreadPool.QueueUserWorkItem(_ => AsyncOperation1(token)); //执行一个多线程操作
            Sleep(TimeSpan.FromSeconds(2));
            cts.Cancel();
        }

        // 取消任务 抛出 ThrowIfCancellationRequesed 异常
        using (var cts = new CancellationTokenSource())
        {
            CancellationToken token = cts.Token;
            ThreadPool.QueueUserWorkItem(_ => AsyncOperation2(token));
            Sleep(TimeSpan.FromSeconds(2));
            cts.Cancel();
        }

        // 取消任务 并 执行取消后的回调函数
        using (var cts = new CancellationTokenSource())
        {
            CancellationToken token = cts.Token;
            //CancellationToken的Register方法可以注册一个取消后的回调函数
            token.Register(() => { Debug.Log("第三个任务被取消，执行回调函数。"); });
            ThreadPool.QueueUserWorkItem(_ => AsyncOperation3(token));
            Sleep(TimeSpan.FromSeconds(2));
            cts.Cancel();
        }

    }

    void RunThreadPoolTimeout()
    {
        // 设置超时时间为 5s WorkerOperation会延时 6s 肯定会超时
        RunOperationsTimeOut(TimeSpan.FromSeconds(5), 1);

        // 设置超时时间为 7s 不会超时
        RunOperationsTimeOut(TimeSpan.FromSeconds(7), 2);
    }


    #endregion


    void RunOperationsTimeOut(TimeSpan workerOperationTimeout, int index)
    {
        using (var evt = new ManualResetEvent(false)) //跳出using语句会自动执行Dispose方法
        using (var cts = new CancellationTokenSource())
        {
            Debug.Log($"{index}注册超时操作...");
            // 传入同步事件  超时处理函数  和 超时时间
            var worker = ThreadPool.RegisterWaitForSingleObject(evt
                , (state, isTimedOut) => WorkerOperationWait(cts, isTimedOut, state)
                , null
                , workerOperationTimeout
                , true);

            Debug.Log("启动长时间运行操作...");
            ThreadPool.QueueUserWorkItem(_ => WorkerOperation(cts.Token, evt));

            Sleep(workerOperationTimeout.Add(TimeSpan.FromSeconds(2)));

            Debug.Log($"{index}取消注册超时操作...");
            // 取消注册等待的操作
            worker.Unregister(evt);

        }
    }

    void WorkerOperation(CancellationToken token, ManualResetEvent evt)
    {
        for (int i = 0; i < 6; i++)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }
            Sleep(TimeSpan.FromSeconds(1));
        }
        evt.Set();
    }

    void WorkerOperationWait(CancellationTokenSource cts, bool isTimedOut, object state)
    {
        Debug.Log($"WorkerOperationWait state: {state}");
        if (isTimedOut)
        {
            cts.Cancel();
            Debug.Log("工作操作超时并被取消.");
        }
        else
        {
            Debug.Log("工作操作成功.");
        }
    }
    void AsyncOperation1(CancellationToken token)
    {
        Debug.Log("启动第一个任务.");
        for (int i = 0; i < 5; i++)
        {
            if (token.IsCancellationRequested) 
            {
                Debug.Log("第一个任务被取消.");
                return;
            }
            Sleep(TimeSpan.FromSeconds(1));
        }
        Debug.Log("第一个任务运行完成.");
    }

    void AsyncOperation2(CancellationToken token)
    {
        try
        {
            Debug.Log("启动第二个任务.");

            for (int i = 0; i < 5; i++)
            {
                token.ThrowIfCancellationRequested();
                Sleep(TimeSpan.FromSeconds(1));
            }
            Debug.Log("第二个任务运行完成.");
        }
        catch (OperationCanceledException)
        {
            Debug.Log("第二个任务被取消.");
        }
    }


    void AsyncOperation3(CancellationToken token)
    {
        Debug.Log("启动第三个任务.");
        for (int i = 0; i < 5; i++)
        {
            if (token.IsCancellationRequested)
            {
                Debug.Log("第三个任务被取消.");
                return;
            }
            Sleep(TimeSpan.FromSeconds(1));
        }
        Debug.Log("第三个任务运行完成.");
    }

    void UseThreadPool(int numberOfOperations)
    {
        using (var countdown = new CountdownEvent(numberOfOperations)) //使用CountdownEvent方式进行同步
        {
            Debug.Log("使用线程池开始工作");
            for (int i = 0; i < numberOfOperations; i++)
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    Debug.Log($"{CurrentThread.ManagedThreadId},");
                    Sleep(TimeSpan.FromSeconds(0.1));
                    countdown.Signal();
                });
            }
            countdown.Wait();
        }
    }

    void UseThreads(int numberOfOperations)
    {
        //CountdownEvent 方式加锁
        using (var countdown = new CountdownEvent(numberOfOperations))
        {
            Debug.Log("通过创建线程调度工作");
            for (int i = 0; i < numberOfOperations; i++)
            {
                var thread = new Thread(() =>
                {
                    Debug.Log($"{CurrentThread.ManagedThreadId},");
                    Sleep(TimeSpan.FromSeconds(0.1));
                    countdown.Signal();
                });
                thread.Start();
            }
            countdown.Wait();
        }
    }

    private static void Callback(IAsyncResult ar)
    {
        Debug.Log("Callback - 开始运行Callback...");
        Debug.Log($"Callback - 回调传递状态: {ar.AsyncState}");
        Debug.Log($"Callback - 是否为线程池线程: {CurrentThread.IsThreadPoolThread}");
        Debug.Log($"Callback - 线程池工作线程Id: {CurrentThread.ManagedThreadId}");
    }

 
    private string Test(out int threadId)
    {
        string isThreadPoolThread = CurrentThread.IsThreadPoolThread ? "ThreadPool - " : "Thread - ";

        Debug.Log($"{isThreadPoolThread}开始运行...");
        Debug.Log($"{isThreadPoolThread}是否为线程池线程: {CurrentThread.IsThreadPoolThread}");
        Sleep(TimeSpan.FromSeconds(2));
        threadId = CurrentThread.ManagedThreadId;
        return $"{isThreadPoolThread}线程池工作线程Id: {threadId}";
    }

    private void AsyncOperation(object state)
    {
        Debug.Log($"Operation state: {state ?? "(null)"}");
        Debug.Log($"工作线程 id: {CurrentThread.ManagedThreadId} 是否为线程池线程: {CurrentThread.IsThreadPoolThread}");
        Sleep(TimeSpan.FromSeconds(2));
    }
}
