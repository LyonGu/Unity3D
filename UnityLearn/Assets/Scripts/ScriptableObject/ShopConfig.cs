using System.Collections.Generic;
using UnityEngine;


//https://www.cnblogs.com/hammerc/p/4829934.html

/// <summary>
/// 商品配置表.
/// </summary>
public class ShopConfig : ScriptableObject
{
    /// <summary>
    /// 商品页签枚举.
    /// </summary>
    public enum ShopTag
    {
        hot,
        item,
        weapon
    }

    /// <summary>
    /// 商品列表.
    /// </summary>
    public List<ShopListInfo> ShopList;
}

/// <summary>
/// 指定页签的商品列表.
/// </summary>
[System.Serializable]
public class ShopListInfo
{
    /// <summary>
    /// 页签.
    /// </summary>
    public ShopConfig.ShopTag tag;

    /// <summary>
    /// 商品列表.
    /// </summary>
    public List<ShopItemInfo> list;
}


/// <summary>
/// 商品.
/// </summary>
[System.Serializable]
public class ShopItemInfo
{
    /// <summary>
    /// 名称.
    /// </summary>
    public string name;

    /// <summary>
    /// 价格.
    /// </summary>
    public int price;
}