using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class TimeTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Subject<string> subject = new Subject<string>();
        
        //注册监听函数
        subject.Subscribe(msg => Debug.Log("Subscribe1:" + msg + "\n"));
        subject.Subscribe(msg=> Debug.Log("Subscribe2:"+msg+"\n"));
        subject.Subscribe(msg=> Debug.Log("Subscribe3:"+msg+"\n"));
        
        subject.OnNext("Hello 001"); // 注册的三个函数都会接收到
        subject.OnNext("Hello 002");
        subject.OnNext("Hello 003");
        
        
        //事件过滤，筛选 Where
        Subject<string> subject1 = new Subject<string>();
        subject1
            .Where(x=> x=="Enemy")
            .Subscribe(x => Debug.Log(string.Format("Hello : {0}", x)));
        
        subject1.OnNext("Enemy");
        subject1.OnNext("Wall");
        subject1.OnNext("Wall");
        subject1.OnNext("Enemy");
        subject1.OnNext("Weapon");

        Test1();
        Test2();
        Test3();
        Test4();
        Test5();
    }

    private void Test1()
    {
        //整数通知（发布）
        var subject = new Subject<int>();

        subject.Subscribe(x => Debug.Log(x), () => Debug.Log("subject completed"));

        subject.OnNext(1);
        subject.OnNext(2);
        subject.OnNext(3);
        subject.OnCompleted();
        
        
        //一个没有意义的值的通知（发布) Unit
        //使用了一个Unit类型的特殊类型，这种类型表示当前信息内容是没有意义的。这对于事件的发布时机来说是很重要的，OnNext()中的内容在任何情况下都可以使用
        var subject1 = new Subject<Unit>();
        subject1.Subscribe(
            onNext: x => Debug.Log(x),
            onCompleted: () => Debug.Log("subject1 onCompleted")
        );
        subject1.OnNext(Unit.Default);
    }
    private Subject<Unit> initialedSubject  = new Subject<Unit>();
    private void Test2()
    {
        StartCoroutine(GameInitialitializeCoroutine());
        //以Unit作为传递值以通知（发布）场景初始化完成
     
        initialedSubject .Subscribe(_ => { Debug.Log("场景初始化完成"); });
    }
    
    IEnumerator GameInitialitializeCoroutine () {
        /*
        一些耗时的初始化处理，自行脑补
         */
        yield return null;
        initialedSubject.OnNext (Unit.Default); //这种情况下，我们只需要发出场景初始化完成的通知，并不需要发布值，便可使用Unit来表示。
        initialedSubject.OnCompleted ();
    }

    private void Test3()
    {
        //在Subscribe接收过程中发生错误
        var stringSubject = new Subject<string>();
        stringSubject
            .Select(str => int.Parse(str))
            .Subscribe(
                onNext: v => { Debug.Log ("转换成功:" + v);}, 
                onError: ex => { Debug.Log ("转换失败： " + ex);},
                onCompleted: () => { Debug.Log ("stringSubject onCompleted===="); }
            );
        
        stringSubject.OnNext("1");
        stringSubject.OnNext("2");
        stringSubject.OnNext("100");
        stringSubject.OnNext("Hello"); //流执行过程中遇到错误，会导致OnCompleted调用不来，流终止了也被销毁了
        stringSubject.OnCompleted(); //就算调用了OnCompleted也不会执行注册方法，因为流被错误终止了也被销毁了
        
        
        //OnNext发出的字符串被Select(选择或者转换)操作符解析并打印出Int类型的流；通过OnError，就可以在处理流的过程中，发生异常时，便可知道得到异常的细节。
        //如果流收到异常之后没有被处理，那么当前流就会被终止
    }

    private void Test4()
    {
        //OnCompleted 当流完成时发出通知，并且之后不再发出通知。如果OnCompleted消息到达Subscribe,和OnError一样，该流的订阅将会被终止和销毁。
        //因此，可以向流发出OnCompleted来终止流的订阅，同样，也可以用此方法来清理流。
        
        Subject<string> stringSubject = new Subject<string>();

        stringSubject.Subscribe(
            onNext: x => Debug.Log(x),
            onCompleted: () =>
            {
                Debug.Log("OnCompleted");
            });
        
        stringSubject.OnNext("test4");
        stringSubject.OnCompleted(); //一旦调用，流就被终止销毁了
    }
    
    private void Test5()
    {
        //使用Dispose结束流的订阅
        //可以通过调用Dispose来终止订阅。这里需要注意一点，如果使用Dispose来终止流的订阅，那么OnCompleted将不会被出发。
        //所以，如果你在OnCompleted中写了停止流时的一些触发处理，那么使用Dispose释放流之后，是不会运行的。
        var subject = new Subject<int>();
        var disposable = subject.Subscribe(x => Debug.Log(x), () => Debug.Log("Test5 OnCompleted"));

        subject.OnNext(51);
        subject.OnNext(52);

        disposable.Dispose(); //结束流

        subject.OnNext(100); //后面所有的消息都收不到了

        subject.OnNext(10);

        subject.OnCompleted(); //使用Dispose来终止流的订阅，那么OnCompleted将不会被出发
        
        //只终止（释放）特定的流
        var subject1 = new Subject<int>();
        var disposable1 = subject.Subscribe(
            onNext: x => Debug.Log("Disposable 1:" + x),
            onCompleted: () => Debug.Log("OnCompleted: 1"));
        var disposable2 = subject.Subscribe(
            onNext: x => Debug.Log("Diaposable 2:" + x),
            onCompleted: () => Debug.Log("OnCompleted: 2"));

        subject1.OnNext(1);
        subject1.OnNext(2);
        //释放第一个流
        disposable1.Dispose();
        //第二个流未被释放，继续传递
        subject.OnNext(3);
        subject.OnCompleted();
    }
}
