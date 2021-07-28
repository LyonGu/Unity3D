using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UniRx;
using UniRx.Triggers;
using UnityEngine;


// 介绍一些常用的操作符
//https://blog.csdn.net/u012338130/article/details/99674280
//https://blog.csdn.net/u012338130/article/details/99678992
//https://blog.csdn.net/u012338130/article/details/99645709
public class Example7 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
//        //NextFrame
//        Test1();
//        //DelayFrame
//        Test2();
//        
//        //FrameInterval
//        Test3();
//        
//        //ForEachAsync 遍历循环
//        Test4();
//        
//        //SampleFrame
//        Test5();
//        
//        //RepeatUntilDestroy
//        Test6();
//        
//        //ObserveOnMainThread 回到主线程
//        Test7();
//        
//        //ThrottleFirstFrame
//        Test8();
//        
//        //TakeUntilDestroy  TakeUntilDisable  RepeatUntilDisable
//        Test9();
//        
//        //Interval 每隔一段时间进行操作
//        Test10();
//        
//        //TakeUntil
//        Test11();
//        
//        //SkipUntil  丢弃原始 Observable 发射的数据，直到第二个 Observable 发射了一项数据
//        Test12();
//        
//        //Buffer 打包操作
//        Test13();
        
        //Throttle 仅在过了一段指定的时间还没发射数据时才发射一个数据
        //Test14();
        
        //Delay 延迟一段指定的时间再发射来自Observable的发射
        //Test15();
        
        //Return  立马返回
        //Test16();
        
        //DistinctUntilChanged
        Test17();
        
        //Take 从序列列的开头返回指定数量量的相邻元素
        //Skip 跳过序列中指定数量的元素，然后返回剩余的元素。
        Test18();
        
        //WhenAll 确定序列列中的所有元素是否都满⾜足条件。
    }

    private void Test18()
    {
        //输出结果为，只有前 5 次⿏鼠标点击才会输出 1
        this.UpdateAsObservable()
        .Where(_=>Input.GetMouseButtonDown(0))
        .Take(5)
        .Subscribe(_ => Debug.Log(1));
        
        //鼠标点击第六次时，才开始输出”mouse clicked”。
        this.UpdateAsObservable()
            .Where(_ => Input.GetMouseButtonDown(0))
            .Skip(5)
            .Subscribe(_ => { Debug.Log("mouse clicked"); });

    }

    private void Test17()
    {
        
        var subject = new Subject<int>();
        var distinct = subject.DistinctUntilChanged();
        subject.Subscribe(i => Debug.LogFormat("{0}", i),() => Debug.LogFormat("subject.OnCompleted()"));
        distinct.Subscribe(i => Debug.LogFormat("distinct.OnNext({0})", i),() => Debug.LogFormat("distinct.OnCompleted()"));
        subject.OnNext(1);
        subject.OnNext(2);
        subject.OnNext(3);
        subject.OnNext(1);
        subject.OnNext(1); // 这个不会再distinct里输出
        subject.OnNext(4);
        subject.OnCompleted();
    }

    private void Test16()
    {
        Observable.Return("hello")
            .Subscribe(Debug.Log);

        Observable.Return(Unit.Default)
            .Delay(TimeSpan.FromSeconds(1.0f))
            .Repeat()
            .Subscribe(_ => Debug.Log("Test16 arter 1 seconds"));

    }
    
    private void Test15()
    {
        //点击⿏标 1 秒之后输出 mouse clicked，每次点击事件都是 1 秒之后才输出
        Observable.EveryUpdate()
            .Where(_ => Input.GetMouseButtonDown(0))
            .Delay(TimeSpan.FromSeconds(1.0f))
            .Subscribe(_ => { Debug.Log("mouse clicked"); })
            .AddTo(gameObject);
 
    }
    private void Test14()
    {
        
        //点击鼠标后 1 秒内不在点击则输出，1秒内如果有点击则中重新计时 1 秒再输出
        Observable.EveryUpdate().Where(_ => Input.GetMouseButtonDown(0))
        .Throttle(TimeSpan.FromSeconds(1))
        .Subscribe(_ => Debug.Log("Test14 一秒过后"));
    }

    private void Test13()
    {
        Observable.Interval(TimeSpan.FromSeconds(1.0f))
            .Buffer(TimeSpan.FromSeconds(3.0f))
            .Subscribe(_ => { Debug.LogFormat("currentTime:{0}",
                DateTime.Now.Second); })
            .AddTo(gameObject);
    }

    private void Test12()
    {
        //点击鼠标左键之后就开始持续输出 “鼠标按过了了”
        // 条件
        var clickStream = this.UpdateAsObservable().Where(_ =>
        Input.GetMouseButtonDown(0));
        // 监听
        this.UpdateAsObservable()
        .SkipUntil(clickStream)
        .Subscribe(_ => Debug.Log("鼠标按过了了"));
    }

    private void Test11()
    {
        
        //运行之后持续输出 123，当点击鼠标左键后，停止输出 123。
        this.UpdateAsObservable() 
            .TakeUntil(Observable.EveryUpdate().Where(_=>Input.GetMouseButtonDown(0)))
            .Subscribe(_ =>
            {
                Debug.Log(123);
            });
        
        //TakeUntil 订阅并开始发射原始Observable，它还监视你提供的第二个 Observable。如果第二个
        //Observable 发射了一项数据或者发射了了一个终止通知，TakeUntil 返回的Observable会停止发射原始
        //Observable并终止。

    }

    private void Test10()
    {
        Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(seconds =>
        {
        Debug.LogFormat("当前时间:{0} s", seconds);
        }).AddTo(gameObject);
    }

    private void Test9()
    {
        //每次按下⿏标左键，则输出 mouse clicked，将 该脚本所在的 GameObject 被销毁后，点击鼠标不再输出
//        Observable.EveryUpdate()
//        .Where(_ => Input.GetMouseButtonDown(0))
//        .TakeUntilDestroy(this)
//        .Subscribe(_ => Debug.Log("mouse clicked"));
//        
//        Observable.EveryUpdate()
//        .Where(_ => Input.GetMouseButtonDown(0))
//        .TakeUntilDisable(this)
//        .Subscribe(_ => Debug.Log("mouse clicked"));


        Observable.Timer(TimeSpan.FromSeconds(1.0f))
            .RepeatUntilDisable(this) //每隔一秒输出 ticked，当把该脚本所在的 GameObject 隐藏，则停止输出
            .Subscribe(_ => Debug.Log(" Test9 ticked"))
            .AddTo(gameObject);
        
    }

    private void Test8()
    {
        //每 30 帧内的第一次点击事件输出 clicked 
        Observable.EveryUpdate()
            .Where(_ => Input.GetMouseButtonDown(0))
//        .ThrottleFirstFrame(30)
            .Subscribe(x => Debug.Log($"Test8 clicked==={x}  {Time.frameCount}"))
            .AddTo(gameObject);

    }

    private void Test7()
    {
        Debug.Log(Time.time);
        
        //开了一个线程
        Observable.Start(() =>
            {
                Thread.Sleep(TimeSpan.FromSeconds(1.0f));
                return 1;
            })
            .ObserveOnMainThread()
            .Subscribe(threadResult => Debug.LogFormat("ObserveOnMainThread ： {0} {1}",
                threadResult, Time.time));


    }
    private void Test6()
    {
        //运行结果为，每个一秒输出一次 ticked，当把 脚本所在 GameObject 删除掉，则不再输出。
        Observable.Timer(TimeSpan.FromSeconds(1.0f)) //延迟1秒
            .RepeatUntilDestroy(this)  //
            .Subscribe(_ => Debug.Log("ticked"));

    }
    
    private void Test5()
    {
        Observable.EveryUpdate()
            .SampleFrame(5)  //间隔5帧
            .Subscribe(_ => Debug.Log("SampleFrame: " + Time.frameCount));

    }

    private void Test4()
    {
        Observable.Range(0, 10)
            .ForEachAsync(number => Debug.Log("ForEachAsync: " + number))
            .Subscribe()
            .AddTo(gameObject);
    }

    private void Test3()
    {
        Observable.EveryUpdate()
        .Where(_ => Input.GetMouseButtonDown(0))
        .FrameInterval()
        .Subscribe(frameInterval => Debug.Log(frameInterval.Interval)); //会输出距离上一次鼠标点击所间隔的帧数

    }

    private void Test2()
    {
        Observable.ReturnUnit()
            .DelayFrame(10)
            .Subscribe(_ => Debug.Log("DelayFrame: " + Time.frameCount));
    }

    private void Test1()
    {
        //NextFrame 下一帧，会间隔间隔一帧，然后在第三帧打印
        Debug.Log(Time.frameCount);
        Observable.NextFrame().Subscribe(_ => Debug.Log("NextFrame " + Time.frameCount));
    }
}
