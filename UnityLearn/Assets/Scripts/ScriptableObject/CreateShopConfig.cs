using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CreateShopConfig
{
    [MenuItem("Tools/ScriptableObject/CreateShopConfig")]
    private static void Create()
    {
        CreateConfig();
    }

    private static void CreateConfig()
    {
        /*
            1 继承自ScriptableObject的类不能使用new来创建，要使用ScriptableObject.CreateInstance<T>()方法来创建；
            2 必须先创建对应的Asset文件才能打包，同时Asset文件的后缀必须是asset，否则Unity不能识别
         */
        ShopConfig shopConfig = ScriptableObject.CreateInstance<ShopConfig>();

        //填充数据, 可以从外部有策划配置好的配置表（如CSV、XML、JSON甚至是二进制文件）中通过通用代码读取所有数据来进行填充
        //这里只是测试就直接手写了(⊙﹏⊙)b

        shopConfig.ShopList = new List<ShopListInfo>();

        ShopListInfo list = new ShopListInfo();
        list.tag = ShopConfig.ShopTag.hot;
        list.list = new List<ShopItemInfo>();
        list.list.Add(new ShopItemInfo { name = "优你弟内裤", price = 10000 });
        list.list.Add(new ShopItemInfo { name = "扣扣死内裤", price = 5000 });
        list.list.Add(new ShopItemInfo { name = "内裤", price = 100 });
        shopConfig.ShopList.Add(list);

        list = new ShopListInfo();
        list.tag = ShopConfig.ShopTag.item;
        list.list = new List<ShopItemInfo>();
        list.list.Add(new ShopItemInfo { name = "金疮药", price = 250 });
        list.list.Add(new ShopItemInfo { name = "和合散", price = 500 });
        shopConfig.ShopList.Add(list);

        list = new ShopListInfo();
        list.tag = ShopConfig.ShopTag.weapon;
        list.list = new List<ShopItemInfo>();
        list.list.Add(new ShopItemInfo { name = "轩辕剑", price = 1 });
        list.list.Add(new ShopItemInfo { name = "桃木剑", price = 5 });
        list.list.Add(new ShopItemInfo { name = "小李飞刀", price = 213 });
        list.list.Add(new ShopItemInfo { name = "大李飞刀", price = 313 });
        shopConfig.ShopList.Add(list);

        //填充好数据后就可以打包到 AssetBundle 中了
        //第一步必须先创建一个保存了配置数据的 Asset 文件, 后缀必须为 asset
        AssetDatabase.CreateAsset(shopConfig, "Assets/Scripts/ScriptableObject/ShopConfig.asset");

        //第二步就可以使用 BuildPipeline 打包了
        //BuildPipeline.BuildAssetBundle(null, new[]
        //    {
        //        AssetDatabase.LoadAssetAtPath("Assets/ShopConfig.asset", typeof(ShopConfig))
        //    },
        //    Application.streamingAssetsPath + "/Config.assetbundle",
        //    BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets | BuildAssetBundleOptions.UncompressedAssetBundle,
        //    BuildTarget.StandaloneWindows64);
    }
}