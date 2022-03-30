using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if CLIENT_RUNTIME
[XLua.BlackList]
#endif
public class UIGraphicRaycaster : GraphicRaycaster
{

    public static void EnableAllRaycastControl()
    {
#if CLIENT_RUNTIME
        GraphicRaycasterControl.instance.useRaycastControl = true;
#endif

        isRayCasterProgress = false;
    }

    public static void DissableAllRaycastControl()
    {
#if CLIENT_RUNTIME
        GraphicRaycasterControl.instance.useRaycastControl = false;
#endif

        isRayCasterProgress = true;
    }

    //大的在前面
    public static int CompareUIGraphicRaycaster(UIGraphicRaycaster x, UIGraphicRaycaster y)
    {
        var xCanvas = x.GetCanvas();
        var yCanvas = y.GetCanvas();

        var xDepth = x.eventCamera.depth;
        var yDepth = y.eventCamera.depth;
        if (xDepth != yDepth)
        {
            if (xDepth < yDepth)
            {
                return 1;
            }
        }

        var xSortLayer = SortingLayer.GetLayerValueFromID(xCanvas.sortingLayerID);
        var xSortOrder = xCanvas.sortingOrder;

        var ySortLayer = SortingLayer.GetLayerValueFromID(yCanvas.sortingLayerID);
        var ySortOrder = yCanvas.sortingOrder;

        if (xSortLayer != ySortLayer)
            return ySortLayer.CompareTo(xSortLayer);

        if (xSortOrder != ySortOrder)
            return ySortOrder.CompareTo(xSortOrder);

        return 0;

    }

    public static int maxSortingLayer = int.MinValue;
    public static int maxSortingOrder = int.MinValue;
    public static float maxEventCameraDepth = int.MinValue;



    public static bool isEnableDragRaycaster;

    public static bool isRayCasterProgress;
//    private Camera _targetCamera;
    private Canvas _canvas;
    protected override void Awake()
    {
//        _targetCamera = base.eventCamera;
        _canvas = GetComponent<Canvas>();
    }

//    public override Camera eventCamera
//    {
//        get
//        {
//            return _targetCamera;
//        }
//    }


    #region 事件点击部分
    private Canvas GetCanvas()
    {
        return _canvas;
    }

    [NonSerialized] private List<Graphic> m_RaycastResults = new List<Graphic>();

    /// <summary>
    /// Perform the raycast against the list of graphics associated with the Canvas.
    /// </summary>
    /// <param name="eventData">Current event data</param>
    /// <param name="resultAppendList">List of hit objects to append new results to.</param>
    public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
    {


        //进行拖动的过程中不再进行UI点击事件等的判定
        if (!isEnableDragRaycaster)
        {
            if (eventData.dragging) return;
        }

        var canvas = this.GetCanvas();
        if (canvas == null)
            return;

        #region 相当于提前排序
        //临时开关
        var myCameraDepth = eventCamera.depth;
        var mySortingLayer = SortingLayer.GetLayerValueFromID(canvas.sortingLayerID);
        var mySortingOrder = canvas.sortingOrder;
#if CLIENT_RUNTIME
        if (GraphicRaycasterControl.instance.useRaycastControl)
#endif
        {


            //相机的depth

            if (myCameraDepth < maxEventCameraDepth)
            {
                //Debug.LogError($"{Time.frameCount} {name} 因为相机depth return. cameraDepth:{myCameraDepth} maxEventCameraDepth:{maxEventCameraDepth} ");
                return;
            }
            //else
            //    maxEventCameraDepth = myCameraDepth;

            //canvas的 sortinglayer 和 sortingorder


            if (mySortingLayer < maxSortingLayer)
            {
                //Debug.LogError($"{Time.frameCount} {name} 因为mySortingLayer return. mySortingLayer:{mySortingLayer} maxSortingLayer:{maxSortingLayer} ");
                return;
            }
            //else
            //    maxSortingLayer = mySortingLayer;

            if (mySortingOrder < maxSortingOrder)
            {
                //Debug.LogError($"{Time.frameCount} {name} 因为mySortingOrder return. mySortingOrder:{mySortingOrder} maxSortingOrder:{maxSortingOrder} ");
                return;
            }
            //else
            //    maxSortingOrder = mySortingOrder;

        }
        #endregion

        var canvasGraphics = GraphicRegistry.GetGraphicsForCanvas(canvas);
        if (canvasGraphics == null || canvasGraphics.Count == 0)
            return;

        int displayIndex;
        var currentEventCamera = eventCamera; // Property can call Camera.main, so cache the reference

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay || currentEventCamera == null)
            displayIndex = canvas.targetDisplay;
        else
            displayIndex = currentEventCamera.targetDisplay;

        var eventPosition = Display.RelativeMouseAt(eventData.position);
        if (eventPosition != Vector3.zero)
        {
            // We support multiple display and display identification based on event position.

            int eventDisplayIndex = (int)eventPosition.z;

            // Discard events that are not part of this display so the user does not interact with multiple displays at once.
            if (eventDisplayIndex != displayIndex)
                return;
        }
        else
        {
            // The multiple display system is not supported on all platforms, when it is not supported the returned position
            // will be all zeros so when the returned index is 0 we will default to the event data to be safe.
            eventPosition = eventData.position;

            // We dont really know in which display the event occured. We will process the event assuming it occured in our display.
        }

        // Convert to view space
        Vector2 pos;
        if (currentEventCamera == null)
        {
            // Multiple display support only when not the main display. For display 0 the reported
            // resolution is always the desktops resolution since its part of the display API,
            // so we use the standard none multiple display method. (case 741751)
            float w = Screen.width;
            float h = Screen.height;
            if (displayIndex > 0 && displayIndex < Display.displays.Length)
            {
                w = Display.displays[displayIndex].systemWidth;
                h = Display.displays[displayIndex].systemHeight;
            }
            pos = new Vector2(eventPosition.x / w, eventPosition.y / h);
        }
        else
            pos = currentEventCamera.ScreenToViewportPoint(eventPosition);

        // If it's outside the camera's viewport, do nothing
        if (pos.x < 0f || pos.x > 1f || pos.y < 0f || pos.y > 1f)
            return;

        float hitDistance = float.MaxValue;

        Ray ray = new Ray();

        if (currentEventCamera != null)
            ray = currentEventCamera.ScreenPointToRay(eventPosition);

        if (canvas.renderMode != RenderMode.ScreenSpaceOverlay && blockingObjects != BlockingObjects.None)
        {
            float distanceToClipPlane = 100.0f;

            if (currentEventCamera != null)
            {
                float projectionDirection = ray.direction.z;
                distanceToClipPlane = Mathf.Approximately(0.0f, projectionDirection)
                    ? Mathf.Infinity
                    : Mathf.Abs((currentEventCamera.farClipPlane - currentEventCamera.nearClipPlane) / projectionDirection);
            }
        }

        m_RaycastResults.Clear();

        Raycast(canvas, currentEventCamera, eventPosition, canvasGraphics, m_RaycastResults);

        int totalCount = m_RaycastResults.Count;
        for (var index = 0; index < totalCount; index++)
        {
            var go = m_RaycastResults[index].gameObject;
            bool appendGraphic = true;

            if (ignoreReversedGraphics)
            {
                if (currentEventCamera == null)
                {
                    // If we dont have a camera we know that we should always be facing forward
                    var dir = go.transform.rotation * Vector3.forward;
                    appendGraphic = Vector3.Dot(Vector3.forward, dir) > 0;
                }
                else
                {
                    // If we have a camera compare the direction against the cameras forward.
                    var cameraForward = currentEventCamera.transform.rotation * Vector3.forward * currentEventCamera.nearClipPlane;
                    appendGraphic = Vector3.Dot(go.transform.position - currentEventCamera.transform.position - cameraForward, go.transform.forward) >= 0;
                }
            }

            if (appendGraphic)
            {
                float distance = 0;
                Transform trans = go.transform;
                Vector3 transForward = trans.forward;

                if (currentEventCamera == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    distance = 0;
                else
                {
                    // http://geomalgorithms.com/a06-_intersect-2.html
                    distance = (Vector3.Dot(transForward, trans.position - ray.origin) / Vector3.Dot(transForward, ray.direction));

                    // Check to see if the go is behind the camera.
                    if (distance < 0)
                        continue;
                }

                if (distance >= hitDistance)
                    continue;

                var castResult = new RaycastResult
                {
                    gameObject = go,
                    module = this,
                    distance = distance,
                    screenPosition = eventPosition,
                    displayIndex = displayIndex,
                    index = resultAppendList.Count,
                    depth = GetGraphicDepth(m_RaycastResults[index]),
                    sortingLayer = canvas.sortingLayerID,
                    sortingOrder = canvas.sortingOrder,
                    worldPosition = ray.origin + ray.direction * distance,
                    worldNormal = -transForward
                };
                resultAppendList.Add(castResult);

                maxEventCameraDepth = myCameraDepth;
                maxSortingLayer = mySortingLayer;
                maxSortingOrder = mySortingOrder;

            }
        }

        m_RaycastResults.Clear();
    }

    [NonSerialized] static readonly List<Graphic> s_SortedGraphics = new List<Graphic>();
    /// <summary>
    /// Perform a raycast into the screen and collect all graphics underneath it.
    /// </summary>
    private static void Raycast(Canvas canvas, Camera eventCamera, Vector2 pointerPosition, IList<Graphic> foundGraphics, List<Graphic> results)
    {
        int totalCount = foundGraphics.Count;
        Graphic upGraphic = null;
        int upIndex = -1;
        for (int i = 0; i < totalCount; ++i)
        {
            Graphic graphic = foundGraphics[i];
            if (!graphic.raycastTarget)
            {
                //不响应点击事件的就不要执行了
                continue;
            }
            int depth = GetGraphicDepth(graphic);
            CanvasRenderer canvasRenderer = GetGraphicCanvasRenderer(graphic);
            if (depth == -1 || canvasRenderer.cull)
                continue;

            if (!RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, pointerPosition, eventCamera))
                continue;

            if (eventCamera != null && eventCamera.WorldToScreenPoint(graphic.rectTransform.position).z > eventCamera.farClipPlane)
                continue;

            if (graphic.Raycast(pointerPosition, eventCamera))
            {
                if (!isRayCasterProgress)
                {
                    if (depth > upIndex)
                    {
                        upIndex = depth;
                        upGraphic = graphic;
                    }
                }
                else
                {
                    s_SortedGraphics.Add(graphic);
                }

            }
        }

        if (!isRayCasterProgress)
        {
            //不穿透只放入最上面那个
            if (upGraphic != null)
                results.Add(upGraphic);
        }
        else
        {
            s_SortedGraphics.Sort((g1, g2) => g2.depth.CompareTo(g1.depth));
            totalCount = s_SortedGraphics.Count;
            for (int i = 0; i < totalCount; ++i)
                results.Add(s_SortedGraphics[i]);

            s_SortedGraphics.Clear();
        }


    }
    #endregion

    /// <summary>
    /// 获取层级；
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    public static int GetGraphicDepth(Graphic graphic)
    {
        if (graphic is LatebindImage)
        {
            return (graphic as LatebindImage).depth;
        }
        return graphic.depth;
    }

    /// <summary>
    /// 获取CanvasRenderer；
    /// </summary>
    /// <param name="gra"></param>
    /// <returns></returns>
    public static CanvasRenderer GetGraphicCanvasRenderer(Graphic graphic)
    {
        if (graphic is LatebindImage)
        {
            return (graphic as LatebindImage).canvasRenderer;
        }
        return graphic.canvasRenderer;
    }
}
