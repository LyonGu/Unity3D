using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Recipe3
{
    class Program
    {
        static void Main(string[] args)
        {
            // 创建6个线程 竞争访问AccessDatabase
            for (int i = 1; i <= 6; i++)
            {
                string threadName = "线程 " + i;
                // 越后面的线程，访问时间越久 方便查看效果
                int secondsToWait = 2 + 2 * i;
                var t = new Thread(() => AccessDatabase(threadName, secondsToWait));
                t.Start();
            }

            Console.ReadLine();
        }

        // 同时允许4个线程访问
        static SemaphoreSlim _semaphore = new SemaphoreSlim(4);

        static void AccessDatabase(string name, int seconds)
        {
            Console.WriteLine($"{name} 等待访问数据库.... {DateTime.Now.ToString("HH:mm:ss.ffff")}");

            // 等待获取锁 进入临界区
            _semaphore.Wait();

            Console.WriteLine($"{name} 已获取对数据库的访问权限 {DateTime.Now.ToString("HH:mm:ss.ffff")}");
            // Do something
            Thread.Sleep(TimeSpan.FromSeconds(seconds));

            Console.WriteLine($"{name} 访问完成... {DateTime.Now.ToString("HH:mm:ss.ffff")}");
            // 释放锁
            _semaphore.Release();
        }
    }
}
