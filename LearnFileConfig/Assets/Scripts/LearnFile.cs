using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LearnFile : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        string filePath = Utils.GetConfigFilePath("text.txt");
        var str = File.ReadAllText(filePath);
        Debug.Log(str);
    }

}
