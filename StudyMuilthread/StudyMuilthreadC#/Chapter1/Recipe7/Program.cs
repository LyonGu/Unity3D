using System;
using System.Threading;

namespace Recipe7
{
    class Program
    {
        static void Main(string[] args)
        {
            var sampleForeground = new ThreadSample(10);
            var sampleBackground = new ThreadSample(20);
            var threadPoolBackground = new ThreadSample(20);

            // 默认创建为前台线程
            var threadOne = new Thread(sampleForeground.CountNumbers);
            threadOne.Name = "前台线程";

            var threadTwo = new Thread(sampleBackground.CountNumbers);
            threadTwo.Name = "后台线程";
            // 设置IsBackground属性为 true 表示后台线程
            threadTwo.IsBackground = true;

            // 线程池内的线程默认为 后台线程
            ThreadPool.QueueUserWorkItem((obj) => {

                Thread.CurrentThread.Name = "线程池线程";
                threadPoolBackground.CountNumbers();
            });

            // 启动线程 
            threadOne.Start();
            threadTwo.Start();
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
                for (int i = 0; i < _iterations; i++)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(0.5));
                    Console.WriteLine($"{Thread.CurrentThread.Name} prints {i}");
                }
            }
        }
    }
}
