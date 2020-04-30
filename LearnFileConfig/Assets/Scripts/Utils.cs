using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
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
        var uri = new System.Uri(path);
        var request = UnityWebRequest.Get(uri.AbsoluteUri);
        request.SendWebRequest();
        while (!request.isDone) { };
        string str = request.downloadHandler.text;
#endif
        return str;
    }
}
