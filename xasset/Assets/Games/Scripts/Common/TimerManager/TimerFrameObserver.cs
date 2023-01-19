using System;
using UnityEngine;

namespace GameTimer
{
    public class TimerFrameObserver:MonoBehaviour
    {
        public int timerId;

        private void OnDestroy()
        {
            TimerManager.Instance.RemoveFrameTimerTask(timerId);
        }
    }
}