using System;
using System.Threading;

namespace Recipe6
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"开始两个操作  {DateTime.Now.ToString("mm:ss.ffff")}");
            var t1 = new Thread(() => PerformOperation("操作 1 完成！", 4));
            var t2 = new Thread(() => PerformOperation("操作 2 完成！", 8));
            t1.Start();
            t2.Start();

            // 等待操作完成
            _countdown.Wait();
            Console.WriteLine($"所有操作都完成  {DateTime.Now.ToString("mm: ss.ffff")}");
            _countdown.Dispose();

            Console.ReadLine();
        }

        // 构造函数的参数为2 表示只有调用了两次 Signal方法 CurrentCount 为 0时  Wait的阻塞才解除
        static CountdownEvent _countdown = new CountdownEvent(2);

        static void PerformOperation(string message, int seconds)
        {
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            Console.WriteLine($"{message}  {DateTime.Now.ToString("mm:ss.ffff")}");

            // CurrentCount 递减 1
            _countdown.Signal();
        }
    }
}
