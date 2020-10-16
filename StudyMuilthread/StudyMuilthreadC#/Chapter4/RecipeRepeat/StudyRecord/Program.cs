using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyRecord
{
    class Program
    {
        static void Main(string[] args)
        {
            LearnDelegateAyns();


            Console.ReadKey();
        }


        #region 学习笔记1:复习委托，并且通过委托的异步调用开启一个新线程和异步回调、异步等待
        static void LearnDelegateAyns()
        {
            /*
                一. 再谈委托

                1. 委托是一个关键字为delegate的自定义类型，通过委托可以把方法以参数的形式传递给另外一个方法，实现插件式的开发模式；

                    同时调用委托的时候，委托所包含的所有方法都会被实现。

                2. 委托的发展历史：new实例化传递方法→直接等于方法名→delegate匿名方法→省略delegate→省略括号中的参数→当只有一个参数省略小括号

　　　　　　　　　                 →当方法体只有一行，省略大括号

　                (详见：http://www.cnblogs.com/yaopengfei/p/6959141.html)

                3：常用的Action委托和Func委托

　　                A. Action<>委托，无返回值，至少有一个参数的委托

　　                B. Func<>委托，有返回值，可以无参数的委托(当然也可以有参数)

　　                C. Action委托，无参数无返回值的委托 

                二. 委托的调用

                委托的调用分为两种：

　　                A. 同步调用：Invoke方法，方法参数为函数的参数。

　　                B. 异步调用：BeginInvoke方法。

                其中无论是哪类调用，都有两类写法：

　　                ①：利用Action<>(或Func<>)内置委托，调用的时候赋值。

　　                ②：利用Action委托，直接赋值，然后调用。
             */

            #region 1- 同步调用 Invoke
            Action<string, string> myFunc = TestThread2;
            for (int i = 0; i < 5; i++)
            {
                string name = $"button1_click{i}";
                myFunc.Invoke(name, name );
            }
            #endregion

        }

        /// <summary>
        /// 执行动作:耗时而已
        /// </summary>
        static private void TestThread2(string threadName1, string threadName2)
        {
              Console.WriteLine("线程开始：线程名为：{2}和{3}，当前线程的id为:{0}，当前时间为：{1},", System.Threading.Thread.CurrentThread.ManagedThreadId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"), threadName1, threadName2);
              long sum = 0;
              for (int i = 1; i < 999999999; i++)
              {
                 sum += i;             }             Console.WriteLine("线程结束：线程名为：{2}和{3}，当前线程的id为::{0}，当前时间为：{1}", System.Threading.Thread.CurrentThread.ManagedThreadId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"), threadName1, threadName2);
         }
        #endregion
        //void Print()
        //{
        //    Console.WriteLine("hello world");
        //    Console.ReadLine();
        //}
    }
}
