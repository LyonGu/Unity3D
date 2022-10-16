using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "ImportAssetsWhiteList", menuName = "ScriptableObjects/ImportAssetsWhiteList", order = 1)]
public class ImportAssetsWhiteList : ScriptableObject
{
    public List<string> importWhiteList = new List<string>();
}
