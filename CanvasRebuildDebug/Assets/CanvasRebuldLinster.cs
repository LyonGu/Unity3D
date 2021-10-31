using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
 
public class CanvasRebuldLinster : MonoBehaviour {
#if UNITY_EDITOR
    public bool isEnable = false;
    private IList<ICanvasElement> m_LayoutRebuildQueue;
    private IList<ICanvasElement> m_GraphicRebuildQueue;

    public Slider slider;

    private void Awake()
    {
        System.Type type = typeof(CanvasUpdateRegistry);
        FieldInfo field = type.GetField("m_LayoutRebuildQueue", BindingFlags.NonPublic | BindingFlags.Instance);
        m_LayoutRebuildQueue = (IList<ICanvasElement>)field.GetValue(CanvasUpdateRegistry.instance);
        field = type.GetField("m_GraphicRebuildQueue", BindingFlags.NonPublic | BindingFlags.Instance);
        m_GraphicRebuildQueue = (IList<ICanvasElement>)field.GetValue(CanvasUpdateRegistry.instance);
    }

    private string GetResPrefabName(Transform t)
    {
        var testScript = t.GetComponent<Test>();
        while (testScript == null)
        {
            t = t.parent;
            if(t!=null)
                testScript = t.GetComponent<Test>();
        }
        if (testScript != null)
            return testScript.gameObject.name;
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
                Debug.LogFormat("m_LayoutRebuildQueue中 <{0}> 引起 <{1}> 网格重建 UI资源是 <{2}>", rtransform.name, rtransform.GetComponent<Graphic>().canvas.name, prefabUIName);
            }
        }

        for (int j = 0; j < m_GraphicRebuildQueue.Count; j++)
        {
            var element = m_GraphicRebuildQueue[j];
           
            if (ObjectValidFofUpdate(element))
            {
                var etransform = element.transform;
                var prefabUIName = GetResPrefabName(etransform);
                Debug.LogFormat("m_GraphicRebuildQueue中<{0}> 引起 <{1}> 网格重建 UI资源是 <{2}>", etransform.name, etransform.GetComponent<Graphic>().canvas.name, prefabUIName);
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