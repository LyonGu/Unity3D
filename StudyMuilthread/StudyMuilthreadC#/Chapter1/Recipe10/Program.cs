using System;
using System.Threading;

namespace Recipe10
{
    class Program
    {
        static void Main(string[] args)
        {
            object lock1 = new object();
            object lock2 = new object();

            new Thread(() => LockTooMuch(lock1, lock2)).Start();

            lock (lock2)
            {
                Thread.Sleep(1000);
                Console.WriteLine("Monitor.TryEnter可以不被阻塞, 在超过指定时间后返回false");
                // 如果5S不能进入同步块，那么返回。
                // 因为前面的lock锁定了 lock2变量  而LockTooMuch()一开始锁定了lock1 所以这个同步块无法获取 lock1 而LockTooMuch方法内也不能获取lock2
                // 只能等待TryEnter超时 释放 lock2 LockTooMuch()才会是释放 lock1
                if (Monitor.TryEnter(lock1, TimeSpan.FromSeconds(5)))
                {
                    Console.WriteLine("获取保护资源成功");
                }
                else
                {
                    Console.WriteLine("获取资源超时");
                }
            }

            new Thread(() => LockTooMuch(lock1, lock2)).Start();

            Console.WriteLine("----------------------------------");
            lock (lock2)
            {
                Console.WriteLine("这里会发生资源死锁");
                Thread.Sleep(1000);
                // 这里必然会发生死锁  
                // 本同步块 锁定了 lock2 无法得到 lock1
                // 而 LockTooMuch 锁定了 lock1 无法得到 lock2
                lock (lock1)
                {
                    // 该语句永远都不会执行
                    Console.WriteLine("获取保护资源成功");
                }
            }
        }

        static void LockTooMuch(object lock1, object lock2)
        {
            lock (lock1)
            {
                Thread.Sleep(1000);
                lock (lock2) ;
            }
        }
    }
}
