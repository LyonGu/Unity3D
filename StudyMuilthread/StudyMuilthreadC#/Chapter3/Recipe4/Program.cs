using System;
using System.Threading;
using static System.Console;
using static System.Threading.Thread;

namespace Recipe4
{
    class Program
    {
        static void Main(string[] args)
        {
            // 使用CancellationToken来取消任务  取消任务直接返回
            using (var cts = new CancellationTokenSource())
            {
                CancellationToken token = cts.Token;
                ThreadPool.QueueUserWorkItem(_ => AsyncOperation1(token));
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
                token.Register(() => { WriteLine("第三个任务被取消，执行回调函数。"); });
                ThreadPool.QueueUserWorkItem(_ => AsyncOperation3(token));
                Sleep(TimeSpan.FromSeconds(2));
                cts.Cancel();
            }

            ReadLine();
        }

        static void AsyncOperation1(CancellationToken token)
        {
            WriteLine("启动第一个任务.");
            for (int i = 0; i < 5; i++)
            {
                if (token.IsCancellationRequested)
                {
                    WriteLine("第一个任务被取消.");
                    return;
                }
                Sleep(TimeSpan.FromSeconds(1));
            }
            WriteLine("第一个任务运行完成.");
        }

        static void AsyncOperation2(CancellationToken token)
        {
            try
            {
                WriteLine("启动第二个任务.");

                for (int i = 0; i < 5; i++)
                {
                    token.ThrowIfCancellationRequested();
                    Sleep(TimeSpan.FromSeconds(1));
                }
                WriteLine("第二个任务运行完成.");
            }
            catch (OperationCanceledException)
            {
                WriteLine("第二个任务被取消.");
            }
        }

        static void AsyncOperation3(CancellationToken token)
        {
            WriteLine("启动第三个任务.");
            for (int i = 0; i < 5; i++)
            {
                if (token.IsCancellationRequested)
                {
                    WriteLine("第三个任务被取消.");
                    return;
                }
                Sleep(TimeSpan.FromSeconds(1));
            }
            WriteLine("第三个任务运行完成.");
        }
    }
}
