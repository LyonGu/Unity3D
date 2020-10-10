using System;
using System.Threading; // 创建线程需要用到的命名空间

namespace Recipe2
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1.创建一个线程 PrintNumbers为该线程所需要执行的方法
            Thread t = new Thread(PrintNumbersWithDelay);
            // 2.启动线程
            t.Start();

            // 暂停一下
            Console.ReadKey();
        }

        static void PrintNumbersWithDelay()
        {
            Console.WriteLine($"线程：{Thread.CurrentThread.ManagedThreadId} 开始打印... 现在时间{DateTime.Now.ToString("HH:mm:ss.ffff")}");
            for (int i = 0; i < 10; i++)
            {
                //3. 使用Thread.Sleep方法来使当前线程睡眠，TimeSpan.FromSeconds(2)表示时间为 2秒
                Thread.Sleep(TimeSpan.FromSeconds(2));
                Console.WriteLine($"线程：{Thread.CurrentThread.ManagedThreadId} 打印:{i} 现在时间{DateTime.Now.ToString("HH:mm:ss.ffff")}");
            }
        }
    }
}
