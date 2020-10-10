using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Recipe7
{
    class Program
    {
        static void Main(string[] args)
        {
            var t1 = new Thread(() => PlayMusic("钢琴家", "演奏一首令人惊叹的独奏曲", 5));
            var t2 = new Thread(() => PlayMusic("歌手", "唱着他的歌", 2));

            t1.Start();
            t2.Start();

            Console.ReadLine();
        }

        static Barrier _barrier = new Barrier(2,
    b => Console.WriteLine($"第 {b.CurrentPhaseNumber + 1} 阶段结束"));

        static void PlayMusic(string name, string message, int seconds)
        {
            for (int i = 1; i < 3; i++)
            {
                Console.WriteLine("----------------------------------------------");
                Thread.Sleep(TimeSpan.FromSeconds(seconds));
                Console.WriteLine($"{name} 开始 {message}");
                Thread.Sleep(TimeSpan.FromSeconds(seconds));
                Console.WriteLine($"{name} 结束 {message}");
                _barrier.SignalAndWait();
            }
        }
    }
}
