using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public static class Utils 
{
#if UNITY_EDITOR
    private static string ConfigPath = Path.Combine(Application.dataPath, "Config");
#else 
    private static string ConfigPath = Path.Combine(Application.streamingAssetsPath, "Config");
#endif

    public static string GetConfigFilePath(string fileName)
    {
        string filePath = Path.Combine(ConfigPath, fileName);
        filePath = filePath.Replace("\\", "/");
        return filePath;
    }
}
