using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

public class Odin_ex25 : MonoBehaviour
{
    [TableList]
    public List<MyItem> List = new List<MyItem>()
    {
        new MyItem(),
        new MyItem(),
        new MyItem(),
    };

    [Serializable]
    public class MyItem
    {
        [PreviewField(Height = 20)]
        [TableColumnWidth(30, Resizable = false)]
        public Texture2D Icon ;

        [TableColumnWidth(60)]
        public int ID;

        public string Name;
    }
}
