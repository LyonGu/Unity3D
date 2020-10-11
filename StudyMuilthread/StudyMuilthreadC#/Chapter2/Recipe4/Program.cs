using System;
using System.Threading;

namespace Recipe4
{
    class Program
    {
        static void Main(string[] args)
        {
            var t = new Thread(() => Process(3));
            t.Start();

            Console.WriteLine("等待另一个线程完成工作！");
            // 等待工作线程通知 主线程阻塞
            _workerEvent.WaitOne();
            Console.WriteLine("第一个操作已经完成！");
            Console.WriteLine("在主线程上执行操作");
            Thread.Sleep(TimeSpan.FromSeconds(5));

            // 发送通知 工作线程继续运行
            _mainEvent.Set(); //事件内核变量置为true，发送事件通知阻塞处解除阻塞，但是并不是立即执行
            Console.WriteLine("主线程发事件后我还是优先执行======"); //这句代码会在子线程等待处优先执行
            Console.WriteLine("现在在第二个线程上运行第二个操作");

            // 等待工作线程通知 主线程阻塞
            _workerEvent.WaitOne();
            Console.WriteLine("第二次操作完成！");

            Console.ReadLine();
        }

        // 工作线程Event
        private static AutoResetEvent _workerEvent = new AutoResetEvent(false);
        // 主线程Event
        private static AutoResetEvent _mainEvent = new AutoResetEvent(false);

        static void Process(int seconds)
        {
            Console.WriteLine("开始长时间的工作...");
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            Console.WriteLine("工作完成!");

            // 发送通知 主线程继续运行
            _workerEvent.Set(); //事件内核变量置为true，发送事件通知阻塞处解除阻塞，但是并不是立即执行
            Console.WriteLine("子线程发事件后我还是优先执行======");
            Console.WriteLine("等待主线程完成其它工作"); //这句代码会在主线程等待处优先执行

            // 等待主线程通知 工作线程阻塞
            _mainEvent.WaitOne();
            Console.WriteLine("启动第二次操作...");
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            Console.WriteLine("工作完成!");

            // 发送通知 主线程继续运行
            _workerEvent.Set();
        }
    }
}
