using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class UnitSettings
{
    [Header("配表内ID")]
    public int ID;
    public string Name;
    public int HitPointLimit;
    public int Damage;
    public int MoveSpeed;
}

public class UnitInfo : MonoBehaviour
{
    public UnitSettings Settings;
}