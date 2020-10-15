using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using static System.Threading.Thread;
using static System.Console;

namespace Recipe2
{
    class Program
    {
        static void Main(string[] args)
        {
            // 直接执行方法 作为参照
            TaskMethod("主线程任务");

            // 访问 Result属性 达到运行结果
            Task<int> task = CreateTask("Task 1");
            task.Start();
            int result = task.Result;
            WriteLine($"运算结果: {result}");

            // 使用当前线程，同步执行任务
            task = CreateTask("Task 2");
            task.RunSynchronously();  //居然在主线程上运行
            result = task.Result;
            WriteLine($"运算结果：{result}");

            // 通过循环等待 获取运行结果
            task = CreateTask("Task 3");
            WriteLine($"1=====Task 3 {task.Status}");
            task.Start();

            while (!task.IsCompleted)
            {
                WriteLine($"Task 3 Loop {task.Status}");
                Sleep(TimeSpan.FromSeconds(0.5));
            }

            WriteLine($"2=====Task 3 {task.Status}");
            result = task.Result;
            WriteLine($"运算结果：{result}");

            Console.ReadLine();
        }

        static Task<int> CreateTask(string name)
        {
            return new Task<int>(() => TaskMethod(name));  //结果值为int类型
        }

        static int TaskMethod(string name)
        {
            WriteLine($"{name} 运行在线程 {CurrentThread.ManagedThreadId}上. 是否为线程池线程 {CurrentThread.IsThreadPoolThread}");

            Sleep(TimeSpan.FromSeconds(2));

            return 42;
        }
    }
}
