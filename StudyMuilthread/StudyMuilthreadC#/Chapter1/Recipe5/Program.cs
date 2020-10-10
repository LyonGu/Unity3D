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
            Console.WriteLine("开始执行...");

            Thread t = new Thread(PrintNumbersWithStatus);
            Thread t2 = new Thread(DoNothing);

            // 使用ThreadState查看线程状态 此时线程未启动，应为Unstarted
            Console.WriteLine($"Check 1 :{t.ThreadState}");

            t2.Start();
            t.Start();

            // 线程启动， 状态应为 Running
            Console.WriteLine($"Check 2 :{t.ThreadState}");

            // 由于PrintNumberWithStatus开始执行，状态为Running
            // 但是经接着Thread.Sleep 状态会转为 WaitSleepJoin
            for (int i = 1; i < 30; i++)
            {
                Console.WriteLine($"Check 3 : {t.ThreadState}");
            }

            // 延时一段时间，方便查看状态
            Thread.Sleep(TimeSpan.FromSeconds(6));

            // 终止线程
            t.Abort();

            Console.WriteLine("t线程被终止");

            // 由于该线程是被Abort方法终止 所以状态为 Aborted或AbortRequested
            Console.WriteLine($"Check 4 : {t.ThreadState}");
            // 该线程正常执行结束 所以状态为Stopped
            Console.WriteLine($"Check 5 : {t2.ThreadState}");

            Console.ReadKey();
        }

        static void DoNothing()
        {
            Thread.Sleep(TimeSpan.FromSeconds(2));
        }

        static void PrintNumbersWithStatus()
        {
            Console.WriteLine("t线程开始执行...");

            // 在线程内部，可通过Thread.CurrentThread拿到当前线程Thread对象
            Console.WriteLine($"Check 6 : {Thread.CurrentThread.ThreadState}");
            for (int i = 1; i < 10; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(2));
                Console.WriteLine($"t线程输出 ：{i}");
            }
        }
    }
}
