using System;
using System.Threading.Tasks;
using static System.Console;
using static System.Threading.Thread;

namespace Recipe7
{
    class Program
    {
        static void Main(string[] args)
        {
            Task<int> task;
            // 在主线程中调用 task.Result task中的异常信息会直接抛出到 主线程中
            try
            {
                task = Task.Run(() => TaskMethod("Task 1", 2));
                int result = task.Result;
                WriteLine($"结果为: {result}");
            }
            catch (Exception ex)
            {
                WriteLine($"Task 1 异常被捕捉：{ex.Message}");
            }
            WriteLine("------------------------------------------------");
            WriteLine();

            // 同上 只是访问Result的方式不同
            try
            {
                task = Task.Run(() => TaskMethod("Task 2", 2));
                int result = task.GetAwaiter().GetResult();
                WriteLine($"结果为：{result}");
            }
            catch (Exception ex)
            {
                WriteLine($"Task 2 异常被捕捉: {ex.Message}");
            }
            WriteLine("----------------------------------------------");
            WriteLine();

            var t1 = new Task<int>(() => TaskMethod("Task 3", 3));
            var t2 = new Task<int>(() => TaskMethod("Task 4", 4));

            var complexTask = Task.WhenAll(t1, t2);
            // 通过ContinueWith TaskContinuationOptions.OnlyOnFaulted的方式 如果task出现异常 那么才会执行该方法
            var exceptionHandler = complexTask.ContinueWith(t => {
                WriteLine($"Task.WhenAll 异常被捕捉：{t.Exception.Message}");
                foreach (var ex in t.Exception.InnerExceptions)
                {
                    WriteLine($"Task.WhenAll-------------------------- {ex.Message}");
                }
            },TaskContinuationOptions.OnlyOnFaulted);

            t1.Start();
            t2.Start();

            ReadLine();
        }

        static int TaskMethod(string name, int seconds)
        {
            WriteLine($"{name} 任务运行在{CurrentThread.ManagedThreadId}上. 是否为线程池线程：{CurrentThread.IsThreadPoolThread}");

            Sleep(TimeSpan.FromSeconds(seconds));
            // 人为抛出一个异常
            throw new Exception($"{name} Boom!");
            return 42 * seconds;
        }
    }
}
