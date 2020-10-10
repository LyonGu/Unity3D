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
            // 创建3个 读线程
            new Thread(() => Read("Reader 1")) { IsBackground = true }.Start();
            new Thread(() => Read("Reader 2")) { IsBackground = true }.Start();
            new Thread(() => Read("Reader 3")) { IsBackground = true }.Start();

            // 创建两个写线程
            new Thread(() => Write("Writer 1")) { IsBackground = true }.Start();
            new Thread(() => Write("Writer 2")) { IsBackground = true }.Start();

            // 使程序运行30S
            Thread.Sleep(TimeSpan.FromSeconds(30));

            Console.ReadLine();
        }

        static ReaderWriterLockSlim _rw = new ReaderWriterLockSlim();
        static Dictionary<int, int> _items = new Dictionary<int, int>();

        static void Read(string threadName)
        {
            while (true)
            {
                try
                {
                    // 获取读锁定
                    _rw.EnterReadLock();
                    Console.WriteLine($"{threadName} 从字典中读取内容  {DateTime.Now.ToString("mm:ss.ffff")}");
                    foreach (var key in _items.Keys)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(0.1));
                    }
                }
                finally
                {
                    // 释放读锁定
                    _rw.ExitReadLock();
                }
            }
        }

        static void Write(string threadName)
        {
            while (true)
            {
                try
                {
                    int newKey = new Random().Next(250);
                    // 尝试进入可升级锁模式状态
                    _rw.EnterUpgradeableReadLock();
                    if (!_items.ContainsKey(newKey))
                    {
                        try
                        {
                            // 获取写锁定
                            _rw.EnterWriteLock();
                            _items[newKey] = 1;
                            Console.WriteLine($"{threadName} 将新的键 {newKey} 添加进入字典中  {DateTime.Now.ToString("mm:ss.ffff")}");
                        }
                        finally
                        {
                            // 释放写锁定
                            _rw.ExitWriteLock();
                        }
                    }
                    Thread.Sleep(TimeSpan.FromSeconds(0.1));
                }
                finally
                {
                    // 减少可升级模式递归计数，并在计数为0时  推出可升级模式
                    _rw.ExitUpgradeableReadLock();
                }
            }
        }
    }
}
