using System;
using System.ComponentModel;
using static System.Console;
using static System.Threading.Thread;

namespace Recipe7
{
    class Program
    {
        static void Main(string[] args)
        {
            var bw = new BackgroundWorker();
            // 设置可报告进度更新
            bw.WorkerReportsProgress = true;
            // 设置支持取消操作
            bw.WorkerSupportsCancellation = true;

            // 需要做的工作
            bw.DoWork += Worker_DoWork;
            // 工作处理进度
            bw.ProgressChanged += Worker_ProgressChanged;
            // 工作完成后处理函数
            bw.RunWorkerCompleted += Worker_Completed;

            bw.RunWorkerAsync();

            WriteLine("按下 `C` 键 取消工作");
            do
            {
                if (ReadKey(true).KeyChar == 'C')
                {
                    bw.CancelAsync();
                }

            }
            while (bw.IsBusy);
        }

        static void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            WriteLine($"DoWork 线程池 线程 id: {CurrentThread.ManagedThreadId}");
            var bw = (BackgroundWorker)sender;
            for (int i = 1; i <= 100; i++)
            {
                if (bw.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                if (i % 10 == 0)
                {
                    bw.ReportProgress(i);
                }

                Sleep(TimeSpan.FromSeconds(0.1));
            }

            e.Result = 42;
        }

        static void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            WriteLine($"已完成{e.ProgressPercentage}%. " +
                      $"处理线程 id: {CurrentThread.ManagedThreadId}");
        }

        static void Worker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            WriteLine($"完成线程池线程 id: {CurrentThread.ManagedThreadId}");
            if (e.Error != null)
            {
                WriteLine($"异常 {e.Error.Message} 发生.");
            }
            else if (e.Cancelled)
            {
                WriteLine($"操作已被取消.");
            }
            else
            {
                WriteLine($"答案是 : {e.Result}");
            }
        }
    }
}
