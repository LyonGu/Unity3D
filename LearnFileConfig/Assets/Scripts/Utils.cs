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

    public static string ReadConfigFile(string fileName)
    {
        string path = GetConfigFilePath(fileName);
#if UNITY_EDITOR
        string str = File.ReadAllText(path);
#else
        string str = File.ReadAllText(path);
#endif
        return str;
    }
}
