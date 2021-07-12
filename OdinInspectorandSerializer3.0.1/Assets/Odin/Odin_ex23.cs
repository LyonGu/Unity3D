using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

public class Odin_ex23 : SerializedMonoBehaviour
{
    public Dictionary<int, int> dic1 = new Dictionary<int, int>();

    [DictionaryDrawerSettings(KeyLabel = "道具Id", ValueLabel = "道具描述")]
    public Dictionary<int, string> dic2 = new Dictionary<int, string>();

    public Dictionary<int, List<int>> dic3 = new Dictionary<int, List<int>>();


    //只要第一次管用 删除代码重新编译
    [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
    public Dictionary<string, List<int>> StringListDictionary = new Dictionary<string, List<int>>()
    {
        { "Numbers", new List<int>(){ 1, 2, 3, 4, } },
        { "Numbers2", new List<int>(){ 1, 2, 3, 4, } },
    };

    //这个显示比较好
    [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.Foldout)]
    public Dictionary<SomeEnum, MyCustomType> EnumObjectLookup = new Dictionary<SomeEnum, MyCustomType>()
    {
        { SomeEnum.Third, new MyCustomType() },
        { SomeEnum.Fourth, new MyCustomType() },
    };

    [InlineProperty(LabelWidth = 90)]
    public struct MyCustomType
    {
        public int SomeMember;
        public GameObject SomePrefab;
    }

    public enum SomeEnum
    {
        First, Second, Third, Fourth, AndSoOn
    }

    [Serializable] //为什么struct跟class不一样显示，class会显示类名
    public struct PlayerItem
    {
        public string name;
        public int age;
        public Texture2D texture2D;
        public GameObject gameObject;
        public Material material;

    }
    [DictionaryDrawerSettings(KeyLabel = "玩家Id", ValueLabel = "玩家数据", DisplayMode = DictionaryDisplayOptions.Foldout)]
    public Dictionary<string, PlayerItem> playerDic = new Dictionary<string, PlayerItem>();  //int為key就不行 傻逼

}
