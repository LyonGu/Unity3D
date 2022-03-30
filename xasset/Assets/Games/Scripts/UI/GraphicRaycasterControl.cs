using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GraphicRaycasterControl : MonoBehaviour
{


    static GraphicRaycasterControl _instance;
    public static GraphicRaycasterControl instance
    {
        get
        {
            if(_instance == null)
            {
                var com = new GameObject("GraphicRaycasterControl").AddComponent<GraphicRaycasterControl>();
                _instance = com;
                DontDestroyOnLoad(_instance);
            }
            return _instance;
        }
    }

    public bool useRaycastControl = false;




    private void Start()
    {
        if(useRaycastControl)
            Sort();
        
    }
    private void LateUpdate()
    {
        
        UIGraphicRaycaster.maxSortingLayer = int.MinValue;
        UIGraphicRaycaster.maxSortingOrder = int.MinValue;
        UIGraphicRaycaster.maxEventCameraDepth = int.MinValue;
    }

    void Sort()
    {
        var allASm = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var asm in allASm)
        {
            if(true)
            {
                var tp = asm.GetType("UnityEngine.EventSystems.RaycasterManager");
                if(tp != null)
                {
                    var function = tp.GetMethod("GetRaycasters");
                    var res = function.Invoke(null, null);
                    var resList = res as List<BaseRaycaster>;
                    resList.Sort(SortRaycaster);

                    return;
                }
          
            }
        }
    }

    int SortRaycaster(BaseRaycaster x, BaseRaycaster y)
    {
        if (x is UIGraphicRaycaster && y is UIGraphicRaycaster)
        {

            return UIGraphicRaycaster.CompareUIGraphicRaycaster(x as UIGraphicRaycaster, y as UIGraphicRaycaster);
        }

        return 0;
    }
}
