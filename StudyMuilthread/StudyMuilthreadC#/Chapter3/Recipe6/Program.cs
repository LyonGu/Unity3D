using System;
using System.Threading;
using static System.Console;
using static System.Threading.Thread;

namespace Recipe6
{
    class Program
    {
        static void Main(string[] args)
        {
            WriteLine("按下回车键，结束定时器...");
            DateTime start = DateTime.Now;

            // 创建定时器
            _timer = new Timer(_ => TimerOperation(start), null
                , TimeSpan.FromSeconds(1)
                , TimeSpan.FromSeconds(2));
            try
            {
                Sleep(TimeSpan.FromSeconds(6));

                _timer.Change(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(4));

                ReadLine();
            }
            finally
            {
                //实现了IDispose接口  要及时释放
                _timer.Dispose();
            }
        }

        static Timer _timer;

        static void TimerOperation(DateTime start)
        {
            TimeSpan elapsed = DateTime.Now - start;
            WriteLine($"离 {start} 过去了 {elapsed.Seconds} 秒. " +
                      $"定时器线程池 线程 id: {CurrentThread.ManagedThreadId}");
        }
    }
}
