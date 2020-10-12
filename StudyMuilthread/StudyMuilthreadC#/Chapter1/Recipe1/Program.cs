using System;
using System.Threading; // 创建线程需要用到的命名空间

namespace Recipe1
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1.创建一个线程 PrintNumbers为该线程所需要执行的方法
            Thread t = new Thread(PrintNumbers);
            // 2.启动线程
            t.Start();

            // 主线程也运行PrintNumbers方法，方便对照
            PrintNumbers();
            // 暂停一下
            Console.ReadKey();
        }

        static void PrintNumbers()
        {
            // 使用Thread.CurrentThread.ManagedThreadId 可以获取当前运行线程的唯一标识，通过它来区别线程
            Console.WriteLine($"线程：{Thread.CurrentThread.ManagedThreadId} 开始打印...");
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine($"线程：{Thread.CurrentThread.ManagedThreadId} 打印:{i}");
            }
        }
    }
}
