using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Timeline;


//更具实用性的转换Unity的Updaet方法上
//如何使用UniRx将Unity中的Update转化为UniRx中的Observable.
/*
 * UniRx.Triggers中的UpdateAsObservable
 * Observable 中的EveryUpdate
 */
public class Example5 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
//        Test1();
//        Test2();
        Test3();
    }

    private void Test3()
    {
        //GetKeyDown 当用户按下指定名称的按键时的那一帧返回true。
        //GetKey是可以用于持续按下
        //GetKeyUp 在用户释放给定名字的按键的那一帧返回true。
        this.UpdateAsObservable()
            .Where(_ => Input.GetKey(KeyCode.G))  
            .ThrottleFirst(TimeSpan.FromSeconds(0.25f)) //在给定条件内，只使用最前面那个
            .Subscribe(_ => Attack());
        
 
    }
    

    private void Attack()
    {
        Debug.Log("Test3 Attack");
    }

    private void Test2()
    {
        //Observable.EveryUpdate
        Observable.EveryUpdate()
            .Subscribe(
                _ => Debug.Log("Test2 Update!")
            ).AddTo(gameObject); //生命周期跟当前GameObject绑定
        
        Destroy(gameObject, 2.0f);
        /*
         * Observable.EveryUpdate()并不会自动发布OnComplete.他不像UniRx.Triggers中的 this.UpdateAsObservable() 那样，生命周期和GameObject 的生命周期有关联。
         * 所以，当你使用Observable.EvenryUpdate时你需要自己进行流体的生命周期管理。
         * 未手动释放流，就算当前GameObject被销毁，流依然执行：
         */
        
        //Observable.EveryUpdate的工作原理
        /*
         * Observable.EveryUpdate是利用UniRx提供的功能之一的微协程来运行的，其工作原理相对复杂一些。
         * 简单来说，就是当每次执行Observable.EveryUpdate时。他就会启动一个协程，除非你手动去终止这个协程，否则，协程会一直执行下去；所以说，管理好流的生命周期很重要。
         * 然而，另外一方面，使用Observable.EveryUpdate有以下好处：
                因为Observable.EveryUpdate在单例上执行，可以用与在游戏过程中一直存在的流
            大量的Subscribe不会降低性能（微协程的概念）
            
        另外MainThreadDispatcher是由一个单例创建的的GameObject.
        在使用UniRx的过程中，你可能会发现，有些东西是由UniRx生成的，这些东西对于UniRx来说也是必要的，所以不要随意删除他们。
         */
    }

    private void Test1()
    {
        //UniRx.Triggers中的UpdateAsObservable
        //this.UpdateAsObservable()
        
        //Update事件转换成UniRx的流来使用。当流的GameObject 被销毁时会自动触发OnCompleted,此时，流的生命周期管理变得更加容易
//        this.UpdateAsObservable()
//            .Subscribe(_ =>
//            {
//                Debug.Log("Example5 Update");
//            });

        this.UpdateAsObservable()
            .Subscribe(
                onNext: _ =>
                {
                    Debug.Log("Example5 Update");
                },
                onCompleted: () =>
                {
                    Debug.Log("Example5 OnCompleted");
                }
            );
        this.OnDestroyAsObservable()
            .Subscribe(_ =>
            {
                Debug.Log("Example5 OnDestroy");
            });

        Destroy(gameObject, 2.0f);
        
        /*
         * UpdateAsObservable工作原理
            UpdateAsObservable流是ObservableUpdateTrigger组件中具有实体的流。
            在调用UpdateAsObservable时，UniRx会自动触发相关GameObject的ObservableUpdateTrigger组件，利用这个组件发出相应的事件
         */
    
    }
}
