using System.Threading.Tasks;
using static System.Console;
using static System.Threading.Thread;

namespace Recipe1
{
    class Program
    {
        static void Main(string[] args)
        {
            // 使用构造方法创建任务
            var t1 = new Task(() => TaskMethod("Task 1"));
            var t2 = new Task(() => TaskMethod("Task 2"));

            // 需要手动启动
            t2.Start();
            t1.Start();

            // 使用Task.Run 方法启动任务  不需要手动启动
            Task.Run(() => TaskMethod("Task 3"));

            // 使用 Task.Factory.StartNew方法 启动任务 实际上就是Task.Run
            Task.Factory.StartNew(() => TaskMethod("Task 4"));

            // 在StartNew的基础上 添加 TaskCreationOptions.LongRunning 告诉 Factory该任务需要长时间运行
            // 那么它就会可能会创建一个 非线程池线程来执行任务  
            Task.Factory.StartNew(() => TaskMethod("Task 5"), TaskCreationOptions.LongRunning);

            ReadLine();
        }

        static void TaskMethod(string name)
        {
            WriteLine($"任务 {name} 运行，线程 id {CurrentThread.ManagedThreadId}. 是否为线程池线程: {CurrentThread.IsThreadPoolThread}.");
        }
    }
}
