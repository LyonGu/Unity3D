using System;
using System.Threading;
using static System.Console;
using static System.Threading.Thread;


namespace Recipe5
{
    class Program
    {
        static void Main(string[] args)
        {
            // 设置超时时间为 5s WorkerOperation会延时 6s 肯定会超时
            RunOperations(TimeSpan.FromSeconds(5), 1);

            // 设置超时时间为 7s 不会超时
            RunOperations(TimeSpan.FromSeconds(7),2);
        }

        static void RunOperations(TimeSpan workerOperationTimeout, int index)
        {
            using (var evt = new ManualResetEvent(false)) //跳出using语句会自动执行Dispose方法
            using (var cts = new CancellationTokenSource())
            {
                WriteLine($"{index}注册超时操作...");
                // 传入同步事件  超时处理函数  和 超时时间
                var worker = ThreadPool.RegisterWaitForSingleObject(evt
                    , (state, isTimedOut) => WorkerOperationWait(cts, isTimedOut, state)
                    , null
                    , workerOperationTimeout
                    , true);

                WriteLine("启动长时间运行操作...");
                ThreadPool.QueueUserWorkItem(_ => WorkerOperation(cts.Token, evt));

                Sleep(workerOperationTimeout.Add(TimeSpan.FromSeconds(2)));

                WriteLine($"{index}取消注册超时操作...");
                // 取消注册等待的操作
                worker.Unregister(evt);
                

                ReadLine();
            }
        }

        static void WorkerOperation(CancellationToken token, ManualResetEvent evt)
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

        static void WorkerOperationWait(CancellationTokenSource cts, bool isTimedOut, object state)
        {
            WriteLine($"WorkerOperationWait state: {state}");
            if (isTimedOut)
            {
                cts.Cancel();
                WriteLine("工作操作超时并被取消.");
            }
            else
            {
                WriteLine("工作操作成功.");
            }
        }
    }
}
