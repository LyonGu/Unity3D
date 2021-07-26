using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;


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
        Test1();
    }

    private void Test1()
    {
        //UniRx.Triggers中的UpdateAsObservable
        //this.UpdateAsObservable()
        
        //Update事件转换成UniRx的流来使用。当流的GameObject 被销毁时会自动触发OnCompleted,此时，流的生命周期管理变得更加容易
        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                Debug.Log("Example5 Update");
            });
    }
}
