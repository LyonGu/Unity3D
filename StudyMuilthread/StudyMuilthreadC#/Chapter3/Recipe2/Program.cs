using System;
using System.Threading;
using static System.Console;
using static System.Threading.Thread;

namespace Recipe2
{
    class Program
    {
        static void Main(string[] args)
        {
            const int x = 1;
            const int y = 2;
            const string lambdaState = "lambda state 2";

            // 直接将方法传递给线程池
            ThreadPool.QueueUserWorkItem(AsyncOperation);
            Sleep(TimeSpan.FromSeconds(1));

            // 直接将方法传递给线程池 并且 通过state传递参数
            ThreadPool.QueueUserWorkItem(AsyncOperation, "async state");
            Sleep(TimeSpan.FromSeconds(1));

            // 使用Lambda表达式将任务传递给线程池 并且通过 state传递参数
            ThreadPool.QueueUserWorkItem(state =>
            {
                WriteLine($"Operation state: {state}");
                WriteLine($"工作线程 id: {CurrentThread.ManagedThreadId}");
                Sleep(TimeSpan.FromSeconds(2));
            }, "lambda state");

            // 使用Lambda表达式将任务传递给线程池 通过 **闭包** 机制传递参数
            ThreadPool.QueueUserWorkItem(_ =>
            {
                WriteLine($"Operation state: {x + y}, {lambdaState}");
                WriteLine($"工作线程 id: {CurrentThread.ManagedThreadId}");
                Sleep(TimeSpan.FromSeconds(2));
            }, "lambda state");

            ReadLine();
        }

        private static void AsyncOperation(object state)
        {
            WriteLine($"Operation state: {state ?? "(null)"}");
            WriteLine($"工作线程 id: {CurrentThread.ManagedThreadId}");
            Sleep(TimeSpan.FromSeconds(2));
        }
    }
}
