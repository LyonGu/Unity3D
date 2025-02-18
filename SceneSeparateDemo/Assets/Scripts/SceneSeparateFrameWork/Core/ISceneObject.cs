﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 场景物体接口：需要插入到场景四叉树并实现动态显示与隐藏的物体实现该接口
/// </summary>
public interface ISceneObject
{
    /// <summary>
    /// 该物体的包围盒
    /// </summary>
    Bounds Bounds { get; }

    /// <summary>
    /// 该物体进入显示区域时调用（在这里处理物体的加载或显示）
    /// </summary>
    /// <param name="parent"></param>
    /// <returns></returns>
    bool OnShow(Transform parent);

    /// <summary>
    /// 该物体离开显示区域时调用（在这里处理物体的卸载或隐藏）
    /// </summary>
    void OnHide();

    
}

//这个接口类暂时没看懂 TODO
public interface ISOLinkedListNode
{

	Dictionary<uint, System.Object> GetNodes();

	LinkedListNode<T> GetLinkedListNode<T>(uint morton) where T : ISceneObject;

	//场景元素对象跟LinkedListNode做的一个映射管理
	void SetLinkedListNode<T>(uint morton, LinkedListNode<T> node);

	//void ClearLinkedListNode(int morton);
}