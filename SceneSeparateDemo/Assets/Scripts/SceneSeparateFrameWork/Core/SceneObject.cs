using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 场景物件（用来包装实际用于动态加载的物体）
/// </summary>
public class SceneObject : ISceneObject, ISOLinkedListNode
{
    /// <summary>
    /// 场景物件创建标记
    /// </summary>
    public enum CreateFlag
    {
        /// <summary>
        /// 未创建
        /// </summary>
        None,
        /// <summary>
        /// 标记为新物体
        /// </summary>
        New,
        /// <summary>
        /// 标记为旧物体
        /// </summary>
        Old,
        /// <summary>
        /// 标记为离开视野区域
        /// </summary>
        OutofBounds,
    }

    /// <summary>
    /// 场景物体加载标记
    /// </summary>
    public enum CreatingProcessFlag
    {
        None,
        /// <summary>
        /// 准备加载
        /// </summary>
        IsPrepareCreate,
        /// <summary>
        /// 准备销毁
        /// </summary>
        IsPrepareDestroy,
    }

    /// <summary>
    /// 场景物体包围盒
    /// </summary>
    public Bounds Bounds
    {
        get { return m_TargetObj.Bounds; }
    }

    //这个是用来标记什么的？？ TODO
    public float Weight
    {
        get { return m_Weight; }
        set { m_Weight = value; }
    }

    /// <summary>
    /// 被包装的实际用于动态加载和销毁的场景物体
    /// </summary>
    public ISceneObject TargetObj
    {
        get { return m_TargetObj; }
    }

    //创建物体标记
    public CreateFlag Flag { get; set; }

    //加载物体标记
    public CreatingProcessFlag ProcessFlag { get; set; }

    private ISceneObject m_TargetObj;

    private float m_Weight;

	//private System.Object m_Node;
	// 这个是存什么的呀？？TODO
	private Dictionary<uint, System.Object> m_Nodes;

    public SceneObject(ISceneObject obj)
    {
        m_Weight = 0;
        m_TargetObj = obj;
    }

	//public LinkedListNode<T> GetLinkedListNode<T>() where T : ISceneObject
	//{
	//    return (LinkedListNode<T>)m_Node;
	//}

	//public void SetLinkedListNode<T>(LinkedListNode<T> node)
	//{
	//    m_Node = node;
	//}

	public Dictionary<uint, System.Object> GetNodes()
	{
		return m_Nodes;
	}

	public LinkedListNode<T> GetLinkedListNode<T>(uint morton) where T : ISceneObject
	{
		if (m_Nodes != null && m_Nodes.ContainsKey(morton))
			return (LinkedListNode<T>)m_Nodes[morton];
		return null;
	}

	public void SetLinkedListNode<T>(uint morton, LinkedListNode<T> node)
	{
		if (m_Nodes == null)
			m_Nodes = new Dictionary<uint, object>();
		m_Nodes[morton] = node;
	}

	//隐藏物体
	public void OnHide()
    {
        Weight = 0;
        m_TargetObj.OnHide();
    }
	
	//显示物体
    public bool OnShow(Transform parent)
    {
        return m_TargetObj.OnShow(parent);
    }

#if UNITY_EDITOR
    public void DrawArea(Color color, Color hitColor)
    {
        if (Flag == CreateFlag.New || Flag == CreateFlag.Old)
        {
            m_TargetObj.Bounds.DrawBounds(hitColor);
        }
        else 
        m_TargetObj.Bounds.DrawBounds(color);
    }
#endif
}
