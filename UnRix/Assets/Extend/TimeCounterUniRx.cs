using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
public class TimeCounterUniRx : MonoBehaviour
{
    public float TimeDown = 10.0f;
    private Subject<float> timerSubject = new Subject<float>();
    public IObservable<float> OnTimeChanged => timerSubject;
    void Start()
    {
        StartCoroutine(TimerCoroutine());
    }

    IEnumerator TimerCoroutine()
    {
        var time = TimeDown;
        while (time > 0.0f)
        {
            time -= Time.deltaTime;
            timerSubject.OnNext(time);
            yield return null;
        }
        timerSubject.OnCompleted();
        
    }

}
