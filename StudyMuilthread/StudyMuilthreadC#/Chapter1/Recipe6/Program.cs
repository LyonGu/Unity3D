using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Recipe6
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"当前线程优先级: {Thread.CurrentThread.Priority} \r\n");

            // 第一次测试，在所有核心上运行
            Console.WriteLine("运行在所有空闲的核心上");
            RunThreads();
            Thread.Sleep(TimeSpan.FromSeconds(2));

            // 第二次测试，在单个核心上运行
            Console.WriteLine("\r\n运行在单个核心上");
            // 设置在单个核心上运行
            System.Diagnostics.Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(1);
            RunThreads();

            Console.ReadLine();
        }

        static void RunThreads()
        {
            var sample = new ThreadSample();

            var threadOne = new Thread(sample.CountNumbers);
            threadOne.Name = "线程一";
            var threadTwo = new Thread(sample.CountNumbers);
            threadTwo.Name = "线程二";

            // 设置优先级和启动线程
            threadOne.Priority = ThreadPriority.Highest;
            threadTwo.Priority = ThreadPriority.Lowest;
            threadOne.Start();
            threadTwo.Start();

            // 延时2秒 查看结果
            Thread.Sleep(TimeSpan.FromSeconds(2));
            sample.Stop();
        }

        class ThreadSample
        {
            private bool _isStopped = false;

            public void Stop()
            {
                _isStopped = true;
            }

            public void CountNumbers()
            {
                long counter = 0;

                while (!_isStopped)
                {
                    counter++;
                }

                Console.WriteLine($"{Thread.CurrentThread.Name} 优先级为 {Thread.CurrentThread.Priority,11} 计数为 = {counter,13:N0}");
            }
        }
    }
}
