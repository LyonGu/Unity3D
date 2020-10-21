using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Net;
using System;
using System.Threading.Tasks;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;

#endif


/// <summary>
/// 通过http下载资源
/// </summary>
public class HttpDownLoad
{
    //下载进度
    public float progress { get; private set; }
    //涉及子线程要注意,Unity关闭的时候子线程不会关闭，所以要有一个标识
    private bool isStop;
    //子线程负责下载，否则会阻塞主线程，Unity界面会卡主
    private Thread thread;
    //表示下载是否完成
    public bool isDone { get; private set; }
    const int ReadWriteTimeOut = 2 * 1000;//超时等待时间
    const int TimeOutWait = 5 * 1000;//超时等待时间

    const string lookUpFile = "lookUp.txt";
    private Dictionary<string, string> lookUpDic = new Dictionary<string, string>();
    private static System.Object locker = new System.Object();
    private string ConstSavePath = Path.Combine(Application.dataPath, "DownLoadTest");

    public enum DOWNLOADTYPE
    {
        FILE = 1,
        TEXTURE = 2
    }


    public void Init()
    {

        if (!Directory.Exists(ConstSavePath))
        {
            Directory.CreateDirectory(ConstSavePath);
        }
        string lookUpfilePath = ConstSavePath + "/" + lookUpFile;
        if (!File.Exists(lookUpfilePath))
        {
            File.Create(lookUpfilePath);
        }
        string[] RawString = System.IO.File.ReadAllLines(lookUpfilePath);  //路径

        for (int i = 0; i < RawString.Length; i++)     //
        {
            string[] ss = RawString[i].Split(';');     //截断字节
            lookUpDic.Add(ss[0], ss[1]); // key为url，value 为filePath

        }
    }

    /// <summary>
    /// 下载方法(断点续传)
    /// </summary>
    /// <param name="url">URL下载地址</param>
    /// <param name="savePath">Save path保存路径</param>
    /// <param name="callBack">Call back回调函数</param>
    public void DownLoad(string url, string savePath, string fileName, Action callBack, System.Threading.ThreadPriority threadPriority = System.Threading.ThreadPriority.Normal)
    {
        isStop = false;
        System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
        //开启子线程下载,使用匿名方法
        thread = new Thread(delegate ()
        {
            stopWatch.Start();
            //判断保存路径是否存在
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }


            //获取下载文件的总长度
            Debug.Log($"{url} {fileName}");
            long totalLength = GetLength(url);

            //获取文件现在的长度
            string filePath = savePath + "/" + fileName;
            FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            long fileLength = fs.Length;
            Debug.Log($"文件:{fileName} 已下载{fileLength}字节，剩余{totalLength - fileLength}字节");
            //如果没下载完
            if (fileLength < totalLength)
            {
                //断点续传核心，设置本地文件流的起始位置
                fs.Seek(fileLength, SeekOrigin.Begin);

                HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
                request.ReadWriteTimeout = ReadWriteTimeOut;
                request.Timeout = TimeOutWait;

                //断点续传核心，设置远程访问文件流的起始位置
                request.AddRange((int)fileLength);
                Stream stream = null;
                try
                {
                    stream = request.GetResponse().GetResponseStream();
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.ToString());
                }
                byte[] buffer = new byte[1024];
                //使用流读取内容到buffer中
                //注意方法返回值代表读取的实际长度,并不是buffer有多大，stream就会读进去多少
                int length = stream.Read(buffer, 0, buffer.Length);
                while (length > 0)
                {
                    //如果Unity客户端关闭，停止下载
                    if (isStop) break;
                    //将内容再写入本地文件中
                    fs.Write(buffer, 0, length);
                    //计算进度
                    fileLength += length;
                    progress = (float)fileLength / (float)totalLength;
                    //类似尾递归
                    length = stream.Read(buffer, 0, buffer.Length);

                }
                stream.Close();
                stream.Dispose();
            }
            else
            {
                progress = 1;
            }
            stopWatch.Stop();
            Debug.Log($"耗时: {stopWatch.ElapsedMilliseconds}");
            fs.Close();
            fs.Dispose();
            //如果下载完毕，执行回调
            if (progress == 1)
            {
                isDone = true;
                if (callBack != null) callBack();
                Debug.Log($"{url} download finished");
                thread.Abort();
            }

        });
        //开启子线程
        thread.IsBackground = true;
        thread.Priority = threadPriority;
        thread.Start();
    }

    public void DownLoadWithTask(string url, string savePath, string fileName, Action callBack)
    {
        isStop = false;
        System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
        var cts = new CancellationTokenSource();
        Task task = new Task(() =>
        {
            stopWatch.Start();
            //判断保存路径是否存在
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            //获取下载文件的总长度
            Debug.Log($"{url} {fileName}");
            long totalLength = GetLength(url);

            //获取文件现在的长度
            string filePath = savePath + "/" + fileName;
            FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            long fileLength = fs.Length;
            Debug.Log($"文件:{fileName} 已下载{fileLength}字节，剩余{totalLength - fileLength}字节");
            //如果没下载完
            if (fileLength < totalLength)
            {
                //断点续传核心，设置本地文件流的起始位置
                fs.Seek(fileLength, SeekOrigin.Begin);

                HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
                request.ReadWriteTimeout = ReadWriteTimeOut;
                request.Timeout = TimeOutWait;

                //断点续传核心，设置远程访问文件流的起始位置
                request.AddRange((int)fileLength);
                Stream stream = null;
                try
                {
                    stream = request.GetResponse().GetResponseStream();
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.ToString());
                }
                byte[] buffer = new byte[1024];
                //使用流读取内容到buffer中
                //注意方法返回值代表读取的实际长度,并不是buffer有多大，stream就会读进去多少
                int length = stream.Read(buffer, 0, buffer.Length);
                while (length > 0)
                {
                    //如果Unity客户端关闭，停止下载
                    if (isStop) break;
                    //将内容再写入本地文件中
                    fs.Write(buffer, 0, length);
                    //计算进度
                    fileLength += length;
                    progress = (float)fileLength / (float)totalLength;
                    //类似尾递归
                    length = stream.Read(buffer, 0, buffer.Length);

                }
                stream.Close();
                stream.Dispose();
            }
            else
            {
                progress = 1;
            }
            stopWatch.Stop();
            Debug.Log($"耗时: {stopWatch.ElapsedMilliseconds}");
            fs.Close();
            fs.Dispose();

            //如果下载完毕，执行回调
            if (progress == 1)
            {
                isDone = true;

                //lock (locker)
                //{
                //    string lookUpfilePath = savePath + "/" + lookUpFile;

                //    FileStream fs1 = new FileStream(lookUpfilePath, FileMode.Append, FileAccess.Write);
                //    string content = $"{url};{filePath} \n";

                //    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(content);
                //    fs1.Write(bytes, 0, bytes.Length);
                //    fs1.Flush();
                //    fs1.Close();
                //    fs1.Dispose();
                //}
               

                if (callBack != null) callBack();
                Debug.Log($"{url} download finished");
                cts.Cancel();

#if UNITY_EDITOR
                AssetDatabase.Refresh();

#endif
            }
        }, cts.Token);

        task.Start();
    }



//    public void DownLoadWithTaskAD(string url, Action callBack, DOWNLOADTYPE downType = DOWNLOADTYPE.TEXTURE)
//    {
//        isStop = false;
//        System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
//        var cts = new CancellationTokenSource();
//        Task task = new Task(() =>
//        {
//            stopWatch.Start();
//            //判断保存路径是否存在
//            if (!Directory.Exists(ConstSavePath))
//            {
//                Directory.CreateDirectory(ConstSavePath);
//            }



//            //获取下载文件的总长度
//            Debug.Log($"{url} {fileName}");
//            long totalLength = GetLength(url);

//            //获取文件现在的长度
//            string filePath = savePath + "/" + fileName;
//            FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
//            long fileLength = fs.Length;
//            Debug.Log($"文件:{fileName} 已下载{fileLength}字节，剩余{totalLength - fileLength}字节");
//            //如果没下载完
//            if (fileLength < totalLength)
//            {
//                //断点续传核心，设置本地文件流的起始位置
//                fs.Seek(fileLength, SeekOrigin.Begin);

//                HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
//                request.ReadWriteTimeout = ReadWriteTimeOut;
//                request.Timeout = TimeOutWait;

//                //断点续传核心，设置远程访问文件流的起始位置
//                request.AddRange((int)fileLength);
//                Stream stream = null;
//                try
//                {
//                    stream = request.GetResponse().GetResponseStream();
//                }
//                catch (Exception ex)
//                {
//                    Debug.LogError(ex.ToString());
//                }
//                byte[] buffer = new byte[1024];
//                //使用流读取内容到buffer中
//                //注意方法返回值代表读取的实际长度,并不是buffer有多大，stream就会读进去多少
//                int length = stream.Read(buffer, 0, buffer.Length);
//                while (length > 0)
//                {
//                    //如果Unity客户端关闭，停止下载
//                    if (isStop) break;
//                    //将内容再写入本地文件中
//                    fs.Write(buffer, 0, length);
//                    //计算进度
//                    fileLength += length;
//                    progress = (float)fileLength / (float)totalLength;
//                    //类似尾递归
//                    length = stream.Read(buffer, 0, buffer.Length);

//                }
//                stream.Close();
//                stream.Dispose();
//            }
//            else
//            {
//                progress = 1;
//            }
//            stopWatch.Stop();
//            Debug.Log($"耗时: {stopWatch.ElapsedMilliseconds}");
//            fs.Close();
//            fs.Dispose();

//            //如果下载完毕，执行回调
//            if (progress == 1)
//            {
//                isDone = true;

//                //lock (locker)
//                //{
//                //    string lookUpfilePath = savePath + "/" + lookUpFile;

//                //    FileStream fs1 = new FileStream(lookUpfilePath, FileMode.Append, FileAccess.Write);
//                //    string content = $"{url};{filePath} \n";

//                //    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(content);
//                //    fs1.Write(bytes, 0, bytes.Length);
//                //    fs1.Flush();
//                //    fs1.Close();
//                //    fs1.Dispose();
//                //}


//                if (callBack != null) callBack();
//                Debug.Log($"{url} download finished");
//                cts.Cancel();

//#if UNITY_EDITOR
//                AssetDatabase.Refresh();

//#endif
//            }
//        }, cts.Token);

//        task.Start();
//    }


    public void DownLoads(Dictionary<string, string> urls, /*string url,*/ string savePath, /*string fileName,*/ Action callBack, System.Threading.ThreadPriority threadPriority = System.Threading.ThreadPriority.Normal)
    {
        isStop = false;
        System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
        //开启子线程下载,使用匿名方法
        thread = new Thread(delegate ()
        {
            stopWatch.Start();
            //判断保存路径是否存在
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            int idx = 0;
            foreach (var kv in urls)
            {
                //获取下载文件的总长度
                //UnityEngine.Debug.Log(kv.k + " " + fileName);
                long totalLength = GetLength(kv.Key);

                //获取文件现在的长度
                string filePath = savePath + "/" + kv.Value;
                FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
                long fileLength = fs.Length;
                Debug.Log($"文件:{kv.Value} 已下载{fileLength}字节，剩余{totalLength - fileLength}字节");
                //如果没下载完
                if (fileLength < totalLength)
                {
                    //断点续传核心，设置本地文件流的起始位置
                    fs.Seek(fileLength, SeekOrigin.Begin);

                    HttpWebRequest request = HttpWebRequest.Create(kv.Key) as HttpWebRequest;
                    request.ReadWriteTimeout = ReadWriteTimeOut;
                    request.Timeout = TimeOutWait;

                    //断点续传核心，设置远程访问文件流的起始位置
                    request.AddRange((int)fileLength);
                    Stream stream = null;
                    try
                    {
                        stream = request.GetResponse().GetResponseStream();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex.ToString());
                    }
                    byte[] buffer = new byte[1024];
                    //使用流读取内容到buffer中
                    //注意方法返回值代表读取的实际长度,并不是buffer有多大，stream就会读进去多少
                    int length = stream.Read(buffer, 0, buffer.Length);
                    while (length > 0)
                    {
                        //如果Unity客户端关闭，停止下载
                        if (isStop) break;
                        //将内容再写入本地文件中
                        fs.Write(buffer, 0, length);
                        //计算进度
                        fileLength += length;
                        //progress = (float)fileLength / (float)totalLength;
                        //类似尾递归
                        length = stream.Read(buffer, 0, buffer.Length);

                    }
                    stream.Close();
                    stream.Dispose();
                    idx++;
                    progress = (float)idx / (float)urls.Count;
                    Debug.Log($"progress {progress}");
                }
                else
                {
                    //progress = 1;
                    idx++;
                    progress = (float)idx / (float)urls.Count;
                    Debug.Log($"progress {progress}");
                }
                stopWatch.Stop();
                Debug.Log("耗时: " + stopWatch.ElapsedMilliseconds);
                fs.Close();
                fs.Dispose();
                //如果下载完毕，执行回调
                if (progress >= 1)
                {
                    isDone = true;
                    if (callBack != null) callBack();
                    Debug.Log($"{urls.Count} download finished");
                    thread.Abort();
                }
            }
        });


        //开启子线程
        thread.IsBackground = true;
        thread.Priority = threadPriority;
        thread.Start();
    }


    /// <summary>
    /// 获取下载文件的大小
    /// </summary>
    /// <returns>The length.</returns>
    /// <param name="url">URL.</param>
    long GetLength(string url)
    {
        HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
        request.Method = "HEAD";
        request.ReadWriteTimeout = ReadWriteTimeOut;
        request.Timeout = TimeOutWait;
        HttpWebResponse response = null;
        try
        {
            response = request.GetResponse() as HttpWebResponse;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }

        return response.ContentLength;
    }

    public void Close()
    {
        isStop = true;
    }

}
