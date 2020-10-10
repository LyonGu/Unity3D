using System;
using System.Threading;
using System.Diagnostics;

namespace Recipe1
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("错误的计数");

            var c = new Counter();
            Execute(c);

            Console.WriteLine("--------------------------");


            Console.WriteLine("正确的计数 - 有锁");

            var c2 = new CounterWithLock();
            Execute(c2);

            Console.WriteLine("--------------------------");


            Console.WriteLine("正确的计数 - 无锁");

            var c3 = new CounterNoLock();
            Execute(c3);

            Console.ReadLine();
        }

        static void Execute(CounterBase c)
        {
            // 统计耗时
            var sw = new Stopwatch();
            sw.Start();

            var t1 = new Thread(() => TestCounter(c));
            var t2 = new Thread(() => TestCounter(c));
            var t3 = new Thread(() => TestCounter(c));
            t1.Start();
            t2.Start();
            t3.Start();
            t1.Join();
            t2.Join();
            t3.Join();

            sw.Stop();
            Console.WriteLine($"Total count: {c.Count} Time:{sw.ElapsedMilliseconds} ms");
        }

        static void TestCounter(CounterBase c)
        {
            for (int i = 0; i < 100000; i++)
            {
                c.Increment();
                c.Decrement();
            }
        }

        class Counter : CounterBase
        {
            public override void Increment()
            {
                _count++;
            }

            public override void Decrement()
            {
                _count--;
            }
        }

        class CounterNoLock : CounterBase
        {
            public override void Increment()
            {
                // 使用Interlocked执行原子操作
                Interlocked.Increment(ref _count);
            }

            public override void Decrement()
            {
                Interlocked.Decrement(ref _count);
            }
        }

        class CounterWithLock : CounterBase
        {
            private readonly object _syncRoot = new Object();

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
            protected int _count;

            public int Count
            {
                get
                {
                    return _count;
                }
                set
                {
                    _count = value;
                }
            }

            public abstract void Increment();

            public abstract void Decrement();
        }
    }
}
