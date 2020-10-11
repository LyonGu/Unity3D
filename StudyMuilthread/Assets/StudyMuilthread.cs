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

        //CreateThread();

        //使用Thread.Sleep 暂停线程
        //PauseThread();

        //线程等待使用的是Join方法，该方法将暂停执行当前线程，直到所等待的另一个线程终止
        //WaitThread();

        //线程终止使用Thread.Abort,
        /*
            终止线程使用的方法是Abort方法，当该方法被执行时，将尝试销毁该线程。通过引发ThreadAbortException异常使线程被销毁。
            但一般不推荐使用该方法，原因有以下几点

            1 使用Abort方法只是尝试销毁该线程，但不一定能终止线程。
            2 如果被终止的线程在执行lock内的代码，那么终止线程会造成线程不安全。
            3 线程终止时，CLR会保证自己内部的数据结构不会损坏，但是BCL不能保证。
            

        基于以上原因不推荐使用Abort方法，在实际项目中一般使用CancellationToken来终止线程。
         */
        //AbortThread();

        //PrintThreadStatus();

        //设置Thread的Priority属性，可设置为ThreadPriority枚举类型的五个值之一：Lowest、BelowNormal、Normal、AboveNormal 或 Highest。
        //CLR为自己保留了Idle和Time-Critical优先级，程序中不可设置。
        //ThreadPriorityTest();

        //前后台线程
        /*
            在CLR中，线程要么是前台线程，要么就是后台线程。
            当一个进程的所有前台线程停止运行时（stoped状态时，正常跑完逻辑就是），CLR将强制终止仍在运行的任何后台线程，不会抛出异常。

            在C#中可通过Thread类中的IsBackground属性来指定是否为后台线程。
            在线程生命周期中，任何时候都可从前台线程变为后台线程。
            
            线程池中的线程默认为后台线程。
         */
        //ThreadForegroundOrBackground();


        //线程传参
        /*
            向线程中传递参数常用的有三种方法，构造函数传值、Start方法传值和Lambda表达式传值，一般常用Start方法来传值
         */
        ThreadSendParams();
    }

    #region 测试案例
    void CreateThread()
    {
        //开启线程后，主线程和子线程同时被cpu分配时间片，然后执行对应逻辑
        Thread t = new Thread(PrintNumbers);
        t.Name = "hxpLearn";
        t.Start(); //start并不是立即调用函数

        PrintNumbers();
    }

    void PauseThread()
    {
        // 1.创建一个线程 PrintNumbers为该线程所需要执行的方法
        Thread t = new Thread(PrintNumbersWithDelay);
        // 2.启动线程
        t.Start();
    }

    void WaitThread()
    {
        Debug.Log($"-------开始执行 现在时间{DateTime.Now.ToString("HH:mm:ss.ffff")}-------");

        // 1.创建一个线程 PrintNumbersWithDelay为该线程所需要执行的方法
        Thread t = new Thread(PrintNumbersWithDelay);
        // 2.启动线程
        t.Start();
        // 3.等待线程结束
        t.Join(); //这句代码会阻塞主线程，让子线程执行完主线程才会继续执行

        Debug.Log($"-------执行完毕 现在时间{DateTime.Now.ToString("HH:mm:ss.ffff")}-------");
    }

    void AbortThread()
    {
        Debug.Log($"-------开始执行 现在时间{DateTime.Now.ToString("HH:mm:ss.ffff")}-------");

        // 1.创建一个线程 PrintNumbersWithDelay为该线程所需要执行的方法
        Thread t = new Thread(PrintNumbersWithDelay);
        // 2.启动线程
        t.Start();
        // 3.主线程休眠6秒
        Thread.Sleep(TimeSpan.FromSeconds(6));
        // 4.终止线程
        t.Abort();

        Debug.Log($"-------执行完毕 现在时间{DateTime.Now.ToString("HH:mm:ss.ffff")}-------");
    }


    void PrintThreadStatus()
    {
        Debug.Log("开始执行...");

        Thread t = new Thread(PrintNumbersWithStatus);
        Thread t2 = new Thread(DoNothing);

        // 使用ThreadState查看线程状态 此时线程未启动，应为Unstarted
        Debug.Log($"Check 1 : t status = {t.ThreadState}, t2 status = {t2.ThreadState}");

        t2.Start();
        t.Start();

        // 线程启动， 状态应为 Running
        Debug.Log($"Check 2 :t status = {t.ThreadState}, t2 status = {t2.ThreadState}");

        // 由于PrintNumberWithStatus开始执行，状态为Running
        // 但是经接着Thread.Sleep 状态会转为 WaitSleepJoin
        for (int i = 1; i < 30; i++)
        {
            Debug.Log($"Check 3 :t status =  {t.ThreadState}, t2 status =  {t2.ThreadState}");
        }

        // 延时一段时间，方便查看状态
        Thread.Sleep(TimeSpan.FromSeconds(6));

        // 终止线程
        t.Abort();

        Debug.Log("t线程被终止");

        // 由于该线程是被Abort方法终止 所以状态为 Aborted或AbortRequested
        Debug.Log($"Check 4 : t status = {t.ThreadState}");
        // 该线程正常执行结束 所以状态为Stopped
        Debug.Log($"Check 5 : {t2.ThreadState}");

    }

    void ThreadPriorityTest()
    {
        Debug.Log($"当前线程优先级: {Thread.CurrentThread.Priority} 支持最大核数 {SystemInfo.processorCount}");
        // 第一次测试，在所有核心上运行
        Debug.Log("运行在所有空闲的核心上");
        RunTestThreadsPriority();
        Thread.Sleep(TimeSpan.FromSeconds(2));

        // 第二次测试，在单个核心上运行
        Debug.Log("运行在单个核心上");
        // 设置在单个核心上运行

        System.Diagnostics.Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(1);
        RunTestThreadsPriority();

    }

    void ThreadForegroundOrBackground()
    {
        var sampleForeground = new ThreadSample_1(10);
        var sampleBackground = new ThreadSample_1(20);
        var threadPoolBackground = new ThreadSample_1(20);

        // 默认创建为前台线程
        var threadOne = new Thread(sampleForeground.CountNumbers);
        threadOne.Name = "前台线程";

        var threadTwo = new Thread(sampleBackground.CountNumbers);
        threadTwo.Name = "后台线程";
        // 设置IsBackground属性为 true 表示后台线程
        threadTwo.IsBackground = true;

        // 线程池内的线程默认为 后台线程
        ThreadPool.QueueUserWorkItem((obj) =>
        {
            Thread.CurrentThread.Name = "线程池线程";
            threadPoolBackground.CountNumbers();
        });

        // 启动线程 
        threadOne.Start();
        threadTwo.Start();


        //结果:当前台线程10次循环结束以后，创建的后台线程和线程池线程都会被CLR强制结束, 这里没结束时因为主线程一直在跑，主线程是前台线程
    }

    void ThreadSendParams()
    {
        // 第一种方法 通过构造函数传值
        var sample = new ThreadSample_2(10);

        var threadOne = new Thread(sample.CountNumbers);
        threadOne.Name = "ThreadOne";
        threadOne.Start();
        threadOne.Join();

        Debug.Log("--------------------------");

        // 第二种方法 使用Start方法传值 
        // Count方法 接收一个Object类型参数
        var threadTwo = new Thread(Count);
        threadTwo.Name = "ThreadTwo";
        // Start方法中传入的值 会传递到 Count方法 Object参数上
        threadTwo.Start(8);
        threadTwo.Join();

        Debug.Log("--------------------------");

        // 第三种方法 Lambda表达式传值
        // 实际上是构建了一个匿名函数 通过函数闭包来传值
        var threadThree = new Thread(() => CountNumbers(12));
        threadThree.Name = "ThreadThree";
        threadThree.Start();
        threadThree.Join();
        Debug.Log("--------------------------");

        // Lambda表达式传值 会共享变量值
        int i = 10;
        var threadFour = new Thread(() => PrintNumber(i));
        i = 20;
        var threadFive = new Thread(() => PrintNumber(i));
        threadFour.Start();
        threadFive.Start();

    }

    #endregion










    void DoNothing()
    {
        Thread.Sleep(TimeSpan.FromSeconds(2));  //在某个线程里调用Thread.Sleep就是让该线程暂停
    }

    void PrintNumbersWithStatus()
    {
        Debug.Log("t线程开始执行...");

        // 在线程内部，可通过Thread.CurrentThread拿到当前线程Thread对象
        Debug.Log($"Check 6 : t status = {Thread.CurrentThread.ThreadState}");
        for (int i = 1; i < 10; i++)
        {
            Thread.Sleep(TimeSpan.FromSeconds(2));
            Debug.Log($"t线程输出 ：{i}");
        }
    }

    void PrintNumbersWithDelay()
    {
        Debug.Log($"线程：{Thread.CurrentThread.ManagedThreadId} 开始打印... 现在时间 {DateTime.Now.ToString("HH:mm:ss.ffff")}");
        for (int i = 0; i < 10; i++)
        {
            //3. 使用Thread.Sleep方法来使当前线程睡眠，TimeSpan.FromSeconds(2)表示时间为 2秒
            Thread.Sleep(TimeSpan.FromSeconds(2));
            Debug.Log($"线程：{Thread.CurrentThread.ManagedThreadId} 打印:{i} 现在时间{DateTime.Now.ToString("HH:mm:ss.ffff")}");
        }
    }

    void PrintNumbers()
    {
        // 使用Thread.CurrentThread.ManagedThreadId 可以获取当前运行线程的唯一标识，通过它来区别线程
        Debug.Log($"线程:{Thread.CurrentThread.ManagedThreadId} {Thread.CurrentThread.Name}开始打印...");
     
        for (int i = 0; i < 10; i++)
        {
            Debug.Log($"线程:{Thread.CurrentThread.ManagedThreadId} {Thread.CurrentThread.Name} 打印:{i}");
        }
    }

    class ThreadSample
    {
        private bool _isStopped = false;

        public void Stop()
        {
            _isStopped = true;
        }

        public void CountNumbers()
        {
            long counter = 0;

            while (!_isStopped)
            {
                counter++;
            }

            Debug.Log($"{Thread.CurrentThread.Name} 优先级为 {Thread.CurrentThread.Priority,11} 计数为 = {counter,13:N0}");
        }
    }

    void RunTestThreadsPriority()
    {
        var sample = new ThreadSample();
        var threadOne = new Thread(sample.CountNumbers);
        threadOne.Name = "线程一";

        var threadTwo = new Thread(sample.CountNumbers);
        threadTwo.Name = "线程二";

        // 设置优先级和启动线程
        threadOne.Priority = System.Threading.ThreadPriority.Highest;
        threadTwo.Priority = System.Threading.ThreadPriority.Lowest;

        threadOne.Start();
        threadTwo.Start();

        // 延时2秒 查看结果
        Thread.Sleep(TimeSpan.FromSeconds(2));
        sample.Stop();

    }


    class ThreadSample_1
    {
        private readonly int _iterations;

        public ThreadSample_1(int iterations)
        {
            _iterations = iterations;
        }
        public void CountNumbers()
        {
            for (int i = 0; i < _iterations; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
                Debug.Log($"{Thread.CurrentThread.Name} prints {i}");
            }
        }
    }

    class ThreadSample_2
    {
        private readonly int _iterations;

        public ThreadSample_2(int iterations)
        {
            _iterations = iterations;
        }
        public void CountNumbers()
        {
            for (int i = 1; i <= _iterations; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
                Debug.Log($"{Thread.CurrentThread.Name} prints {i}");
            }
        }
    }


    void Count(object iterations)
    {
        CountNumbers((int)iterations);
    }

    void CountNumbers(int iterations)
    {
        for (int i = 1; i <= iterations; i++)
        {
            Thread.Sleep(TimeSpan.FromSeconds(0.5));
            Debug.Log($"{Thread.CurrentThread.Name} prints {i}");
        }
    }

    void PrintNumber(int number)
    {
        Debug.Log(number);
    }
}
