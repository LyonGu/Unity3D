using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/*
    TPL内部使用了线程池，但是效率更高。在把线程归还回线程池之前，它会在同一线程中顺序执行多少Task，这样避免了一些小任务上下文切换浪费时间片的问题。
     
 */
public class StudyTPL : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //创建任务
        //CreateTask();

        //使用任务执行基本的操作
        //RunBaseOperation();

        //组合任务
        //RunTaskCombination();

        //取消任务 使用CancellationToken
        //RunCancleTask();

        //并行执行任务 Task.WhenAll 和 Task.WhenAny
        RunTaskParallel();

    }


    #region 测试接口

    void CreateTask()
    {
        // 使用构造方法创建任务  需要手动启动 调用Start
        var t1 = new Task(()=> TaskMethod("Task 1"));
        var t2 = new Task(() => TaskMethod("Task 2"));
        t2.Start();
        t1.Start();

        // 使用Task.Run 方法启动任务  不需要手动启动
        Task.Run(() => TaskMethod("Task 3"));

        // 使用 Task.Factory.StartNew方法 启动任务 实际上就是Task.Run
        Task.Factory.StartNew(() => TaskMethod("Task 4"));

        // 在StartNew的基础上 添加 TaskCreationOptions.LongRunning 告诉 Factory该任务需要长时间运行
        // 那么它就会可能会创建一个 非线程池线程来执行任务  
        Task.Factory.StartNew(() => TaskMethod("Task 5"), TaskCreationOptions.LongRunning);

    }

    void RunBaseOperation()
    {
        // 直接执行方法 作为参照
        TaskMethodOperation("主线程任务");

        // 访问 Result属性 达到运行结果
        Task<int> task = CreateTask("Task 1");
        task.Start();
        int result = task.Result; //当子线程没有执行完，就阻塞主线程
        Debug.Log($"1 task1------运算结果: {result}");

        // 使用当前线程，同步执行任务
        task = CreateTask("Task 2");
        task.RunSynchronously();  //居然在主线程上运行
        result = task.Result;
        Debug.Log($"2 task2------运算结果：{result}");

        // 通过循环等待 获取运行结果
        task = CreateTask("Task 3");
        Debug.Log($"3=====Task 3 {task.Status}");
        task.Start();
        while (!task.IsCompleted)
        {
            Debug.Log($"Task 3 Loop {task.Status}");
            Thread.Sleep(TimeSpan.FromSeconds(0.5));
        }

        result = task.Result;
        Debug.Log($"4=====Task 3 {result}");
        Debug.Log($"5=====Task 3 {task.Status}");

    }

    void RunTaskCombination()
    {
        Debug.Log($"主线程 线程 Id {Thread.CurrentThread.ManagedThreadId} 当前时间 {DateTime.Now.ToString("mm:ss.ffff")}");
        // 创建两个任务
        var firstTask = new Task<int>(() => TaskMethodCombination("Frist Task", 3));
        var secondTask = new Task<int>(() => TaskMethodCombination("Second Task", 2));

        // 在默认的情况下 ContiueWith会在前面任务运行后再运行 ==> firstTask里的TaskMethod执行完才会执行这个lanmda表达式
        firstTask.ContinueWith(t => Debug.Log($"第一次运行答案是 {t.Result}. 线程Id {Thread.CurrentThread.ManagedThreadId}. 是否为线程池线程: {Thread.CurrentThread.IsThreadPoolThread}  当前时间 {DateTime.Now.ToString("mm:ss.ffff")}"));

        // 启动任务
        firstTask.Start();
        secondTask.Start();

        Thread.Sleep(TimeSpan.FromSeconds(4));

        Debug.Log($"主线程延迟 线程 Id {Thread.CurrentThread.ManagedThreadId} 当前时间 {DateTime.Now.ToString("mm:ss.ffff")}");
        // 这里会紧接着 Second Task运行后运行， 但是由于添加了 OnlyOnRanToCompletion 和 ExecuteSynchronously 所以会由运行SecondTask的线程来 运行这个任务
        //secondTask.ContinueWith里的执行方法居然在主线程上执行？？？
        Task continuation = secondTask.ContinueWith(t => Debug.Log($"第二次运行的答案是 {t.Result}. 线程Id {Thread.CurrentThread.ManagedThreadId}. 是否为线程池线程：{Thread.CurrentThread.IsThreadPoolThread} 当前时间 {DateTime.Now.ToString("mm:ss.ffff")}"), TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);

        // OnCompleted 是一个事件  当contiuation运行完成后 执行OnCompleted Action事件，
        //continuation.GetAwaiter().OnCompleted注册的方法在secondTask所在线程上执行
        continuation.GetAwaiter().OnCompleted(() => Debug.Log($"后继任务完成. 线程Id {Thread.CurrentThread.ManagedThreadId}. 是否为线程池线程 {Thread.CurrentThread.IsThreadPoolThread} 当前时间 {DateTime.Now.ToString("mm:ss.ffff")}"));

        Thread.Sleep(TimeSpan.FromSeconds(2));

        Debug.Log($"主线程 askCreationOptions.AttachedToParent=======线程 Id {Thread.CurrentThread.ManagedThreadId} 当前时间 {DateTime.Now.ToString("mm:ss.ffff")}");
        firstTask = new Task<int>(() =>
        {
            // 使用了TaskCreationOptions.AttachedToParent 将这个Task和父Task关联， 当这个Task没有结束时  父Task 状态为 WaitingForChildrenToComplete
            // 子任务和父任务同步执行
            var innerTask = Task.Factory.StartNew(() => TaskMethodCombination("Second Task", 5), TaskCreationOptions.AttachedToParent);

            //
            innerTask.ContinueWith(t => TaskMethodCombination("Thrid Task", 2), TaskContinuationOptions.AttachedToParent);

            return TaskMethodCombination("First Task", 2);
        });

        firstTask.Start();

        // 检查firstTask线程状态  根据上面的分析 首先是  Running -> WatingForChildrenToComplete -> RanToCompletion
        while (!firstTask.IsCompleted)
        {
            Debug.Log($"firstTask ======={firstTask.Status}");

            Thread.Sleep(TimeSpan.FromSeconds(0.5));
        }

        Debug.Log($"firstTask final======={firstTask.Status}");
    }

    void RunCancleTask()
    {
        var cts = new CancellationTokenSource();
        // new Task时  可以传入一个 CancellationToken对象  可以在线程创建时  变取消任务
        var longTask = new Task<int>(() => TaskMethodCancle("Task 1", 10, cts.Token), cts.Token);
        Debug.Log(longTask.Status);
        cts.Cancel(); //取消任务
        Debug.Log(longTask.Status);
        Debug.Log("第一个任务在运行前被取消.");

        // 同样的 可以通过CancellationToken对象 取消正在运行的任务
        cts = new CancellationTokenSource();
        longTask = new Task<int>(() => TaskMethodCancle("Task 2", 10, cts.Token), cts.Token);
        longTask.Start();

        for (int i = 0; i < 5; i++)
        {
            Thread.Sleep(TimeSpan.FromSeconds(0.5));
            Debug.Log($"longTask Status {longTask.Status}, 当前时间 {DateTime.Now.ToString("mm:ss.ffff")}");
        }
        cts.Cancel();  //提前取消了

        for (int i = 0; i < 5; i++)
        {
            Thread.Sleep(TimeSpan.FromSeconds(0.5));
            Debug.Log($"longTask Status {longTask.Status}, 当前时间 {DateTime.Now.ToString("mm:ss.ffff")}");
        }

        Debug.Log($"这个任务已完成，结果为{longTask.Result}");

    }

    void RunTaskParallel()
    {
        // 第一种方式 通过Task.WhenAll 等待所有任务运行完成
        var firstTask = new Task<int>(() => TaskMethodParallel("First Task", 3));
        var secondTask = new Task<int>(() => TaskMethodParallel("Second Task", 2));

        // 当firstTask 和 secondTask 运行完成后 才执行 whenAllTask的ContinueWith
        var whenAllTask = Task.WhenAll(firstTask, secondTask);
        whenAllTask.ContinueWith(t => Debug.Log($"第一个任务答案为{t.Result[0]}，第二个任务答案为{t.Result[1]}"), TaskContinuationOptions.OnlyOnRanToCompletion);

        firstTask.Start();
        secondTask.Start();

        Thread.Sleep(TimeSpan.FromSeconds(4));

        // 使用WhenAny方法  只要列表中有一个任务完成 那么该方法就会取出那个完成的任务
        var tasks = new List<Task<int>>();
        for (int i = 0; i < 4; i++)
        {
            int counter = 1;
            var task = new Task<int>(() => TaskMethodParallel($"Task {counter}", counter));
            tasks.Add(task);
            task.Start();
        }

        while (tasks.Count > 0)
        {
            var completedTask = Task.WhenAny(tasks).Result; //这里会阻塞主线程
            tasks.Remove(completedTask);
            Debug.Log($"一个任务已经完成，结果为 {completedTask.Result}");
        }
    }

    #endregion

    static int TaskMethodParallel(string name, int seconds)
    {
        Debug.Log($"{name} 任务运行在{Thread.CurrentThread.ManagedThreadId}上. 是否为线程池线程：{Thread.CurrentThread.IsThreadPoolThread}");

        Thread.Sleep(TimeSpan.FromSeconds(seconds));
        return 42 * seconds;
    }

    int TaskMethodCancle(string name, int seconds, CancellationToken token)
    {
        Debug.Log($"任务 {name} 运行在{Thread.CurrentThread.ManagedThreadId}上. 是否为线程池线程：{Thread.CurrentThread.IsThreadPoolThread}");

        for (int i = 0; i < seconds; i++)
        {
            Thread.Sleep(TimeSpan.FromSeconds(1));
            if (token.IsCancellationRequested)
            {
                return -1;
            }
        }

        return 42 * seconds;
    }

    int TaskMethodCombination(string name, int seconds)
    {
        Debug.Log($"任务 {name} 正在运行,线程池线程 Id {Thread.CurrentThread.ManagedThreadId},是否为线程池线程: {Thread.CurrentThread.IsThreadPoolThread} 当前时间 {DateTime.Now.ToString("mm:ss.ffff")}");

        Thread.Sleep(TimeSpan.FromSeconds(seconds));

        return 42 * seconds;
    }

    void TaskMethod(string name)
    {
        Debug.Log($"任务 {name} 运行，线程 id {Thread.CurrentThread.ManagedThreadId}. 是否为线程池线程: {Thread.CurrentThread.IsThreadPoolThread}.");
    }

    Task<int> CreateTask(string name)
    {
        return new Task<int>(() => TaskMethodOperation(name));  //结果值为int类型
    }

    int TaskMethodOperation(string name)
    {
        Debug.Log($"{name} 运行在线程 {Thread.CurrentThread.ManagedThreadId}上. 是否为线程池线程 {Thread.CurrentThread.IsThreadPoolThread}");

        Thread.Sleep(TimeSpan.FromSeconds(2));

        return 42;
    }

}
