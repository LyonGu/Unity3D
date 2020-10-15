using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static System.Console;
using static System.Threading.Thread;

namespace Recipe8
{
    class Program
    {
        static void Main(string[] args)
        {
            // 第一种方式 通过Task.WhenAll 等待所有任务运行完成
            var firstTask = new Task<int>(() => TaskMethod("First Task", 3));
            var secondTask = new Task<int>(() => TaskMethod("Second Task", 2));

            // 当firstTask 和 secondTask 运行完成后 才执行 whenAllTask的ContinueWith
            var whenAllTask = Task.WhenAll(firstTask, secondTask);
            whenAllTask.ContinueWith(t => WriteLine($"第一个任务答案为{t.Result[0]}，第二个任务答案为{t.Result[1]}"), TaskContinuationOptions.OnlyOnRanToCompletion);

            firstTask.Start();
            secondTask.Start();

            Sleep(TimeSpan.FromSeconds(4));

            // 使用WhenAny方法  只要列表中有一个任务完成 那么该方法就会取出那个完成的任务
            var tasks = new List<Task<int>>();
            for (int i = 0; i < 4; i++)
            {
                int counter = 1;
                var task = new Task<int>(() => TaskMethod($"Task {counter}",counter));
                tasks.Add(task);
                task.Start();
            }

            while (tasks.Count > 0)
            {
                var completedTask = Task.WhenAny(tasks).Result;
                tasks.Remove(completedTask);
                WriteLine($"一个任务已经完成，结果为 {completedTask.Result}");
            }

            ReadLine();
        }

        static int TaskMethod(string name, int seconds)
        {
            WriteLine($"{name} 任务运行在{CurrentThread.ManagedThreadId}上. 是否为线程池线程：{CurrentThread.IsThreadPoolThread}");

            Sleep(TimeSpan.FromSeconds(seconds));
            return 42 * seconds;
        }
    }
}
