using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Recipe5
{
    class Program
    {
        static void Main(string[] args)
        {
            var t1 = new Thread(() => TravelThroughGates("Thread 1", 5));
            var t2 = new Thread(() => TravelThroughGates("Thread 2", 6));
            var t3 = new Thread(() => TravelThroughGates("Thread 3", 12));
            t1.Start();
            t2.Start();
            t3.Start();

            // 休眠6秒钟  只有Thread 1小于 6秒钟，所以事件重置时 Thread 1 肯定能进入大门  而 Thread 2 可能可以进入大门
            Thread.Sleep(TimeSpan.FromSeconds(6));
            Console.WriteLine($"大门现在打开了!  时间：{DateTime.Now.ToString("mm:ss.ffff")}");
            _mainEvent.Set();

            // 休眠2秒钟 此时 Thread 2 肯定可以进入大门
            Thread.Sleep(TimeSpan.FromSeconds(2));
            _mainEvent.Reset();
            Console.WriteLine($"大门现在关闭了! 时间：{DateTime.Now.ToString("mm: ss.ffff")}");

            // 休眠10秒钟 Thread 3 可以进入大门
            Thread.Sleep(TimeSpan.FromSeconds(10));
            Console.WriteLine($"大门现在第二次打开! 时间：{DateTime.Now.ToString("mm: ss.ffff")}");
            _mainEvent.Set();
            Thread.Sleep(TimeSpan.FromSeconds(2));

            Console.WriteLine($"大门现在关闭了! 时间：{DateTime.Now.ToString("mm: ss.ffff")}");
            _mainEvent.Reset();

            Console.ReadLine();
        }

        static void TravelThroughGates(string threadName, int seconds)
        {
            Console.WriteLine($"{threadName} 进入睡眠 时间：{DateTime.Now.ToString("mm:ss.ffff")}");
            Thread.Sleep(TimeSpan.FromSeconds(seconds));

            Console.WriteLine($"{threadName} 等待大门打开! 时间：{DateTime.Now.ToString("mm:ss.ffff")}");
            _mainEvent.Wait();

            Console.WriteLine($"{threadName} 进入大门! 时间：{DateTime.Now.ToString("mm:ss.ffff")}");
        }

        static ManualResetEventSlim _mainEvent = new ManualResetEventSlim(false);
    }
}
