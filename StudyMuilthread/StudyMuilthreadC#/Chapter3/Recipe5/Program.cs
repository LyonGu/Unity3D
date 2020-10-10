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
            RunOperations(TimeSpan.FromSeconds(5));

            // 设置超时时间为 7s 不会超时
            RunOperations(TimeSpan.FromSeconds(7));
        }

        static void RunOperations(TimeSpan workerOperationTimeout)
        {
            using (var evt = new ManualResetEvent(false))
            using (var cts = new CancellationTokenSource())
            {
                WriteLine("注册超时操作...");
                // 传入同步事件  超时处理函数  和 超时时间
                var worker = ThreadPool.RegisterWaitForSingleObject(evt
                    , (state, isTimedOut) => WorkerOperationWait(cts, isTimedOut)
                    , null
                    , workerOperationTimeout
                    , true);

                WriteLine("启动长时间运行操作...");
                ThreadPool.QueueUserWorkItem(_ => WorkerOperation(cts.Token, evt));

                Sleep(workerOperationTimeout.Add(TimeSpan.FromSeconds(2)));

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

        static void WorkerOperationWait(CancellationTokenSource cts, bool isTimedOut)
        {
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
