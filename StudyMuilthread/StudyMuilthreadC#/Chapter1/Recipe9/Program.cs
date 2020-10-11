using System;
using System.Threading;

namespace Recipe9
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("错误的多线程计数方式");

            var c = new Counter();
            // 开启3个线程，使用没有同步块的计数方式对其进行计数
            var t1 = new Thread(() => TestCounter(c));
            var t2 = new Thread(() => TestCounter(c));
            var t3 = new Thread(() => TestCounter(c));
            t1.Start();
            t2.Start();
            t3.Start();

            // 这么来看，必须等三个线程执行完，主线程才会继续执行
            t1.Join();
            t2.Join();
            t3.Join();

            // 因为多线程 线程抢占等原因 其结果是不一定的  碰巧可能为0
            Console.WriteLine($"Total count: {c.Count}");
            Console.WriteLine("--------------------------");

            Console.WriteLine("正确的多线程计数方式");

            var c1 = new CounterWithLock();
            // 开启3个线程，使用带有lock同步块的方式对其进行计数
            t1 = new Thread(() => TestCounter(c1));
            t2 = new Thread(() => TestCounter(c1));
            t3 = new Thread(() => TestCounter(c1));
            t1.Start();
            t2.Start();
            t3.Start();
            t1.Join();
            t2.Join();
            t3.Join();

            // 其结果是一定的 为0
            Console.WriteLine($"Total count: {c1.Count}");

            Console.ReadLine();
        }

        static void TestCounter(CounterBase c)
        {
            for (int i = 0; i < 100000; i++)
            {
                c.Increment();
                c.Decrement();
            }
        }

        // 线程不安全的计数
        class Counter : CounterBase
        {
            public int Count { get; private set; }

            public override void Increment()
            {
                Count++;
            }

            public override void Decrement()
            {
                Count--;
            }
        }

        // 线程安全的计数
        class CounterWithLock : CounterBase
        {
            private readonly object _syncRoot = new Object();

            public int Count { get; private set; }

            public override void Increment()
            {
                // 使用Lock关键字 锁定私有变量
                lock (_syncRoot)
                {
                    // 同步块
                    Count++;
                }
            }

            public override void Decrement()
            {
                lock (_syncRoot)
                {
                    Count--;
                }
            }
        }

        abstract class CounterBase
        {
            public abstract void Increment();

            public abstract void Decrement();
        }
    }
}
