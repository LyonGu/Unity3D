using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DownLoadTask 
{
    /*
        url
        下载类型
        保存名字
     */

    public string url = string.Empty;
    public string saveFileName = string.Empty;
    public float progress = 0.0f;

    public bool isDone = false;


}

public class DownLoadImageTask: DownLoadTask
{
    public Action<Texture2D> downHandle;
    public int texW = 100;
    public int texH = 100;

    public void ExcuteHandle(Texture2D texture2D)
    {
        downHandle?.Invoke(texture2D);
    }
}


public class DownLoadFileTask : DownLoadTask
{
    public Action<byte[]> downHandle;

    public void ExcuteHandle(byte[] bytes)
    {
        downHandle?.Invoke(bytes);
    }
}
