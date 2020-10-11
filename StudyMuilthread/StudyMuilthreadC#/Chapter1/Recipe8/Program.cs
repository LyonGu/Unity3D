using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Recipe8
{
    class Program
    {
        static void Main(string[] args)
        {
            // 第一种方法 通过构造函数传值
            var sample = new ThreadSample(10);

            var threadOne = new Thread(sample.CountNumbers);
            threadOne.Name = "ThreadOne";
            threadOne.Start();
            threadOne.Join();

            Console.WriteLine("--------------------------");

            // 第二种方法 使用Start方法传值 
            // Count方法 接收一个Object类型参数
            var threadTwo = new Thread(Count);
            threadTwo.Name = "ThreadTwo";
            // Start方法中传入的值 会传递到 Count方法 Object参数上
            threadTwo.Start(8);
            threadTwo.Join();

            Console.WriteLine("--------------------------");

            // 第三种方法 Lambda表达式传值
            // 实际上是构建了一个匿名函数 通过函数闭包来传值
            var threadThree = new Thread(() => CountNumbers(12));
            threadThree.Name = "ThreadThree";
            threadThree.Start();
            threadThree.Join();
            Console.WriteLine("--------------------------");

            // Lambda表达式传值 会共享变量值
            int i = 10;
            var threadFour = new Thread(() => PrintNumber(i));
            i = 20;
            var threadFive = new Thread(() => PrintNumber(i));
            threadFour.Start();
            threadFive.Start();

            Console.ReadKey();
        }

        static void Count(object iterations)
        {
            CountNumbers((int)iterations);
        }

        static void CountNumbers(int iterations)
        {
            for (int i = 1; i <= iterations; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
                Console.WriteLine($"{Thread.CurrentThread.Name} prints {i}");
            }
        }

        static void PrintNumber(int number)
        {
            Console.WriteLine(number);
        }

        class ThreadSample
        {
            private readonly int _iterations;

            public ThreadSample(int iterations)
            {
                _iterations = iterations;
            }
            public void CountNumbers()
            {
                for (int i = 1; i <= _iterations; i++)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(0.5));
                    Console.WriteLine($"{Thread.CurrentThread.Name} prints {i}");
                }
            }
        }
    }
}
