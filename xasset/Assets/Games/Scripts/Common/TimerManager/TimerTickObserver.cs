using UnityEngine;

namespace GameTimer
{
    public class TimerTickObserver:MonoBehaviour
    {
        public int timerId;
        private void OnDestroy()
        {
            TimerManager.Instance.RemoveTickTimerTask(timerId);
        }
    }
}