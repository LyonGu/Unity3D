using System;
using System.Threading;
using System.Threading.Tasks;
using static System.Console;
using static System.Threading.Thread;

namespace Recipe6
{
    class Program
    {
        static void Main(string[] args)
        {
            var cts = new CancellationTokenSource();
            // new Task时  可以传入一个 CancellationToken对象  可以在线程创建时  变取消任务
            var longTask = new Task<int>(() => TaskMethod("Task 1", 10, cts.Token), cts.Token);
            WriteLine(longTask.Status);
            cts.Cancel();
            WriteLine(longTask.Status);
            WriteLine("第一个任务在运行前被取消.");

            // 同样的 可以通过CancellationToken对象 取消正在运行的任务
            cts = new CancellationTokenSource();
            longTask = new Task<int>(() => TaskMethod("Task 2", 10, cts.Token), cts.Token);
            longTask.Start();

            for (int i = 0; i < 5; i++)
            {
                Sleep(TimeSpan.FromSeconds(0.5));
                WriteLine($"longTask Status {longTask.Status}, 当前时间 {DateTime.Now.ToString("mm:ss.ffff")}");
            }
            cts.Cancel();
            for (int i = 0; i < 5; i++)
            {
                Sleep(TimeSpan.FromSeconds(0.5));
                WriteLine($"longTask Status {longTask.Status}, 当前时间 {DateTime.Now.ToString("mm:ss.ffff")}");
            }

            WriteLine($"这个任务已完成，结果为{longTask.Result}");

            ReadLine();
        }

        static int TaskMethod(string name, int seconds, CancellationToken token)
        {
            WriteLine($"任务运行在{CurrentThread.ManagedThreadId}上. 是否为线程池线程：{CurrentThread.IsThreadPoolThread}");

            for (int i = 0; i < seconds; i++)
            {
                Sleep(TimeSpan.FromSeconds(1));
                if (token.IsCancellationRequested)
                {
                    return -1;
                }
            }

            return 42 * seconds;
        }
    }
}
