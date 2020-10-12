using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class SynsStudyMuilthread : MonoBehaviour
{

    private SemaphoreSlim _semaphore = new SemaphoreSlim(4);

    // 工作线程Event
    private  AutoResetEvent _workerEvent = new AutoResetEvent(false);
    // 主线程Event
    private  AutoResetEvent _mainEvent = new AutoResetEvent(false);

    private ManualResetEventSlim _mainEvent_ManualResetEventSlim = new ManualResetEventSlim(false);

    // 构造函数的参数为2 表示只有调用了两次 Signal方法 CurrentCount 为 0时  Wait的阻塞才解除
    private CountdownEvent _countdown = new CountdownEvent(2);

    void Start()
    {


        //原子操作加锁，以及lock方式锁
        //RunInterlocked();

        //使用SemaphoreSlim类
        //RunSemaphoreSlim();


        //AutoResetEvent 方式加锁
        //RunAutoResetEvent();

        //ManualResetEventSlim 方式加锁
        //RunManualResetEventSlim();

        //使用CountDownEvent类 加锁
        RunCountDownEvent();

    }


    #region  测试接口

    //使用原子操作同步锁
    void RunInterlocked()
    {
        //Interlocked 执行基本原子操作，基本的读写
        /*
            CLR保证了对这些数据类型的读写是原子性的：Boolean、Char、(S)Byte、(U)Int16、(U)Int32、(U)IntPtr和Single。但是如果读写Int64可能会发生读取撕裂(torn read)的问题，
            因为在32位操作系统中，它需要执行两次Mov操作，无法在一个时间内执行完成。
            那么在本节中，就会着重的介绍System.Threading.Interlocked类提供的方法，Interlocked类中的每个方法都是执行一次的读取以及写入操作
         */

        Debug.Log("错误的计数");

        var c = new Counter();
        Execute(c);

        Debug.Log("--------------------------");


        Debug.Log("正确的计数 - 有锁");

        var c2 = new CounterWithLock();
        Execute(c2);

        Debug.Log("--------------------------");


        Debug.Log("正确的计数 - 无锁");

        var c3 = new CounterNoLock();
        Execute(c3);
    }


    void RunSemaphoreSlim()
    {
        /*
           SemaphoreSlim类与之前提到的同步类有锁不同，之前提到的同步类都是互斥的，也就是说只允许一个线程进行访问资源，
           而SemaphoreSlim是可以允许多个访问

           在之前的部分有提到，以*Slim结尾的线程同步类，都是工作在混合模式下的，也就是说开始它们都是在用户模式下"自旋"，等发生第一次竞争时，才切换到内核模式。
           但是SemaphoreSlim不同于Semaphore类，它不支持系统信号量，所以它不能用于进程之间的同步。

           演示代码演示了6个线程竞争访问只允许4个线程同时访问的数据库

        */

        //结果：前4个线程马上就获取到了锁，进入了临界区，而另外两个线程在等待；等有锁被释放时，才能进入临界区



        // 创建6个线程 竞争访问AccessDatabase
        for (int i = 1; i <= 6; i++)
        {
            string threadName = "线程 " + i;
            // 越后面的线程，访问时间越久 方便查看效果
            int secondsToWait = 2 + 2 * i;

            //通过闭包感觉可以传多个参数
            var t = new Thread(() => AccessDatabase(threadName, secondsToWait));
            t.Start();
        }

    }

    void RunAutoResetEvent()
    {
        /*
            AutoResetEvent叫自动重置事件，虽然名称中有事件一词，但是重置事件和C#中的委托没有任何关系，
            这里的事件只是由内核维护的Boolean变量，当事件为false，那么在事件上等待的线程就阻塞；事件变为true，那么阻塞解除。

            在.Net中有两种此类事件，即AutoResetEvent(自动重置事件)和ManualResetEvent(手动重置事件)。
            这两者均是采用内核模式，它的区别在于当重置事件为true时，自动重置事件它只唤醒一个阻塞的线程，会自动将事件重置回false，造成其它线程继续阻塞。
            而手动重置事件不会自动重置，必须通过代码手动重置回false。

            因为以上的原因，所以在很多文章和书籍中不推荐使用AutoResetEvent(自动重置事件)，因为它很容易在编写生产者线程时发生失误，造成它的迭代次数多余消费者线程。
         */

        var t = new Thread(() => Process(10));
        t.Start();
        Debug.Log("等待另一个线程完成工作！");
        // 等待工作线程通知 主线程阻塞
        _workerEvent.WaitOne();

        Debug.Log("第一个操作已经完成！");
        Debug.Log("在主线程上执行操作");
        Thread.Sleep(TimeSpan.FromSeconds(5));

        // 发送通知 工作线程继续运行
        _mainEvent.Set();
        Console.WriteLine("主线程 发送通知了但是我还是先执行");
        Debug.Log("现在在第二个线程上运行第二个操作");

        // 等待工作线程通知 主线程阻塞
        _workerEvent.WaitOne();
        Debug.Log("第二次操作完成！");

    }

    void RunManualResetEventSlim()
    {

        /*
            ManualResetEventSlim使用和ManualResetEvent类基本一致，
            只是ManualResetEventSlim工作在混合模式下，而它与AutoResetEventSlim不同的地方就是需要手动重置事件，也就是调用Reset()才能将事件重置为false。
            
         */
        var t1 = new Thread(() => TravelThroughGates("Thread 1", 5));
        var t2 = new Thread(() => TravelThroughGates("Thread 2", 6));
        var t3 = new Thread(() => TravelThroughGates("Thread 3", 12));
        t1.Start();
        t2.Start();
        t3.Start();

        // 休眠6秒钟  只有Thread 1小于 6秒钟，所以事件重置时 Thread 1 肯定能进入大门  而 Thread 2 可能可以进入大门
        Thread.Sleep(TimeSpan.FromSeconds(6));
        Debug.Log($"大门现在打开了!  时间：{DateTime.Now.ToString("mm:ss.ffff")}");
        _mainEvent_ManualResetEventSlim.Set();

        // 休眠2秒钟 此时 Thread 2 肯定可以进入大门
        Thread.Sleep(TimeSpan.FromSeconds(2));
        _mainEvent_ManualResetEventSlim.Reset(); //将事件重置为false 线程阻塞
        Debug.Log($"大门现在关闭了! 时间：{DateTime.Now.ToString("mm: ss.ffff")}");

        // 休眠10秒钟 Thread 3 可以进入大门
        Thread.Sleep(TimeSpan.FromSeconds(10));
        Debug.Log($"大门现在第二次打开! 时间：{DateTime.Now.ToString("mm: ss.ffff")}");
        _mainEvent_ManualResetEventSlim.Set();
        Thread.Sleep(TimeSpan.FromSeconds(2));

        Debug.Log($"大门现在关闭了! 时间：{DateTime.Now.ToString("mm: ss.ffff")}");
        _mainEvent_ManualResetEventSlim.Reset();
    }

    void RunCountDownEvent()
    {

        //运行结果如下图所示，可见只有当操作1和操作2都完成以后，才执行输出所有操作都完成。

        /*
         CountDownEvent类内部构造使用了一个ManualResetEventSlim对象。这个构造阻塞一个线程，直到它内部计数器(CurrentCount)变为0时，才解除阻塞。也就是说它并不是阻止对已经枯竭的资源池的访问，而是只有当计数为0时才允许访问。

            这里需要注意的是，当CurrentCount变为0时，那么它就不能被更改了。为0以后，Wait()方法的阻塞被解除。

            演示代码如下所示，只有当Signal()方法被调用2次以后，Wait()方法的阻塞才被解除。
         */
        Debug.Log($"开始两个操作  {DateTime.Now.ToString("mm:ss.ffff")}");

        var t1 = new Thread(() => PerformOperation("操作 1 完成！", 4));
        var t2 = new Thread(() => PerformOperation("操作 2 完成！", 8));
        t1.Start();
        t2.Start();

        // 等待操作完成
        _countdown.Wait();
        Debug.Log($"所有操作都完成  {DateTime.Now.ToString("mm: ss.ffff")}");
        _countdown.Dispose();

    }
    #endregion

    void PerformOperation(string message, int seconds)
    {
        Thread.Sleep(TimeSpan.FromSeconds(2));
        Debug.Log($"{message}  {DateTime.Now.ToString("mm:ss.ffff")}");

        // CurrentCount 递减 1
        _countdown.Signal();

    }
    void TravelThroughGates(string threadName, int seconds)
    {
        Debug.Log($"{threadName} 进入睡眠 时间：{DateTime.Now.ToString("mm:ss.ffff")}");
        Thread.Sleep(TimeSpan.FromSeconds(seconds));

        Debug.Log($"{threadName} 等待大门打开! 时间：{DateTime.Now.ToString("mm:ss.ffff")}");
        _mainEvent_ManualResetEventSlim.Wait();

        Debug.Log($"{threadName} 进入大门! 时间：{DateTime.Now.ToString("mm:ss.ffff")}");
    }
    void Process(int seconds)
    {
        Debug.Log("开始长时间的工作...");
        Thread.Sleep(TimeSpan.FromSeconds(seconds));
        Debug.Log("工作完成!");

        // 发送通知 主线程继续运行
        _workerEvent.Set();
        Debug.Log("子线程 发送通知了但是我还是先执行");
        Debug.Log("等待主线程完成其它工作");


        // 等待主线程通知 工作线程阻塞
        _mainEvent.WaitOne();
        Debug.Log("启动第二次操作...");
        Thread.Sleep(TimeSpan.FromSeconds(seconds));
        Debug.Log("工作完成!");

        // 发送通知 主线程继续运行
        _workerEvent.Set();
    }

    void AccessDatabase(string name, int seconds)
    {
        Debug.Log($"{name} 等待访问数据库.... {DateTime.Now.ToString("HH:mm:ss.ffff")}");
        // 等待获取锁 进入临界区
        _semaphore.Wait();

        Debug.Log($"{name} 已获取对数据库的访问权限 {DateTime.Now.ToString("HH:mm:ss.ffff")}");
        Thread.Sleep(TimeSpan.FromSeconds(seconds));

        Debug.Log($"{name} 访问完成... {DateTime.Now.ToString("HH:mm:ss.ffff")}");
        // 释放锁
        _semaphore.Release();

    }
    static void Execute(CounterBaseLock c)
    {
        // 统计耗时
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        var t1 = new Thread(() => TestCounter(c));
        var t2 = new Thread(() => TestCounter(c));
        var t3 = new Thread(() => TestCounter(c));
        t1.Start();
        t2.Start();
        t3.Start();
        t1.Join();
        t2.Join();
        t3.Join();

        sw.Stop();
        Debug.Log($"Total count: {c.Count} Time:{sw.ElapsedMilliseconds} ms");
    }

    static void TestCounter(CounterBaseLock c)
    {
        for (int i = 0; i < 100000; i++)
        {
            c.Increment();
            c.Decrement();
        }
    }



    class Counter : CounterBaseLock
    {
        public override void Increment()
        {
            _count++;
        }

        public override void Decrement()
        {
            _count--;
        }
    }

    class CounterNoLock : CounterBaseLock
    {
        public override void Increment()
        {
            // 使用Interlocked执行原子操作
            Interlocked.Increment(ref _count);
        }

        public override void Decrement()
        {
            Interlocked.Decrement(ref _count);
        }
    }

    class CounterWithLock : CounterBaseLock
    {
        private readonly object _syncRoot = new System.Object();

        public override void Increment()
        {
            // 使用Lock关键字 锁定私有变量， 语句执行完自动解锁
            lock (_syncRoot)
            {
                // 同步块
                Count++;
            }
        }

        public override void Decrement()
        {
            lock (_syncRoot)
            {
                Count--;
            }
        }
    }


    abstract class CounterBaseLock
    {
        protected int _count;

        public int Count
        {
            get
            {
                return _count;
            }
            set
            {
                _count = value;
            }
        }

        public abstract void Increment();

        public abstract void Decrement();
    }


    // Update is called once per frame

}
