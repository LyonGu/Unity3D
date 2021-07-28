using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class Example6 : MonoBehaviour
{
    // Start is called before the first frame update
    public List<Vector2> moveList=new List<Vector2>(3);
    public bool isPaused = false;
    void Start()
    {
        //从Coroutinue转换成IObservable
        //Test1();

        //从IObservable转换成协程 
        //利用流转化成协程的这个技巧，可以实现诸如在协程上等待流执行结果后继续处理这样的方法。类似于C# 中 Task 的 await .
        //Test2();
        
        
        //串联协程 
        //Test3();
        
        //同时启动多个协程，并等待执行结果
        //Test4();
        
        //将耗时的处理转移到另外一个线程上执行，并在协程上处理执行结果
        //利用Observable.Start()处理逻辑移到其它线程执行，将返回结果转回到协程中处理
        Test5();
        
        //上面的实现方式虽然没有下面的实现方式简洁，但是它详细解释了整个执行过程，以下是，上面的缩写：
//        ObservableWWW.Get("http://api.hogehoge.com/resouces/enemey.xml")
//            .SelectMany(x => Observable.Start(() => ParseXML(x)))
//            .ObserveOnMainThread()
//            .Subscribe(onNext: result =>
//                {
//                    /*对执行结果进行处理 */
//                },
//                onError: ex => Debug.LogError(ex));

    }

    private void Test5()
    {
        StartCoroutine(GetEnemyDataFromServerCoroutine());
    }
    
    Dictionary<string,EnemyParameter> ParseXML(string xml){
        return new Dictionary<string, EnemyParameter>();
    }
    struct EnemyParameter{
        public string Name { get; set; }
        public string Helth { get; set; }
        public string Power { get; set; }
    }
    private IEnumerator GetEnemyDataFromServerCoroutine()
    {
        var www=new WWW("http://api.hogehoge.com/resouces/enemey.xml");
        yield return www;
        if (!string.IsNullOrEmpty(www.error)){
            Debug.Log(www.error);
        }
        var xmlText=www.text;
        var o=Observable.Start(()=>ParseXML(xmlText)).ToYieldInstruction(); //开启一个线程

        yield return o;
        if (o.HasError){
            Debug.Log(o.Error);
            yield break;
        }
        var result=o.Result;

        Debug.Log(result);
        
        /*
         *
         * 
         */
    }

    private void Test4()
    {
        Observable.WhenAll(
            Observable.FromCoroutine<string>(o => CoroutineA1(o)),
            Observable.FromCoroutine<string>(o => CoroutineB1(o))

        ).Subscribe(xs =>
        {
            foreach (var item in xs)
            {
                Debug.Log("result:" + item);
            }
        });
    }

    IEnumerator CoroutineA1(IObserver<string> observer)
    {
        Debug.Log("CoroutineA 开始");
        yield return new WaitForSeconds(3);
        observer.OnNext("协程A 执行完成");
        Debug.Log("A 3秒等待结束");
        observer.OnCompleted();
    }
    
    IEnumerator CoroutineB1(IObserver<string> observer)
    {
        Debug.Log("CoroutineB 开始");
        yield return new WaitForSeconds(1);
        observer.OnNext("协程B 执行完成");
        Debug.Log("B 1秒等待结束");
        observer.OnCompleted();
    }

    private void Test3()
    {
        //协程转成流
        Observable.FromCoroutine(CoroutineA)
            .SelectMany(CoroutineB)
            .Subscribe(_=>Debug.Log("CoroutineA 和CoroutineB 执行完成"));
    }
    IEnumerator CoroutineA()
    {
        Debug.Log("CoroutineA 开始");
        yield return new WaitForSeconds(3);
        Debug.Log("CoroutineB 完成");
    }
    IEnumerator  CoroutineB()
    {
        Debug.Log("CoroutineB 开始");
        yield return new WaitForSeconds(1);
        Debug.Log("CoroutineB 完成");
    }

    private void Test2()
    {
        //将流转换为协程 
        /*
         * 使用 ObservableYieldInstruction ToYieldInstruction(IObservable observable)方法
            参数一： CancellationToken cancel 处理进程中断（可选）
            参数二： bool throwOnError 发生错误时，是否抛出异常
         */
        //通过使用ToYieldInstruction,你可以在协程中执行等待
        StartCoroutine(WaitCoroutine());
    }

    private IEnumerator WaitCoroutine()
    {
        Debug.Log("等待一秒钟");
        yield return Observable.Timer(TimeSpan.FromSeconds(1)).ToYieldInstruction(); //把流转成协程
        Debug.Log("按下键盘上的任意键");
        yield return this.UpdateAsObservable()
            .FirstOrDefault(_ => Input.anyKeyDown)
            .ToYieldInstruction();
        Debug.Log("好了，按下成功");
        
        //ToYieldInstruction收到OnCompleted时会终止yield return.因此，如果你不自己手动发布OnCompleted，那么流就永远不会被终止，这是非常危险的。
        //此外，如果你要使用流发出OnNext信息，可以将ToYieldInstruction的返回值存储在ObservableYieldInstruction变量中。
    }

    private void Test1()
    {
        //从Coroutinue转换成IObservable
        /*
         * 我们先介绍一种从协程转换为IObservable的方法；如果你把协程转换为流，那么你就可以将协程处理结果和UniRx的操作符连接起来。
         * 另外，在创建一个复杂行为流的时候，采用协程实现并转化为流的方式，有时，比仅仅使用UniRx操作链构建流要简单的多。
         */
        
        //******等待携程结束时将其转化为流
        /*
         * 使用Observable.FromCoroutine()方法
            返回值 IObservable
            参数一：Func coroutine
            参数二：bool publishEveryYield=false
         */
        
        //利用Observable.FromCoroutine()方法，我们可以在协程结束时，将其流化处理。当你需要在协程结束时发出通知，可以使用如下
//        Observable.FromCoroutine(NantokaCoroutine, false)
//            .Subscribe(
//                _ =>
//                {
//                    Debug.Log("OnNext");
//                },
//                () =>
//                {
//                    Debug.Log("OnCompleted");
//                }).AddTo(gameObject);
        
        //******取出 yield return 迭代的结果
        /*
         * 使用Observable.FromCoroutineValue()方法
            返回值 IObservable
            参数一：Func coroutine
            参数二：bool nullAsNextUpdate=true
         */
        
        //在协程中，我们不能向使用普通方法那样，将携程的迭代结果赋予一个变量。现在UniRx赋予我们这个能力，我们可以把每次yield的值取出来作为数据流。
        //因为Unity协程中的yield return 每次调用都会停止一帧，可以利用其在某一帧发布值(注意，是指在一帧中执行)：

//        Observable.FromCoroutineValue<Vector2>(MovePositionCoroutine)
//            .Subscribe(x => Debug.Log($"moveList x = {x}"))
//            .AddTo(gameObject);
        
        
        //******在协程内部发布OnNext
        /*
         * 使用Observable.FromCoroutine方法 *******这个重在方法协程结束的时候不会自动终止流，需要手动代用complete或者dispose****
            返回值IObservable
            参数一：Func<IObserver,IEnumerator> coroutine,第一个参数为IObserver
         */
        
//        Observable.FromCoroutine<long>(observer => MovePositionCoroutine1(observer))
//            .Subscribe(x =>
//            {
//                Debug.Log($"MovePositionCoroutine1 OnNext== {x}");
//            }, () =>
//            {
//                Debug.Log($"MovePositionCoroutine1 OnCompleted== ");
//            }).AddTo(gameObject);
        
        //******以更轻便、高效的方式执行协程 推荐这个吧
        /*
         * 使用Observable.FromMicroCoroutine/Observable.FromMicroCoroutine
            参数一：Func coroutine / Func<IObserver, IEnumerator> coroutine
            参数二：FrameCountType frameCountType=FrameCountType.Update协程的执行时机
         */
        
        //Observable.FromMicroCoroutine和Observable.FromMicroCoroutine 的行为几乎和我们之前说过的一致；
        //但是，他们的内部实现却大不相同。虽然在协程内部有yield return null的限制，但是，与Unity标准的协程相比，UniRx提供的MicroCoroutine的启动和运行是非常快速的。
        //它在UniRx中被称之为微协程。以更低的成本启动并运行协程，而不是使用Unity标准的StartCoroutine.
        Observable.FromMicroCoroutine<long>(observer => CountCoroutine(observer))
            .Subscribe(x => { Debug.Log($"Micro CountCoroutine == {x}"); })
            .AddTo(gameObject);
        
//        Observable.FromMicroCoroutine<long>(observer => MovePositionCoroutine1(observer))
//            .Subscribe(
//                x => { Debug.Log($"Micro MovePositionCoroutine1 OnNext== {x}");},
//                () => { Debug.Log($"Micro MovePositionCoroutine1 OnCompleted=====");}
//            )
//            .AddTo(gameObject);
    }

    private IEnumerator CountCoroutine(IObserver<long> observer)
    {
        long number = 0;
        while (true)
        {
            number++;
            observer.OnNext(number);
            if (number >= 10)
                yield break;
            yield return null;
        }
    }

    private IEnumerator MovePositionCoroutine1(IObserver<long> observer)
    {
        long current = 0;
        float deltaTime = 0;
        while (true)
        {
            if (!isPaused)
            {
                deltaTime+=Time.deltaTime;
                if (deltaTime>=1.0f){
                    var integePart=(int)Mathf.Floor(deltaTime);
                    current+=integePart;
                    deltaTime-=integePart;
                    observer.OnNext(current);
                    if (current >= 10)
                    {
                        observer.OnCompleted();
                        yield break; //yield break退出的时候 不会调用流的OnCompleted方法，需要手动调用
    
                    }

                }
            }
            yield return null;
        }
    }
    private IEnumerator MovePositionCoroutine()
    {
        foreach (var item in moveList)
        {
            yield return item;
        }
    }

    private IEnumerator NantokaCoroutine()
    {
        Debug.Log("协程开始");
        yield return new WaitForSeconds(3);
        //Observable.FromeCoroutine启动的协程在终止时会被自动Dispose
        Debug.Log("协程结束"); //协程结束会自动调用流的onNext和onComplete方法， 流会自动释放
    }
}
