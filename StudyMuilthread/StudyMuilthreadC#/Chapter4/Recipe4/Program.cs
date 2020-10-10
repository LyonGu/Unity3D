using System;
using System.Threading.Tasks;
using static System.Console;
using static System.Threading.Thread;

namespace Recipe4
{
    class Program
    {
        static void Main(string[] args)
        {
            int threadId;
            AsynchronousTask d = Test;
            IncompatibleAsychronousTask e = Test;

            // 使用 Task.Factory.FromAsync方法 转换为Task
            WriteLine("Option 1");
            Task<string> task = Task<string>.Factory.FromAsync(d.BeginInvoke("异步任务线程", CallBack, "委托异步调用"), d.EndInvoke);

            task.ContinueWith(t => WriteLine($"回调函数执行完毕，现在运行续接函数！结果：{t.Result}"));

            while (!task.IsCompleted)
            {
                WriteLine(task.Status);
                Sleep(TimeSpan.FromSeconds(0.5));
            }
            WriteLine(task.Status);
            Sleep(TimeSpan.FromSeconds(1));

            WriteLine("----------------------------------------------");
            WriteLine();

            // 使用 Task.Factory.FromAsync重载方法 转换为Task
            WriteLine("Option 2");

            task = Task<string>.Factory.FromAsync(d.BeginInvoke,d.EndInvoke,"异步任务线程","委托异步调用");

            task.ContinueWith(t => WriteLine($"任务完成，现在运行续接函数！结果：{t.Result}"));

            while (!task.IsCompleted)
            {
                WriteLine(task.Status);
                Sleep(TimeSpan.FromSeconds(0.5));
            }
            WriteLine(task.Status);
            Sleep(TimeSpan.FromSeconds(1));

            WriteLine("----------------------------------------------");
            WriteLine();

            // 同样可以使用 FromAsync方法 将 BeginInvoke 转换为 IAsyncResult 最后转换为 Task
            WriteLine("Option 3");

            IAsyncResult ar = e.BeginInvoke(out threadId, CallBack, "委托异步调用");
            task = Task<string>.Factory.FromAsync(ar, _ => e.EndInvoke(out threadId, ar));

            task.ContinueWith(t => WriteLine($"任务完成，现在运行续接函数！结果：{t.Result}，线程Id {threadId}"));

            while (!task.IsCompleted)
            {
                WriteLine(task.Status);
                Sleep(TimeSpan.FromSeconds(0.5));
            }
            WriteLine(task.Status);

            ReadLine();
        }

        delegate string AsynchronousTask(string threadName);
        delegate string IncompatibleAsychronousTask(out int threadId);
        
        static void CallBack(IAsyncResult ar)
        {
            WriteLine("开始运行回调函数...");
            WriteLine($"传递给回调函数的状态{ar.AsyncState}");
            WriteLine($"是否为线程池线程：{CurrentThread.IsThreadPoolThread}");
            WriteLine($"线程池工作线程Id：{CurrentThread.ManagedThreadId}");
        }

        static string Test(string threadName)
        {
            WriteLine("开始运行...");
            WriteLine($"是否为线程池线程：{CurrentThread.IsThreadPoolThread}");
            Sleep(TimeSpan.FromSeconds(2));

            CurrentThread.Name = threadName;
            return $"线程名：{CurrentThread.Name}";
        }

        static string Test(out int threadId)
        {
            WriteLine("开始运行...");
            WriteLine($"是否为线程池线程：{CurrentThread.IsThreadPoolThread}");
            Sleep(TimeSpan.FromSeconds(2));

            threadId = CurrentThread.ManagedThreadId;
            return $"线程池线程工作Id是：{threadId}";
        }
    }
}
