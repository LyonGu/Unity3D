
using UnityEngine;
using UnityEngine.EventSystems;
namespace Universal.UIAnimation
{
    public class EventTriggerListener : EventTrigger
    {
        public delegate void VoidDelegate(GameObject go);
        public delegate void VoidDelegateDrag(GameObject go, PointerEventData eventData);
        public VoidDelegate onClick;
        public VoidDelegate onDown;
        public VoidDelegate onPress;
        public VoidDelegate onEnter;
        public VoidDelegate onExit;
        public VoidDelegate onUp;
        public VoidDelegate onSelect;
        public VoidDelegate onUpdateSelect;
        public VoidDelegateDrag onDrag;
        public VoidDelegate onEndDrag;
        public VoidDelegate onBeginDrag;
        public VoidDelegate onButtonDown;
        public VoidDelegate onButtonObjectDown;
        public VoidDelegate onButtonUp;
        public VoidDelegate onButtonObjectUp;
        static public EventTriggerListener Get(GameObject go)
        {

            EventTriggerListener listener = go.GetComponent<EventTriggerListener>();
            if (listener == null) listener = go.AddComponent<EventTriggerListener>();
            return listener;
        }

        static public void Des(GameObject go)
        {
            Destroy(go.GetComponent<EventTriggerListener>());
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (onClick != null) onClick(gameObject);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (onDown != null)
            {
                onDown(gameObject);
            }
            if (onButtonDown != null)
            {
                onButtonDown(gameObject);
            }
            if (onButtonObjectDown != null)
            {
                onButtonObjectDown(gameObject);
            }
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (onEnter != null) onEnter(gameObject);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (onExit != null) onExit(gameObject);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (onUp != null)
            {
                onUp(gameObject);
            }

            if (onButtonUp != null)
            {
                onButtonUp(gameObject);
            }

            if (onButtonObjectUp != null)
            {
                onButtonObjectUp(gameObject);
            }
        }

        public override void OnSelect(BaseEventData eventData)
        {
            if (onSelect != null) onSelect(gameObject);
        }

        public override void OnUpdateSelected(BaseEventData eventData)
        {
            if (onUpdateSelect != null) onUpdateSelect(gameObject);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (onDrag != null) onDrag(gameObject, eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (onEndDrag != null) onEndDrag(gameObject);
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (onBeginDrag != null) onBeginDrag(gameObject);
        }
    }
}