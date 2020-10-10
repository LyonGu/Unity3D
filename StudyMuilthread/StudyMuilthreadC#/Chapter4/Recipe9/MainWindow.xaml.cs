using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static System.Threading.Thread;

namespace Recipe9
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // 同步执行 计算密集任务 导致UI线程阻塞
        private void ButtonSync_Click(object sender, RoutedEventArgs e)
        {
            ContentTextBlock.Text = string.Empty;

            try
            {
                string result = TaskMethod().Result;
                ContentTextBlock.Text = result;
            }
            catch (Exception ex)
            {
                ContentTextBlock.Text = ex.InnerException.Message;
            }
        }

        // 异步的方式来执行 计算密集任务 UI线程不会阻塞 但是 不能跨线程更新UI 所以会有异常
        private void ButtonAsync_Click(object sender, RoutedEventArgs e)
        {
            ContentTextBlock.Text = string.Empty;
            Mouse.OverrideCursor = Cursors.Wait;

            Task<string> task = TaskMethod();
            task.ContinueWith(t => {
                ContentTextBlock.Text = t.Exception.InnerException.Message;
                Mouse.OverrideCursor = null;
            }, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.FromCurrentSynchronizationContext());
        }

        // 通过 异步 和 FromCurrentSynchronizationContext方法 创建了线程同步的上下文  没有跨线程更新UI 
        private void ButtonAsyncOK_Click(object sender, RoutedEventArgs e)
        {
            ContentTextBlock.Text = string.Empty;
            Mouse.OverrideCursor = Cursors.Wait;
            Task<string> task = TaskMethod(TaskScheduler.FromCurrentSynchronizationContext());

            task.ContinueWith(t => Mouse.OverrideCursor = null,
                CancellationToken.None,
                TaskContinuationOptions.None,
                TaskScheduler.FromCurrentSynchronizationContext());
        }

        Task<string> TaskMethod()
        {
            return TaskMethod(TaskScheduler.Default);
        }

        Task<string> TaskMethod(TaskScheduler scheduler)
        {
            Task delay = Task.Delay(TimeSpan.FromSeconds(5));

            return delay.ContinueWith(t =>
            {
                string str = $"任务运行在{CurrentThread.ManagedThreadId}上. 是否为线程池线程：{CurrentThread.IsThreadPoolThread}";

                Console.WriteLine(str);

                ContentTextBlock.Text = str;
                return str;
            }, scheduler);
        }
    }
}
