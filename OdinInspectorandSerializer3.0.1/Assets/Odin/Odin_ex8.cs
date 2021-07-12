using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

public class Odin_ex8 : MonoBehaviour
{
    [PropertyOrder(1)]
    public int Second;

    [InfoBox("PropertyOrder is used to change the order of properties in the inspector.")]
    [PropertyOrder(-1)]  //显示排序，越小的越显示靠前
    public int First;  
}
