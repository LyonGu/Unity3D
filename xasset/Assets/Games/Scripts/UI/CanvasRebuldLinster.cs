using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
 
public class CanvasRebuldLinster : MonoBehaviour {
#if UNITY_EDITOR
    public bool isEnable = false;
    private IList<ICanvasElement> m_LayoutRebuildQueue;
    private IList<ICanvasElement> m_GraphicRebuildQueue;

    private void Awake()
    {
        if (!isEnable)
            return;
        System.Type type = typeof(CanvasUpdateRegistry);
        FieldInfo field = type.GetField("m_LayoutRebuildQueue", BindingFlags.NonPublic | BindingFlags.Instance);
        m_LayoutRebuildQueue = (IList<ICanvasElement>)field.GetValue(CanvasUpdateRegistry.instance);
        field = type.GetField("m_GraphicRebuildQueue", BindingFlags.NonPublic | BindingFlags.Instance);
        m_GraphicRebuildQueue = (IList<ICanvasElement>)field.GetValue(CanvasUpdateRegistry.instance);
    }

    private string GetResPrefabName(Transform t)
    {
        if (t != null)
        {
            var luaBehavior = t.GetComponent<Game.LuaBehavior>();
            while (luaBehavior == null)
            {
                if (t != null)
                {
                    t = t.parent;
                    if (t != null)
                    {
                        luaBehavior = t.GetComponent<Game.LuaBehavior>();
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            if (luaBehavior != null)
                return luaBehavior.gameObject.name;
        }
        return string.Empty;
    }
    private void Update()
    {
        if (!isEnable)
            return;
        for (int j = 0; j < m_LayoutRebuildQueue.Count; j++)
        {
            var rebuild = m_LayoutRebuildQueue[j];
            
            if (ObjectValidFofUpdate(rebuild))
            {
                var rtransform = rebuild.transform;
                var prefabUIName = GetResPrefabName(rtransform);
                if(rtransform!=null)
                    Debug.LogFormat("{0} ： m_LayoutRebuildQueue中 <{1}> 引起 <{2}> 网格重建 UI资源是 <{3}>", Time.frameCount, rtransform.name, rtransform.GetComponent<Graphic>()?.canvas.name, prefabUIName);
            }
        }

        for (int j = 0; j < m_GraphicRebuildQueue.Count; j++)
        {
            var element = m_GraphicRebuildQueue[j];
           
            if (ObjectValidFofUpdate(element))
            {
                var etransform = element.transform;
                var prefabUIName = GetResPrefabName(etransform);
                if (etransform != null)
                    Debug.LogFormat("{0}：m_GraphicRebuildQueue中<{1}> 引起 <{2}> 网格重建 UI资源是 <{3}>", Time.frameCount, etransform.name, etransform.GetComponent<Graphic>()?.canvas.name, prefabUIName);
            }
        }
    }

    private bool ObjectValidFofUpdate(ICanvasElement element)
    {
        var valid = element != null;
        var isUnityObject = element is Object;
        if (isUnityObject)
            valid = (element as Object) != null; 
        return valid;
    }

    #endif
}
