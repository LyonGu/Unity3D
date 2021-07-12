using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

public class Odin_ex22 : SerializedMonoBehaviour
{

    [InfoBox("In order to serialize dictionaries, all we need to do is to inherit our class from SerializedMonoBehaviour.")]
    public Dictionary<int, Material> IntMaterialLookup;

    public Dictionary<string, string> StringStringDictionary;

    [DictionaryDrawerSettings(KeyLabel = "Custom Key Name", ValueLabel = "Custom Value Label")]
    public Dictionary<SomeEnum, MyCustomType> CustomLabels = new Dictionary<SomeEnum, MyCustomType>()
{
    { SomeEnum.First, new MyCustomType() },
    { SomeEnum.Second, new MyCustomType() },
};

    [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
    public Dictionary<string, List<int>> StringListDictionary = new Dictionary<string, List<int>>()
    {
        { "Numbers", new List<int>(){ 1, 2, 3, 4, } },
    };

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


    [OnInspectorInit]
    private void CreateData()
    {
        IntMaterialLookup = new Dictionary<int, Material>()
    {
        { 1, new Material(Shader.Find("Skybox/Cubemap")) },
        { 7,  new Material(Shader.Find("Skybox/Cubemap"))},
    };

        StringStringDictionary = new Dictionary<string, string>()
    {
        { "One", "Skybox/Cubemap" },
        { "Seven","Skybox/Cubemap" },
    };
    }
}
