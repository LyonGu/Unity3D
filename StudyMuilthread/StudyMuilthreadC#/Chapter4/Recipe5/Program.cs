using System;
using System.ComponentModel;
using System.Threading.Tasks;
using static System.Threading.Thread;
using static System.Console;

namespace Recipe5
{
    class Program
    {
        static void Main(string[] args)
        {
            var tcs = new TaskCompletionSource<int>();

            var worker = new BackgroundWorker();
            worker.DoWork += (sender, eventArgs) =>
            {
                eventArgs.Result = TaskMethod("后台工作", 5);
            };

            // 通过此方法 将EAP模式转换为 任务
            worker.RunWorkerCompleted += (sender, eventArgs) =>
            {
                if (eventArgs.Error != null)
                {
                    tcs.SetException(eventArgs.Error);
                }
                else if (eventArgs.Cancelled)
                {
                    tcs.SetCanceled();
                }
                else
                {
                    tcs.SetResult((int)eventArgs.Result);
                }
            };

            worker.RunWorkerAsync();

            // 调用结果
            int result = tcs.Task.Result;

            WriteLine($"结果是：{result}");

            ReadLine();
        }

        static int TaskMethod(string name, int seconds)
        {
            WriteLine($"任务{name}运行在线程{CurrentThread.ManagedThreadId}上. 是否为线程池线程{CurrentThread.IsThreadPoolThread}");

            Sleep(TimeSpan.FromSeconds(seconds));

            return 42 * seconds;
        }
    }
}
