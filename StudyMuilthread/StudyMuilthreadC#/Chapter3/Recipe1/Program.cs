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

            // 使用APM方式 进行异步调用  异步调用会使用线程池中的线程
            IAsyncResult r = poolDelegate.BeginInvoke(out threadId, Callback, "委托异步调用");
            r.AsyncWaitHandle.WaitOne();

            // 获取异步调用结果
            string result = poolDelegate.EndInvoke(out threadId, r);

            WriteLine($"Thread - 线程池工作线程Id: {threadId}");
            WriteLine(result);

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
