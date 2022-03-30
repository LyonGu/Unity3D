
// 使用泛型 避免装箱


namespace HxpGame
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    #region  事件ID定义

    public enum EEvent
    {
        // 添加你自己需要的事件类型
        TestEvent = 0,
        OnPlayerNameChange=1,
        OnPlayerNameChange1=2,
    }

    #endregion



    public class GEventSystem
    {
        private static GEventSystem instance = new GEventSystem();
        public static GEventSystem Instance { get { return instance; } }

        private Dictionary<EEvent, List<Delegate>> eventDic = new Dictionary<EEvent, List<Delegate>>(500);

        private GEventSystem() { }


        #region  事件注册 RegisterEvent
        public void RegisterEvent(EEvent evt, Action callback)
        {
            InterRegisterEvent(evt, callback);
        }

        public void RegisterEvent<T1>(EEvent evt, Action<T1> callback)
        {
            InterRegisterEvent(evt, callback);
        }

        public void RegisterEvent<T1, T2>(EEvent evt, object listener, Action<T1, T2> callback)
        {
            InterRegisterEvent(evt, callback);
        }

        public void RegisterEvent<T1, T2, T3>(EEvent evt, object listener, Action<T1, T2, T3> callback)
        {
            InterRegisterEvent(evt, callback);
        }

        public void RegisterEvent<T1, T2, T3, T4>(EEvent evt, object listener, Action<T1, T2, T3, T4> callback)
        {
            InterRegisterEvent(evt, callback);
        }
        
        public void RegisterEvent<T1, T2, T3, T4, T5>(EEvent evt, object listener, Action<T1, T2, T3, T4, T5> callback)
        {
            InterRegisterEvent(evt, callback);
        }
        

        private void InterRegisterEvent(EEvent evt, Delegate callback)
        {
            if (eventDic.ContainsKey(evt))
            {
                if (eventDic[evt].IndexOf(callback) < 0)
                {
                    eventDic[evt].Add(callback);
                }
            }
            else
            {
                eventDic.Add(evt, new List<Delegate>() {callback});
            }
        }

        #endregion

        #region 事件注销UnregisterEvent

        public void UnregisterEvent(EEvent evt, Action callback)
        {
            Delegate tempDelegate = callback;
            InterUnregisterEvent(evt, tempDelegate);
        }

        public void UnregisterEvent<T1>(EEvent evt, Action<T1> callback)
        {
            Delegate tempDelegate = callback;
            InterUnregisterEvent(evt, tempDelegate);
        }

        public void UnregisterEvent<T1, T2>(EEvent evt, Action<T1, T2> callback)
        {
            Delegate tempDelegate = callback;
            InterUnregisterEvent(evt, tempDelegate);
        }

        public void UnregisterEvent<T1, T2, T3>(EEvent evt, Action<T1, T2, T3> callback)
        {
            Delegate tempDelegate = callback;
            InterUnregisterEvent(evt, tempDelegate);
        }

        public void UnregisterEvent<T1, T2, T3, T4>(EEvent evt, Action<T1, T2, T3, T4> callback)
        {
            Delegate tempDelegate = callback;
            InterUnregisterEvent(evt, tempDelegate);
        }
        
        public void UnregisterEvent<T1, T2, T3, T4, T5>(EEvent evt, Action<T1, T2, T3, T4, T5> callback)
        {
            Delegate tempDelegate = callback;
            InterUnregisterEvent(evt, tempDelegate);
        }
        
        //注销某个事件的所有监听
        public void UnregisterEventByID(EEvent evt)
        {
            if (eventDic.ContainsKey(evt))
            {
                eventDic.Remove(evt);
            }
        }
        
        //注销某个监听者所有的监听回调
        public void UnregisterEventByLister(object listener)
        {
            foreach (var eventHandle in eventDic)
            {
                var delegateList = eventHandle.Value;
                for (int i = delegateList.Count-1; i >=0; i--)
                {
                    var handle = delegateList[i];
                    if (handle.Target == listener)
                    {
                        delegateList.RemoveAt(i);
                    }
                }
            }
        }

        private void InterUnregisterEvent(EEvent evt, Delegate callback)
        {
            if (eventDic.ContainsKey(evt))
            {
                eventDic[evt].Remove(callback);
                if (eventDic[evt].Count == 0) eventDic.Remove(evt);
            }
        }
        #endregion

        #region 派发事件PostEvent
        public void PostEvent<T1, T2, T3, T4, T5>(EEvent evt, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arge5)
        {
            List<Delegate> eventList = GetEventList(evt);
            if (eventList != null)
            {
                foreach (Delegate callback in eventList)
                {
                    try
                    {
                        ((Action<T1, T2, T3, T4,T5>)callback)(arg1, arg2, arg3, arg4, arge5);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                    }
                }
            }
        }
        public void PostEvent<T1, T2, T3, T4>(EEvent evt, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            List<Delegate> eventList = GetEventList(evt);
            if (eventList != null)
            {
                foreach (Delegate callback in eventList)
                {
                    try
                    {
                        ((Action<T1, T2, T3, T4>)callback)(arg1, arg2, arg3, arg4);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                    }
                }
            }
        }

        public void PostEvent<T1, T2, T3>(EEvent evt, T1 arg1, T2 arg2, T3 arg3)
        {
            List<Delegate> eventList = GetEventList(evt);
            if (eventList != null)
            {
                foreach (Delegate callback in eventList)
                {
                    try
                    {
                        ((Action<T1, T2, T3>)callback)(arg1, arg2, arg3);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                    }
                }
            }
        }

        public void PostEvent<T1, T2>(EEvent evt, T1 arg1, T2 arg2)
        {
            List<Delegate> eventList = GetEventList(evt);
            if (eventList != null)
            {
                foreach (Delegate callback in eventList)
                {
                    try
                    {
                        ((Action<T1, T2>)callback)(arg1, arg2);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                    }
                }
            }
        }

 


        
        public void PostEvent<T>(EEvent evt, T arg)
        {
            List<Delegate> eventList = GetEventList(evt);
            if (eventList != null)
            {
                foreach (Delegate callback in eventList)
                {
                    try
                    {
                        ((Action<T>)callback)(arg);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message + ", method name : " + callback.Method);
                    }
                }
            }

        }
        
        public void PostEvent(EEvent evt)
        {
            List<Delegate> eventList = GetEventList(evt);
            if (eventList != null)
            {
                foreach (Delegate callback in eventList)
                {
                    try
                    {
                        ((Action)callback)();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// 获取所有事件
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        private List<Delegate> GetEventList(EEvent evt)
        {
            if (eventDic.ContainsKey(evt))
            {
                List<Delegate> tempList = eventDic[evt];
                if (null != tempList)
                {
                    return tempList;
                }
            }
            return null;
        }
    }
}