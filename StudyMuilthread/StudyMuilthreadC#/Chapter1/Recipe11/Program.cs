using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Recipe11
{
    class Program
    {
        static void Main(string[] args)
        {
            // 启动线程，线程代码中进行异常处理
            var t = new Thread(FaultyThread);
            t.Start();
            t.Join();

            // 捕获全局异常
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            t = new Thread(BadFaultyThread);
            t.Start();
            t.Join();

            // 线程代码中不进行异常处理，尝试在主线程中捕获
            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
            try
            {
                t = new Thread(BadFaultyThread);
                t.Start();
            }
            catch (Exception ex)
            {
                // 永远不会运行
                Console.WriteLine($"异常处理 3 : {ex.Message}");
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine($"异常处理 2 ：{(e.ExceptionObject as Exception).Message}");
        }

        static void BadFaultyThread()
        {
            Console.WriteLine("有异常的线程已启动...");
            Thread.Sleep(TimeSpan.FromSeconds(2));
            throw new Exception("Boom!");
        }

        static void FaultyThread()
        {
            try
            {
                Console.WriteLine("有异常的线程已启动...");
                Thread.Sleep(TimeSpan.FromSeconds(1));
                throw new Exception("Boom!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"异常处理 1 : {ex.Message}");
            }
        }
    }
}
