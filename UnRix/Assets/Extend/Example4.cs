using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UniRx;
using UniRx.Triggers;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;


/*创建流的几种方式
 
使用Subject系列
使用ReactiveProperty
UniRx提供的方法
使用UniRx.Trigger系列
使用UniRx提供的协程
使用UniRx转换后的UGUI事件
 */
public class Example4 : MonoBehaviour
{
    
//    public ReactiveProperty<int> testValue = new ReactiveProperty<int>(); 泛型无法序列化
    public IntReactiveProperty testValue;

    public Button uButton;
    public InputField uInputField;
    public Slider uSlider;
    
    void Start()
    {
        Test1();
        Test2();
        Test3();
        Test4();
        Test5();
        Test6();
        Test7();
    }

    private void Test7()
    {
        /*
         * Observable.EveryUpdate 以 Time.deltatime 的时间间隔更新流。之前说 UniRx.Triggers和UpdateAsObservabel 的行为与 GameObject 相关联，当对象被 Destroy 时发布 OnCompleted。
         * 但 Observable.EvenryUpdate 的行为却不与 GameObject 的行为相关联，当 GameObject 对象被销毁时， Observable.EveryUpdate 并不会停止，除非你手动释放。
         */
        
        //ObserveEveryValueChanged
        //ObserveEveryValueChanged 在流源中是一个比较特殊的存在，它被定义为类（class）的扩展方法。
        //通过使用这个方法，你可以在每一帧中监控任何对象的参数，并创建一个变化发生时的通知流。
        
        var characterController = GetComponent<CharacterController>();
        if(characterController == null)
            return;
        characterController
            .ObserveEveryValueChanged(character => character.isGrounded) //时刻监听character.isGrounded属性
            .Where(x => x) //这个参数x其实就是 character.isGrounded
            .Subscribe(_ => Debug.Log("落地"))
            .AddTo(this.gameObject);

        Observable.EveryUpdate()
            .Select(_ => characterController.isGrounded)
            .DistinctUntilChanged()
            .Where(x => x)
            .Subscribe(_ => Debug.Log("落地"))
            .AddTo(this.gameObject);
    }

    private void Test6()
    {
        //UGUI事件转换
        uButton.OnClickAsObservable()
            .Subscribe(x => Debug.Log("点击了按钮=="));

        uInputField.OnValueChangedAsObservable().Subscribe(x => Debug.Log($"uInputField ValueChanged=={x}"));
        uInputField.OnEndEditAsObservable().Subscribe(x => Debug.Log($"uInputField ValueEnd=={x}"));
        
        uSlider.OnValueChangedAsObservable().Subscribe(x => Debug.Log($"uSlider ValueChanged=={x}"));
    }

    public bool IsPaused { get; private set; }
    private void Test5()
    {
        //协程转换 UniRx提供了一些方法，使得IObservable和Unity协程的转化变得相当容易
        //从Unity协程转化为IObservable，你可以利用Observable.Fromecoroutine来实现
        //某些情况下，与其通过操作链中复杂的操作符来构建复杂的流，不如使用协程来实现，这种方式实现更简单、容易

        Observable.FromCoroutine<int>(observer => GameTimerCoroutine(observer, 60))
            .Subscribe(t => Debug.Log($"Observable.FromCoroutine==={t}"));
    }

    private IEnumerator GameTimerCoroutine(IObserver<int> observer, int initialCount)
    {
        var current = initialCount;
        while (current > 0)
        {
            if (!IsPaused)
            {
                observer.OnNext(current--);
            }
            yield return new WaitForSeconds(1);
        }
        observer.OnNext(0);
        observer.OnCompleted();
    }

    private void Test4()
    {
        //UniRx.Trigger系列 使用Triggers将Unity的回调函数变成了一个流，那么把所有的事件处理都汇总到Awake/Start中就成了可能
        var isForceEnabled = true;
        var rigidBody = GetComponent<Rigidbody> ();
        if (rigidBody == null)
            rigidBody = this.gameObject.AddComponent<Rigidbody>();
        this.FixedUpdateAsObservable ()
            .Where (_ => isForceEnabled)
            .Subscribe (_ => rigidBody.AddForce (Vector3.up));

        this.OnTriggerEnterAsObservable ()
            .Where (x => x.gameObject.tag == "WarpZone")
            .Subscribe (_ => isForceEnabled = true);

        this.OnTriggerExitAsObservable ()
            .Where (x => x.gameObject.tag == "WarpZone")
            .Subscribe (_ => isForceEnabled = false);
    }

    private void Test3()
    {
        //UniRx的工厂方法
        /*
         * UniRX为构建流源提供了一系列的工厂方法。这时，我们可以很容易的创建一些复杂的流，仅仅通过Subject时无法实现的。
         * 如果你在Unity中使用UniRx,你可能不会很频繁的使用到UniRx提供的工厂方法，但是在某些地方，它是必须的。由于这一类方法较多，我们提取几个使用比较频繁的介绍一下
         */
        
        //Obseervable.Create是一个静态方法，你可以自由的创建一个发布值的流
        Observable.Create<int>(observer =>
        {
            Debug.Log("Start===");
            for (int i = 0; i < 100; i+=10)
            {
                observer.OnNext(i);
            }
            Debug.Log("Finish===");
            observer.OnCompleted();
            return Disposable.Create(() =>
            {
                Debug.Log("Dispose===");
            });
            
        }).Subscribe(x => Debug.Log($"Test3 x==={x}"));
        
        
        //Observable.Start  开启一个线程
        //Observable.Start是一个工厂方法，在不同的线程上运行给定的块，并且只发布一个结果值。你可以使用这个方法异步执行一些操作，然后在你希望获得结果通知时使用它们。

        Observable.Start(() =>
            {
                var req = (HttpWebRequest) WebRequest.Create("https://www.baidu.com");
                var res = (HttpWebResponse) req.GetResponse();
                using (var reader = new StreamReader(res.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            })
            .ObserveOnMainThread()  //回到主线程
            .Subscribe(x => Debug.Log($"Observable.Start x==={x}"));
        
        
        //Observable.Timer/TimeFrame  Observable.Interval/IntervalFrame
        //Observable.Timer是在一定时间后发布消息的一个工厂方法。如果使用真实时间，请使用Observable.Timer方法；如果使用Unity的帧数制定，阿么使用TimeFrame方法
//        Observable.Timer(TimeSpan.FromSeconds(5))
//            .Subscribe(_ => Debug.Log("流失了5秒====="))
//            .AddTo(gameObject);
//        
//        Observable.Timer (TimeSpan.FromSeconds (5), TimeSpan.FromSeconds (1))
//            .Subscribe (_ => Debug.Log ("5秒之后，每间隔1妙发布一次"))
//            .AddTo (gameObject);
        
        //延迟多少帧
//        Debug.Log($"当前帧数====={Time.frameCount}");
//        Observable.TimerFrame(100)
//            .Subscribe(_=>Debug.Log($"流失了100帧====={Time.frameCount}")) 
//            .AddTo(gameObject);

//        Observable.Interval(TimeSpan.FromSeconds(5))
//            .Subscribe(_ =>
//            {
//                Debug.Log($"Observable.Interval 间隔5秒");
//            }).AddTo(gameObject);
//        
//        Debug.Log($"Observable 当前帧数 ====={Time.frameCount}");
//        Observable.IntervalFrame(100)
//            .Subscribe(_ => { Debug.Log($"Observable.IntervalFrame 每隔100帧====={Time.frameCount}"); }).AddTo(gameObject);
    }

    private void Test2()
    {
        //ReacriveProperty系列
        //ReactivePropert是为一个普通变量添加一些Subject的功能（具体的实现过程也是这样的），我们可以像定义变量那样来定义和使用它

        var rp = new ReactiveProperty<int>(10);
        rp.Value = 20;
        var currentValue = rp.Value;
        rp.Subscribe(x => Debug.Log($"rp====x:{x}"));  //居然可以收到2次日志？？
        rp.Value = 30;

        testValue.Subscribe(x => Debug.Log($"testValue====x:{x}"));
        testValue.Value = 30;
        
        IntReactiveProperty playerHealyh=new IntReactiveProperty(100);
        playerHealyh.Subscribe(x=>Debug.Log($"playerHealyh====x:{x}"));
        
        //ReactiveCollection
        var collection = new ReactiveCollection<string>();
        collection.ObserveAdd()
            .Subscribe(x =>
            {
                Debug.Log(string.Format("collection Add {0}={1}", x.Index, x.Value));
            });
        
        collection.ObserveRemove ()
            .Subscribe (x => {
                Debug.Log (string.Format ("collection Remove {0}={1}", x.Index, x.Value));
            });
        
        collection.Add ("Apple");
        collection.Add ("Baseball");
        collection.Add ("Cherry");
        collection.Remove ("Apple");
        
        
        //ReactiveDictionary<T1,T2>
        var dictionary = new ReactiveDictionary<int, string>();
        dictionary.ObserveAdd()
            .Subscribe(x =>
            {
                Debug.Log(string.Format("dictionary Add {0}={1}", x.Key, x.Value));
            });
        dictionary.ObserveRemove ()
            .Subscribe (x => {
                Debug.Log (string.Format ("dictionary Remove {0}={1}", x.Key, x.Value));
            });
        
        dictionary.Add(1,"a");
        dictionary.Add(2,"b");
        dictionary.Add(3,"c");
        dictionary.Remove(3);
    }

    private void Test1()
    {
        //Subject系列
        
        /*
         *Subject 在之前已经出现过很多次了。但他是使用这个Subject系列的基础。如果你想创建一个你自己的流，并发布一些消息，你可以继续使用Subject。对应的，Subject有一些衍生的用法，它们有各自的用法；最好是依据不同的用途来选择合适的用法，下面做一个简单的说明：

            1 Subject 最基础的一项，OnNext执行后，发布对应的值。
            2 BehaviorSubject 缓存最后发布的值，执行到Subscribe时，发布当前值，也可以设置初始值
            3 缓存之前发布过的所有的值，当Subscribe时，将缓存的值汇总并发布
AsyncSubjecr 在没有执行OnNext的情况下，向内部缓存值；并在执行OnCompleted时，只发布最后一个OnNext的值。AsyncSubject和Future和Promise一样。如果你想在结果出来的时候获取它，你可以使用异步的方式来处理它。
         *
         * 
         */
        
        
    }


}
