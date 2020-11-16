using System;
using System.Linq;
using System.Threading;
using static System.Console;
using static System.Threading.Thread;

namespace Recipe1
{
    class Program
    {
        static void Main(string[] args)
        {
            int threadId = 0;

            RunOnThreadPool poolDelegate = Test;

            var t = new Thread(() => Test(out threadId));
            t.Start();
            t.Join();

            WriteLine($"手动创建线程 Id: {threadId}");

            WriteLine("poolDelegate============BeginInvoke");
            // 使用APM方式 进行异步调用  异步调用会使用线程池中的线程
            // 优先执行委托函数Test，在任务处理完成后调用Callback， 这里的任务处理完是调用了EndInvoke  

            ////BeginInvoke(委托函数需要参数，委托函数执行完的回调函数，用户自定义的状态给回调函数) Callback也在是线程池里线程里工作
            IAsyncResult r = poolDelegate.BeginInvoke(out threadId, Callback, "委托异步调用"); //BeginInvoke方式 就是异步调用
            r.AsyncWaitHandle.WaitOne();
            WriteLine("poolDelegate============EndInvoke");
            // 获取异步调用结果 ==》 任务处理完
            //EndInvoke调用完后会执行了Callback方法，但是跟主线程的之后的逻辑没有优先顺序，跟cpu分配时间有关
            string result = poolDelegate.EndInvoke(out threadId, r);

            WriteLine($"------------------1:");
            WriteLine($"Thread - 线程池工作线程Id: {threadId}");
            WriteLine($"poolDelegate result======{result}");
            WriteLine($"------------------2:");

            Console.ReadLine();
        }

        // 创建带一个参数的委托类型
        private delegate string RunOnThreadPool(out int threadId);

        private static void Callback(IAsyncResult ar)
        {
            WriteLine("Callback - 开始运行Callback...");
            WriteLine($"Callback - 回调传递状态: {ar.AsyncState}");
            WriteLine($"Callback - 是否为线程池线程: {CurrentThread.IsThreadPoolThread}");
            WriteLine($"Callback - 线程池工作线程Id: {CurrentThread.ManagedThreadId}");
        }

		private static string Test(out int threadId)
        {
            string isThreadPoolThread = CurrentThread.IsThreadPoolThread ? "ThreadPool - ": "Thread - ";

            WriteLine($"{isThreadPoolThread}开始运行...");
            WriteLine($"{isThreadPoolThread}是否为线程池线程: {CurrentThread.IsThreadPoolThread}");
            Sleep(TimeSpan.FromSeconds(2));
            threadId = CurrentThread.ManagedThreadId;
            return $"{isThreadPoolThread}线程池工作线程Id: {threadId}";
        }
    }
}
