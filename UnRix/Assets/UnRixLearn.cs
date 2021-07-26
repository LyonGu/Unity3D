using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UniRx;
using UniRx.Diagnostics;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Logger = UnityEngine.Logger;
using Object = UnityEngine.Object;


public class MyLoggerHandle : ILogHandler
{
    public void LogFormat(LogType logType, Object context, string format, params object[] args)
    {
        //throw new NotImplementedException();
        Debug.Log(args[0]);
    }

    public void LogException(Exception exception, Object context)
    {
        //throw new NotImplementedException();
        Debug.Log(exception);
    }
}

public class UnRixLearn : MonoBehaviour
{

    public Button UButtnon;

    public Toggle Utoggle;

    public Slider USlider;

    public InputField UInputField;

    public Text UText;
    // using UniRx.Diagnostics;

// logger is threadsafe, define per class with name.
    static readonly Logger logger = new Logger(new MyLoggerHandle());
    // call once at applicationinit
    public static void ApplicationInitialize()
    {
        // Log as Stream, UniRx.Diagnostics.ObservableLogger.Listener is IObservable<LogEntry>
        // You can subscribe and output to any place.
        ObservableLogger.Listener.LogToUnityDebug();

        // for example, filter only Exception and upload to web.
        // (make custom sink(IObserver<EventEntry>) is better to use)
//        ObservableLogger.Listener
//            .Where(x => x.LogType == LogType.Exception)
//            .Subscribe(x =>
//            {
//                // ObservableWWW.Post("", null).Subscribe();
//            });
    }
    void Start()
    {
        
//        /*网络操作*/
//        //使用ObservableWWW 进行一步网络操作。它的Get/Post函数返回可订阅的IObservables.
//        ObservableWWW.Get("http://google.co.jp/")
//            .Subscribe(
//                x => Debug.Log(x.Substring(0, 100)),
//                ex=> Debug.LogException(ex)
//            );
//        
//        //使用Observable.WhenAll 执行并行请求（parallel）:
//        var parallel=Observable.WhenAll(
//            ObservableWWW.Get("http://google.com/"),
//            ObservableWWW.Get("http://bing.com/"),
//            ObservableWWW.Get("http://unity3d.com/")
//        );
//        parallel.Subscribe(xs=>{
//            Debug.Log(xs[0].Substring(0,100));// google
//            Debug.Log(xs[1].Substring(0,100));// bing
//            Debug.Log(xs[2].Substring(0,100));// unity
//        });
//        
//        //提供进度信息：
//        //// notifier for progress use ScheduledNotifier or new Progress<float>(/* action */)
//        var progressNotifer = new ScheduledNotifier<float>();
//        progressNotifer.Subscribe(x => Debug.Log($"progress == {x}"));
//        
//        
//        //错误处理：
//        // If WWW has .error, ObservableWWW throws WWWErrorException to onError pipeline.
//        // WWWErrorException has RawErrorMessage, HasResponse, StatusCode, ResponseHeaders
//        ObservableWWW.Get("http://www.google.com/404")
//            .CatchIgnore((WWWErrorException ex) =>
//            {
//                Debug.Log(ex.RawErrorMessage);
//                if (ex.HasResponse)
//                {
//                    Debug.Log($"ERROR: {ex.StatusCode}");
//                }
//                foreach (var item in ex.ResponseHeaders)
//                {
//                    Debug.Log($"ERROR: key: {item.Key}  value:{item.Value}");
//                }
//            })
//            .Subscribe();
//        
//        
//        //使用IEnumators (Coroutines)
//        /*
//         *IEnumator(Coroutine)是Unity的基本异步工具，UniRx集成了协程和IObservables,
//         * 你可以在协程中写异步代码，并使用UniRx编排他们。这是控制异步流最好的方式
//         */
//        TestCoroutine();
//        
//        //多线程的使用
//        TestThread();
//        
//        //DefaultScheduler(默认调度器)
//        /*
//         *UniRx默认是基于时间操作的（Interval、Timer、Buffer(timeSpan)等等）,使用Scheduler.MainThread作为它们的调度器。
//         * UniRx中的大多数运算符（Observable.Start除外）都是在单个线程上执行的；因此不需要ObserverOn，并且可以忽略线程安全问题。虽然和标准 .NET 中的Rx实现不同，但是这更符合Unity的环境。
//            Scheduler.Mainthread的执行受Time.timeScale的影响，如果你想要在执行时忽略TimeScale，你可以使用Scheduler.MainThreadIgnoreTimeScale代替。
//         * 
//         */
//        
//        
//        //MonoBehaviour triggers
//        TestMonoTrigger();
//        
//        //Observable 生命周期管理
//        /*
//         *什么时候调用OnCompleted? 使用UniRx时，必须考虑订阅的生命周期管理。
//         * 当与GameObject对象相连的游戏对象被销毁时，ObservableTriggers会调用OnCompleted.
//         * 其它的静态生成器方法（Observable.Timer、Observable.EveryUpdate…等等，并不会自动停止，他们的订阅需要被手动管理
//         * Rx提供了一些辅助方法，比如，IDisposable.AddTo运行你一次释放多个订阅：
//         */
//        TestLifetWeak();
//        
//        //将Unity回调转化为IObservables(可观察对象)
//        //使用Subject(或者AsyncSubject进行异步操作)：
//        
//        //Stream Logger
//        TestLogStream();
        
        //MicroCoroutine(微协程)
        /*
         * 微协程的优点在于内存高效和执行快速。它的实现是基于Unity blog’s 10000 UPDATE() CALLS，避免了托管内存-非托管内存的开销，以致迭代速度提升了10倍。
         * 微协程自动用于基于帧数的时间运算符和ObserveEveryValueChanged.如果你想使用微协程替代Unity自带的协程（Coroutine）,使用MainThreadDispatcher.StartUpdateMicroCoroutine 或者Observable.FromMicroCoroutine.
         */
        //延迟5秒
        Observable.Timer(TimeSpan.FromSeconds(5)).Subscribe(_ => TestMicroCoroutine());
        
        
        //UGUI 集成
        //UniRx可以很容易的处理UnityEvent,使用UnityEvent.AsObservable 订阅事件：
        TestUGUI();
        
        //ReactiveProperty,ReactiveCollection
        //游戏数据通常需要通知，我们应该使用属性和事件回调吗？这样的话，简直太麻烦了，还好UniRx为我们提供了ReactiveProperty,轻量级的属性代理人

    }
    
    
// two coroutines
    IEnumerator AsyncA()
    {
        Debug.Log("a start");
        yield return new WaitForSeconds(1);
        Debug.Log("a end");
    }

    IEnumerator AsyncB()
    {
        Debug.Log("b start");
        yield return new WaitForEndOfFrame();
        Debug.Log("b end");
    }
    
    IEnumerator AsyncC()
    {
        Debug.Log("c start");
        yield return new WaitForEndOfFrame();
        Debug.Log("c end");
    }
    private void TestCoroutine()
    {
        Observable.FromCoroutine(AsyncA)
            .SelectMany(AsyncB)
            .SelectMany(AsyncC)
            .Subscribe();
    }
    private void TestThread()
    {
        var heavyMethod = Observable.Start(() =>
        {
            Debug.Log($"子线程1========延迟1秒");
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
            Debug.Log($"子线程1========延迟1秒 返回10");
            return 10;
        });
        
        var heavyMethod2 = Observable.Start(() =>
        {
            Debug.Log($"子线程1========延迟3秒");
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(3));
            Debug.Log($"子线程1========延迟3秒 返回10");
            return 10;
        });
        
        // Join and await two other thread values
        Observable.WhenAll(heavyMethod, heavyMethod2)
            .ObserveOnMainThread() // return to main thread
            .Subscribe(xs =>
            {
                // Unity can't touch GameObject from other thread
                // but use ObserveOnMainThread, you can touch GameObject naturally.
                Debug.Log($"回到主线程调用 收到子线程返回值========{xs[0]}  {xs[1]}");
            }); 
        
        
    }

    private void TestMonoTrigger()
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.AddComponent<ObservableUpdateTrigger>()
            .UpdateAsObservable()
            .SampleFrame(30)
            .Subscribe(x => Debug.Log("UpdateTrigger Cube====="), x => Debug.Log("UpdateTrigger destroy"))
            .AddTo(this);
        GameObject.Destroy(cube, 3f);
    }

    // CompositeDisposable is similar with List<IDisposable>, manage multiple IDisposable
    CompositeDisposable disposables = new CompositeDisposable(); // field
    private void TestLifetWeak()
    {
        //Observable.EveryUpdate().Subscribe(x => Debug.Log($"TestLifetWeak ==== {x}  {Time.frameCount}")).AddTo(disposables);
        
        //如果你想在GameObject被销毁时自动释放，你可以使用AddTo（GameObject/Component):
        //Observable.IntervalFrame(30).Subscribe(x=>Debug.Log($"IntervalFrame === {x}")).AddTo(this);
        
        /*
         *AddTo可以促进流的自动释放，如果你需要在管道中队OnCompleted进行特殊处理，
         * 那么你可以使用TakeWhile、TakeUntil、TakeUntilDestroy和TakeUntilDisable代替：
         */
//        Observable.IntervalFrame(30).TakeUntilDisable(this).
//            Subscribe(x => Debug.Log($"IntervalFrame30 === {x}"), () => Debug.Log("IntervalFrame30 === completed!"));


        //当你处理事件时，Repeat是一种重要但危险的方法，它可能会造成程序的无线循环，因此，请谨慎使用它：
//        this.gameObject.OnMouseDownAsObservable()
//            .SelectMany(_=>this.gameObject.UpdateAsObservable())
//            .TakeUntil(this.gameObject.OnMouseUpAsObservable())
//            .Select(_=>Input.mousePosition)
//            .Repeat()
//            .Subscribe(x=>Debug.Log(x));
        
        
        /*
         * UniRx另外提供了一种安全使用Repeat的方法。RepeatSafe:如果重复调用OnComplete,Repeat将会停止。
         * RepeatUntilDestroy(gameObject/component), RepeatUntilDisable(gameObject/component)允许在目标对象被销毁时停止。
         */
//        this.gameObject.OnMouseDownAsObservable()
//            .SelectMany(_ => this.gameObject.UpdateAsObservable())
//            .TakeUntil(this.gameObject.OnMouseUpAsObservable())
//            .Select(_ => Input.mousePosition)
//            .RepeatUntilDestroy(this) // safety way
//            .Subscribe(x => Debug.Log(x));  

        //每一个类的实例都提供了一个ObserveEveryValueChanged的方法。这个方法可以每一帧检测某个值发生的变化
        // 这里检测的是transfrom的position
        //ObserveEveryValueChanged中的参数x就是this.transform
        //Subscribe的onNext的参数为 this.transform.position
        this.transform.ObserveEveryValueChanged(x => x.position).Subscribe(x => Debug.Log(x));
    }

    private void TestIObservables()
    {
        
    }

    private void TestLogStream()
    {
        logger.Log("Logger Message");
        logger.LogException(new Exception("Logger test exception"));
    }

    
    int counter;

    IEnumerator Worker()
    {
        while(true)
        {
            counter++;
            yield return null;
        }
    }
    private void TestMicroCoroutine()
    {
        Debug.Log("TestMicroCoroutine==================");
        var watch = new Stopwatch();
        watch.Start();
        Profiler.BeginSample("Unrix MicroCoroutine");
        for(var i = 0; i < 10000; i++)
        {
            // fast, memory efficient
            MainThreadDispatcher.StartUpdateMicroCoroutine(Worker());

            // slow...
//            StartCoroutine(Worker());
        }
        Profiler.EndSample();
        watch.Stop();
        Debug.Log("Test Unrix MicroCoroutine " + watch.ElapsedMilliseconds + " ms.");
//        
//        var watch1 = new Stopwatch();
//        watch1.Start();
//        Profiler.BeginSample("Unity MicroCoroutine");
//        for(var i = 0; i < 10000; i++)
//        {
//            // fast, memory efficient
////            MainThreadDispatcher.StartUpdateMicroCoroutine(Worker());
//
////             slow...
//            StartCoroutine(Worker());
//        }
//        Profiler.EndSample();
//        watch1.Stop();
//        Debug.Log("Test Unity Coroutine " + watch1.ElapsedMilliseconds + " ms.");


/*
 *         当然微协程存在一些限制，经支持yield return null 迭代，并且其更新时间取决于启动微协程的方法（StartUpdateMicroCoroutine，StartFixedUpdateMicroCoroutine，StartEndOfFrameMicroCoroutine）。
            如果和其它IObservable结合起来，你可以检测已完成的属性，比如：isDone.
 */

        MainThreadDispatcher.StartUpdateMicroCoroutine(MicroCoroutineWithToYieldInstruction());
    }

    private void TestUGUI()
    {
        UButtnon.onClick.AsObservable().Subscribe(_ => Debug.Log("clicked"));

        Utoggle.OnValueChangedAsObservable().SubscribeToInteractable(UButtnon);

        UInputField.OnValueChangedAsObservable()
            .Where(x => x != null)  //参数x就是value值
            .Delay(TimeSpan.FromSeconds(1))
            .SubscribeToText(UText);

        USlider.OnValueChangedAsObservable()
            .SubscribeToText(UText, x => Math.Round(x, 2).ToString());
    }

    IEnumerator MicroCoroutineWithToYieldInstruction()
    {
        var www = ObservableWWW.Get("http://aaa").ToYieldInstruction();
        while (!www.IsDone)
        {
            yield return null;
        }

        if (www.HasResult)
        {
            Debug.Log($"MicroCoroutineWithToYieldInstruction Result=== {www.Result}");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // .Clear() => Dispose is called for all inner disposables, and the list is cleared.
        // .Dispose() => Dispose is called for all inner disposables, and Dispose is called immediately after additional Adds.
        disposables.Clear();
    }
}
