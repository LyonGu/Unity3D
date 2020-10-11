using System;
using System.Threading;

namespace Recipe3
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"-------开始执行 现在时间{DateTime.Now.ToString("HH:mm:ss.ffff")}-------");

            // 1.创建一个线程 PrintNumbersWithDelay为该线程所需要执行的方法
            Thread t = new Thread(PrintNumbersWithDelay);
            // 2.启动线程
            t.Start();
            // 3.等待线程结束
            t.Join(); //这句代码会阻塞主线程，让子线程执行完主线程才会继续执行

            Console.WriteLine($"-------执行完毕 现在时间{DateTime.Now.ToString("HH:mm:ss.ffff")}-------");
            // 暂停一下
            Console.ReadKey();
        }

        static void PrintNumbersWithDelay()
        {
            Console.WriteLine($"线程：{Thread.CurrentThread.ManagedThreadId} 开始打印... 现在时间{DateTime.Now.ToString("HH:mm:ss.ffff")}");
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(2));
                Console.WriteLine($"线程：{Thread.CurrentThread.ManagedThreadId} 打印:{i} 现在时间{DateTime.Now.ToString("HH:mm:ss.ffff")}");
            }
        }
    }
}
