using System;
using System.Threading;

namespace Recipe2
{
    class Program
    {
        const string MutexName = "CSharpThreadingCookbook";

        static void Main(string[] args)
        {
            // 使用using 及时释放资源
            using (var m = new Mutex(false, MutexName))
            {
                if (!m.WaitOne(TimeSpan.FromSeconds(5), false))
                {
                    Console.WriteLine("已经有实例正在运行!");
                }
                else
                {

                    Console.WriteLine("运行中...");

                    // 演示递归获取锁
                    Recursion();

                    Console.ReadLine();
                    m.ReleaseMutex();
                }
            }

            Console.ReadLine();
        }

        static void Recursion()
        {
            using (var m = new Mutex(false, MutexName))
            {
                if (!m.WaitOne(TimeSpan.FromSeconds(2), false))
                {
                    // 因为Mutex支持递归获取锁 所以永远不会执行到这里
                    Console.WriteLine("递归获取锁失败！");
                }
                else
                {
                    Console.WriteLine("递归获取锁成功！");
                }
            }
        }
    }
}
