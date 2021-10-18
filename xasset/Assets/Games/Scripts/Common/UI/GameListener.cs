



namespace HxpGame
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using XLua;

    public class GameListener : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IScrollHandler, IUpdateSelectedHandler, ISelectHandler, IDeselectHandler, IMoveHandler, ISubmitHandler, ICancelHandler
    {
        public enum LuaEventTriggerType
        {
            // 引擎自带事件
            PointerEnter = EventTriggerType.PointerEnter,
            PointerExit = EventTriggerType.PointerExit,
            PointerDown = EventTriggerType.PointerDown,
            PointerUp = EventTriggerType.PointerUp,
            PointerClick = EventTriggerType.PointerClick,
            Drag = EventTriggerType.Drag,
            Drop = EventTriggerType.Drop,
            Scroll = EventTriggerType.Scroll,
            UpdateSelected = EventTriggerType.UpdateSelected,
            Select = EventTriggerType.Select,
            Deselect = EventTriggerType.Deselect,
            Move = EventTriggerType.Move,
            InitializePotentialDrag = EventTriggerType.InitializePotentialDrag,
            BeginDrag = EventTriggerType.BeginDrag,
            EndDrag = EventTriggerType.EndDrag,
            Submit = EventTriggerType.Submit,
            Cancel = EventTriggerType.Cancel,


            //自定义事件
            LongPress = EventTriggerType.Cancel + 1,

        }
        [CSharpCallLua]
        public delegate void BaseEvent(GameObject obj, BaseEventData eventData);
        [CSharpCallLua]
        public delegate void LongPressEvent(GameObject obj);

        private BaseEvent _pointEnterCallBack;
        private BaseEvent _pointExitCallBack;
        private BaseEvent _pointDownCallBack;
        private BaseEvent _pointUpCallBack;
        private BaseEvent _pointClickCallBack;
        private BaseEvent _beginDragCallBack;
        private BaseEvent _endDragCallBack;
        private BaseEvent _dragCallBack;
        private BaseEvent _dropCallBack;
        private BaseEvent _scollCallBack;
        private BaseEvent _moveCallBack;
        private BaseEvent _cancleCallBack;
        private BaseEvent _longPressCallBack;


        //private float countTime;
        private bool isStart = false;
        private float lastTime = 0;

        private float countTime = 2.0f;
        public float CountTime { get => this.countTime; set => this.countTime = value; }


        //默认穿透层数
        private int throughCount = 5;
        public int ThroughCount { get => throughCount; set => throughCount = value; }

        private int EventCount = (int)LuaEventTriggerType.LongPress;
        private List<bool> blockList = new List<bool>();
        
        void Awake()
        {
            InitBlockList();

        }


        void InitBlockList()
        {
            blockList.Clear();
            for (int i = 0; i <= EventCount; i++)
            {
                blockList.Add(true);
            }

            //拖动事件默认传递
            blockList[(int)LuaEventTriggerType.BeginDrag] = false;
            blockList[(int)LuaEventTriggerType.EndDrag] = false;
            blockList[(int)LuaEventTriggerType.Drag] = false;
        }
        
        public void AddEvent(LuaEventTriggerType luaEventType, BaseEvent call)
        {
            if (call == null)
                return;
            switch (luaEventType)
            {
                case LuaEventTriggerType.PointerEnter:
                    AddPointEnterEvent(call);
                    break;
                case LuaEventTriggerType.PointerExit:
                    AddPointExitEvent(call);
                    break;
                case LuaEventTriggerType.PointerDown:
                    AddPointDownEvent(call);
                    break;
                case LuaEventTriggerType.PointerUp:
                    AddPointUpEvent(call);
                    break;
                case LuaEventTriggerType.PointerClick:
                    AddPointClickEvent(call);
                    break;
                case LuaEventTriggerType.Drag:
                    AddDragEvent(call);
                    break;
                case LuaEventTriggerType.Drop:
                    AddDropEvent(call);
                    break;
                case LuaEventTriggerType.Scroll:
                    AddScrollEvent(call);
                    break;
                case LuaEventTriggerType.UpdateSelected:
                    break;
                case LuaEventTriggerType.Select:
                    break;
                case LuaEventTriggerType.Deselect:
                    break;
                case LuaEventTriggerType.Move:
                    AddMoveEvent(call);
                    break;
                case LuaEventTriggerType.InitializePotentialDrag:
                    break;
                case LuaEventTriggerType.BeginDrag:
                    AddBeginDragEvent(call);
                    break;
                case LuaEventTriggerType.EndDrag:
                    AddEndDragEvent(call);
                    break;
                case LuaEventTriggerType.Submit:
                    break;
                case LuaEventTriggerType.Cancel:
                    AddCancleEvent(call);
                    break;
                case LuaEventTriggerType.LongPress:
                    AddLongPressEvent(call);
                    break;
                default:
                    break;
            }
        }

        public void RemoveEvent(LuaEventTriggerType luaEventType)
        {
            switch (luaEventType)
            {
                case LuaEventTriggerType.PointerEnter:
                    RemovePointEnterEvent();
                    break;
                case LuaEventTriggerType.PointerExit:
                    RemovePointExitEvent();
                    break;
                case LuaEventTriggerType.PointerDown:
                    RemovePointDownEvent();
                    break;
                case LuaEventTriggerType.PointerUp:
                    RemovePointUpEvent();
                    break;
                case LuaEventTriggerType.PointerClick:
                    RemovePointClickEvent();
                    break;
                case LuaEventTriggerType.Drag:
                    RemoveDragEvent();
                    break;
                case LuaEventTriggerType.Drop:
                    RemoveDropEvent();
                    break;
                case LuaEventTriggerType.Scroll:
                    RemoveScrollEvent();
                    break;
                case LuaEventTriggerType.UpdateSelected:
                    break;
                case LuaEventTriggerType.Select:
                    break;
                case LuaEventTriggerType.Deselect:
                    break;
                case LuaEventTriggerType.Move:
                    RemoveMoveEvent();
                    break;
                case LuaEventTriggerType.InitializePotentialDrag:
                    break;
                case LuaEventTriggerType.BeginDrag:
                    RemoveBeginDragEvent();
                    break;
                case LuaEventTriggerType.EndDrag:
                    RemoveEndDragEvent();
                    break;
                case LuaEventTriggerType.Submit:
                    break;
                case LuaEventTriggerType.Cancel:
                    RemoveCancleEvent();
                    break;
                case LuaEventTriggerType.LongPress:
                    RemoveLongPressEvent();
                    break;
                default:
                    break;
            }
        }

        public void RemoveAllEvent()
        {

            RemovePointEnterEvent();
            RemovePointExitEvent();
            RemovePointExitEvent();
            RemovePointDownEvent();
            RemovePointUpEvent();
            RemovePointClickEvent();
            RemoveDragEvent();
            RemoveDropEvent();
            RemoveScrollEvent();
            RemoveMoveEvent();
            RemoveBeginDragEvent();
            RemoveCancleEvent();
            RemoveLongPressEvent();
        }
            //设置对应事件是否向下传递
        public void SetBlockEvent(LuaEventTriggerType luaEventType, bool isBlock)
        {
            if (blockList.Count == 0)
            {
                InitBlockList();
            }
            blockList[(int)luaEventType] = isBlock;
        }

        void AddPointEnterEvent(BaseEvent eve)
        {
            _pointEnterCallBack = eve;
        }
        void RemovePointEnterEvent()
        {
            _pointEnterCallBack = null;
        }

        void AddPointExitEvent(BaseEvent eve)
        {
            _pointExitCallBack = eve;
        }
        void RemovePointExitEvent()
        {
            _pointExitCallBack = null;
        }

        void AddPointDownEvent(BaseEvent eve)
        {

            _pointDownCallBack = eve;
        }
        void RemovePointDownEvent()
        {
            _pointDownCallBack = null;
        }

        void AddPointUpEvent(BaseEvent eve)
        {
            _pointUpCallBack = eve;
        }
        void RemovePointUpEvent()
        {
            _pointUpCallBack = null;
        }

        void AddPointClickEvent(BaseEvent eve)
        {
            _pointClickCallBack = eve;
        }
        void RemovePointClickEvent()
        {
            _pointClickCallBack = null;
        }

        void AddBeginDragEvent(BaseEvent eve)
        {
            _beginDragCallBack = eve;
        }
        void RemoveBeginDragEvent()
        {
            _beginDragCallBack = null;
        }

        void AddDragEvent(BaseEvent eve)
        {
            _dragCallBack = eve;
        }
        void RemoveDragEvent()
        {
            _dragCallBack = null;
        }

        void AddEndDragEvent(BaseEvent eve)
        {
            _endDragCallBack = eve;
        }
        void RemoveEndDragEvent()
        {
            _endDragCallBack = null;
        }
        void AddDropEvent(BaseEvent eve)
        {
            _dropCallBack = eve;
        }
        void RemoveDropEvent()
        {
            _dropCallBack = null;
        }

        void AddScrollEvent(BaseEvent eve)
        {
            _scollCallBack = eve;
        }
        void RemoveScrollEvent()
        {
            _scollCallBack = null;
        }


        void AddMoveEvent(BaseEvent eve)
        {
            _moveCallBack = eve;
        }
        void RemoveMoveEvent()
        {
            _moveCallBack = null;
        }


        void AddCancleEvent(BaseEvent eve)
        {
            _cancleCallBack = eve;
        }
        void RemoveCancleEvent()
        {
            _cancleCallBack = null;
        }


        void AddLongPressEvent(BaseEvent eve)
        {
            _longPressCallBack = eve;
        }
        void RemoveLongPressEvent()
        {
            _longPressCallBack = null;
        }

        [BlackList]
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_pointEnterCallBack != null)
            {
                bool isBlock = blockList[(int)LuaEventTriggerType.PointerEnter];
                _pointEnterCallBack(this.gameObject, eventData);
                PassEvent(eventData, ExecuteEvents.pointerEnterHandler, LuaEventTriggerType.PointerEnter, false, isBlock);
            }
        }
        [BlackList]
        public void OnPointerExit(PointerEventData eventData)
        {
            if (_pointExitCallBack != null)
            {
                bool isBlock = blockList[(int)LuaEventTriggerType.PointerExit];
                _pointExitCallBack(this.gameObject, eventData);
                PassEvent(eventData, ExecuteEvents.pointerExitHandler, LuaEventTriggerType.PointerExit, false, isBlock);
            }
        }
        [BlackList]
        public void OnPointerDown(PointerEventData eventData)
        {
            LongPress(true);
            if (_pointDownCallBack != null)
            {
                bool isBlock = blockList[(int)LuaEventTriggerType.PointerDown];
                _pointDownCallBack(this.gameObject, eventData);
                PassEvent(eventData, ExecuteEvents.pointerDownHandler, LuaEventTriggerType.PointerDown, false, isBlock);
            }

        }

        [BlackList]
        public void OnPointerUp(PointerEventData eventData)
        {
            if (isStart)
            {
                LongPress(false);
            }
            if (_pointUpCallBack != null)
            {
                bool isBlock = blockList[(int)LuaEventTriggerType.PointerUp];
                _pointUpCallBack(this.gameObject, eventData);
                PassEvent(eventData, ExecuteEvents.pointerUpHandler, LuaEventTriggerType.PointerUp, false, isBlock);
            }
        }
        [BlackList]
        public void OnInitializePotentialDrag(PointerEventData eventData)
        {

        }
        [BlackList]
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_beginDragCallBack != null)
            {
                _beginDragCallBack(this.gameObject, eventData);
            }
            PassEvent(eventData, ExecuteEvents.beginDragHandler, LuaEventTriggerType.BeginDrag, true);

        }
        [BlackList]
        public void OnDrag(PointerEventData eventData)
        {
            if (_dragCallBack != null)
            {
                _dragCallBack(this.gameObject, eventData);
            }
            PassEvent(eventData, ExecuteEvents.dragHandler, LuaEventTriggerType.Drag, true);
        }
        [BlackList]
        public void OnCancel(BaseEventData eventData)
        {
            if (_cancleCallBack != null)
            {
                _cancleCallBack(this.gameObject, eventData);
            }
        }
        [BlackList]
        public void OnEndDrag(PointerEventData eventData)
        {
            if (_endDragCallBack != null)
            {
                _endDragCallBack(this.gameObject, eventData);
            }
            PassEvent(eventData, ExecuteEvents.endDragHandler, LuaEventTriggerType.EndDrag, true);

        }
        [BlackList]
        public void OnDrop(PointerEventData eventData)
        {
            if (_dropCallBack != null)
            {
                bool isBlock = blockList[(int)LuaEventTriggerType.Drop];
                _dropCallBack(this.gameObject, eventData);
                PassEvent(eventData, ExecuteEvents.dropHandler, LuaEventTriggerType.Drop, false, isBlock);
            }

        }
        [BlackList]
        public void OnScroll(PointerEventData eventData)
        {
            if (_scollCallBack != null)
            {
                bool isBlock = blockList[(int)LuaEventTriggerType.Scroll];
                _scollCallBack(this.gameObject, eventData);
                PassEvent(eventData, ExecuteEvents.scrollHandler, LuaEventTriggerType.Scroll, false, isBlock);
            }


        }
        [BlackList]
        public void OnUpdateSelected(BaseEventData eventData)
        {
            // throw new System.NotImplementedException();

        }
        [BlackList]
        public void OnSelect(BaseEventData eventData)
        {
            // throw new System.NotImplementedException();
        }
        [BlackList]
        public void OnDeselect(BaseEventData eventData)
        {
            // throw new System.NotImplementedException();
        }
        [BlackList]
        public void OnMove(AxisEventData eventData)
        {
            if (_moveCallBack != null)
            {
                _moveCallBack(this.gameObject, eventData);
            }
        }
        [BlackList]
        public void OnSubmit(BaseEventData eventData)
        {
            // throw new System.NotImplementedException();
        }
        [BlackList]
        public void OnPointerClick(PointerEventData eventData)
        {
            if (_pointClickCallBack != null)
            {
                bool isBlock = blockList[(int)LuaEventTriggerType.PointerClick];
                _pointClickCallBack(this.gameObject, eventData);
                PassEvent(eventData, ExecuteEvents.pointerClickHandler, LuaEventTriggerType.PointerClick, false, isBlock);
            }

        }

        void Update()
        {
            if (isStart && Time.time - lastTime > CountTime)
            {
                isStart = false;

                if (_longPressCallBack != null)
                {
                    _longPressCallBack(this.gameObject, null);
                }
            }
        }

        void LongPress(bool bStart)
        {
            isStart = bStart;
            lastTime = Time.time;
        }

        static bool isProcessing = false;
        //把事件透下去
        [BlackList]
        public void PassEvent<T>(PointerEventData data, ExecuteEvents.EventFunction<T> function, LuaEventTriggerType luaEventType, bool isForce = false, bool isBlock = true)
            where T : IEventSystemHandler
        {

            if (isProcessing)
                return;
            if (isBlock)
            {
                return;
            }
            isProcessing = true;
            try
            {
                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(data, results);
                GameObject current = data.pointerCurrentRaycast.gameObject;
                if (current != null)
                {
                    int curid = current.GetInstanceID();
                    int ghCount = 0;
                    for (int i = 0; i < results.Count; i++)
                    {
                        GameObject obj = results[i].gameObject;
                        int id = obj.GetInstanceID();
                        if (curid != id)
                        {
                            ghCount++;
                            //bool isCan = ExecuteEvents.CanHandleEvent<T>(obj);
                            //if (isCan)
                            {
                                {
                                    //Debug.Log("obj==="+ obj.name + "   event:" + luaEventType);
                                    ExecuteEvents.ExecuteHierarchy(obj, data, function);
                                }
                                if (isForce)
                                {
                                    //拖动事件直接返回
                                    break;
                                }
                                if (!isBlock)
                                {
                                    if (ghCount >= ThroughCount)
                                    {
                                        //非阻挡 限制传递层数
                                        break;
                                    }
                                }
                            }
                            //RaycastAll后ugui会自己排序，如果只想响应透下去的最近的一个响应，ExecuteEvents.Execute后直接break就行。
                        }
                    }
                }
            }
            finally
            {
                isProcessing = false;
            }

        }



        private void OnDestroy()
        {
            _pointEnterCallBack = null;
            _pointExitCallBack = null;
            _pointDownCallBack = null;
            _pointUpCallBack = null;
            _pointClickCallBack = null;
            _beginDragCallBack = null;
            _endDragCallBack = null;
            _dragCallBack = null;
            _dropCallBack = null;
            _scollCallBack = null;
            _moveCallBack = null;
            _cancleCallBack = null;
            _longPressCallBack = null;

            blockList.Clear();
            blockList = null;
        }
    }
}


