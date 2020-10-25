using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DownLoadTest : MonoBehaviour
{
    private HttpDownLoad httpDownLoad;
    private System.Object loomParms = new System.Object();

    public GameObject ImageObj;
    //public RawImage RawImageCom1;
    //public RawImage RawImageCom2;
    //public RawImage RawImageCom3;

    public List<string> urlList = new List<string> ();

    private DownLoadManager downLoadManager;
    public List<RawImage> rawImageList = new List<RawImage>();


    void Start()
    {
        //httpDownLoad = new HttpDownLoad();
        //httpDownLoad.Init();
        //string url = "https://timgsa.baidu.com/timg?image&quality=80&size=b9999_10000&sec=1603262053359&di=aaefa8e8a14e968682aa9dd3b9882e81&imgtype=0&src=http%3A%2F%2Ft7.baidu.com%2Fit%2Fu%3D378254553%2C3884800361%26fm%3D79%26app%3D86%26f%3DJPEG%3Fw%3D1280%26h%3D2030";
        //string savePath = Path.Combine(Application.dataPath, "DownLoadTest");
        //string saveFileName = "TestDownLoadOne.jpg";
        //httpDownLoad.DownLoad(url, savePath, saveFileName, ()=> {
        //    Debug.Log($"HttpDownLoad {saveFileName}================");



        //    //这个回调时在子线程，不能操作相关东西
        //    //ImageObj.SetActive(false);
        //});

        //url = "https://timgsa.baidu.com/timg?image&quality=80&size=b9999_10000&sec=1603263696553&di=c74c035d556e94d2514d4479cf7e6087&imgtype=0&src=http%3A%2F%2Ft8.baidu.com%2Fit%2Fu%3D3571592872%2C3353494284%26fm%3D79%26app%3D86%26f%3DJPEG%3Fw%3D1200%26h%3D1290";
        //saveFileName = "TestDownLoadTwo.jpg";
        //httpDownLoad.DownLoadWithTask(url, savePath, saveFileName, () =>
        //{
        //    Debug.Log($"HttpDownLoad {saveFileName}================");

        //});

        //url = "https://ss1.bdstatic.com/70cFvXSh_Q1YnxGkpoWK1HF6hhy/it/u=1851162063,3540103028&fm=26&gp=0.jpg";
        //saveFileName = "TestDownLoadThree.jpg";
        //httpDownLoad.DownLoadWithTask(url, savePath, saveFileName, () =>
        //{
        //    Debug.Log($"HttpDownLoad {saveFileName}================");
        //});

        //DownloadHandlerAssetBundle t;

        //错误实例 UnityWebRequest不能放到子线程中
        //string filePath = savePath + "/" + saveFileName;
        //DownLoadFileUseUnityWebRequest(url, filePath, (UnityWebRequest t) =>
        //{
        //    Debug.Log("DownLoadFileUseUnityWebRequest DownLoad Over");
        //    var File = t.downloadHandler.data;

        //    //创建文件写入对象
        //    FileStream nFile = new FileStream(filePath, FileMode.Create);

        //    //写入数据
        //    nFile.Write(File, 0, File.Length);
        //    nFile.Close();
        //});


        //Unity 多线程和主线程的交互，使用Loom
        //https://www.cnblogs.com/lancidie/p/5877696.html
        //Loom.Initialize();

        //url = "https://timgsa.baidu.com/timg?image&quality=80&size=b9999_10000&sec=1603263696553&di=c74c035d556e94d2514d4479cf7e6087&imgtype=0&src=http%3A%2F%2Ft8.baidu.com%2Fit%2Fu%3D3571592872%2C3353494284%26fm%3D79%26app%3D86%26f%3DJPEG%3Fw%3D1200%26h%3D1290";
        //saveFileName = "TestDownLoad2.jpg";
        //httpDownLoad.DownLoadWithTask(url, savePath, saveFileName, () =>
        //{
        //    Loom.QueueOnMainThread((System.Object t) =>
        //    {
        //        ImageObj.SetActive(false);
        //    }, loomParms);

        //});

        ////直接使用Loom开启多线程

        //string urlAd1 = "https://timgsa.baidu.com/timg?image&quality=80&size=b9999_10000&sec=1603262053359&di=aaefa8e8a14e968682aa9dd3b9882e81&imgtype=0&src=http%3A%2F%2Ft7.baidu.com%2Fit%2Fu%3D378254553%2C3884800361%26fm%3D79%26app%3D86%26f%3DJPEG%3Fw%3D1280%26h%3D2030";
        //httpDownLoad.DownLoadTextureWithTaskAD(urlAd1, (Texture2D tex) =>
        //{
        //    Debug.Log($"主线程回调1================");
        //});

        //urlAd1 = "https://timgsa.baidu.com/timg?image&quality=80&size=b9999_10000&sec=1603263696553&di=c74c035d556e94d2514d4479cf7e6087&imgtype=0&src=http%3A%2F%2Ft8.baidu.com%2Fit%2Fu%3D3571592872%2C3353494284%26fm%3D79%26app%3D86%26f%3DJPEG%3Fw%3D1200%26h%3D1290";
        //httpDownLoad.DownLoadTextureWithTaskAD(urlAd1, (Texture2D tex) =>
        //{
        //    Debug.Log($"主线程回调2================");
        //});

        //urlAd1 = "https://ss1.bdstatic.com/70cFvXSh_Q1YnxGkpoWK1HF6hhy/it/u=1851162063,3540103028&fm=26&gp=0.jpg";
        //httpDownLoad.DownLoadTextureWithTaskAD(urlAd1, (Texture2D tex) =>
        //{
        //    Debug.Log($"主线程回调3================");
        //    RawImageCom.texture = tex;
        //});


        //测试DownLoadManage接口

        downLoadManager = DownLoadManager.GetInstance();

        //DownLoadImageTask downLoadImageTask1 = new DownLoadImageTask();
        //downLoadImageTask1.url = "https://timgsa.baidu.com/timg?image&quality=80&size=b9999_10000&sec=1603262053359&di=aaefa8e8a14e968682aa9dd3b9882e81&imgtype=0&src=http%3A%2F%2Ft7.baidu.com%2Fit%2Fu%3D378254553%2C3884800361%26fm%3D79%26app%3D86%26f%3DJPEG%3Fw%3D1280%26h%3D2030";
        //downLoadImageTask1.downHandle = DownLoadDone;
        //downLoadManager.DownLoadTextureWithTaskAD(downLoadImageTask1);


        //DownLoadImageTask downLoadImageTask2 = new DownLoadImageTask();
        //downLoadImageTask2.url = "https://timgsa.baidu.com/timg?image&quality=80&size=b9999_10000&sec=1603263696553&di=c74c035d556e94d2514d4479cf7e6087&imgtype=0&src=http%3A%2F%2Ft8.baidu.com%2Fit%2Fu%3D3571592872%2C3353494284%26fm%3D79%26app%3D86%26f%3DJPEG%3Fw%3D1200%26h%3D1290";
        //downLoadImageTask2.downHandle = (Texture2D t) =>
        //{
        //    Debug.Log($"主线程回调1================");
        //    rawImageList[1].texture = t;
        //};
        //downLoadManager.DownLoadTextureWithTaskAD(downLoadImageTask2);

        //DownLoadImageTask downLoadImageTask3 = new DownLoadImageTask();
        //downLoadImageTask3.url = "https://ss1.bdstatic.com/70cFvXSh_Q1YnxGkpoWK1HF6hhy/it/u=1851162063,3540103028&fm=26&gp=0.jpg";
        //downLoadImageTask3.downHandle = (Texture2D t) =>
        //{
        //    Debug.Log($"主线程回调2================");
        //    rawImageList[2].texture = t;
        //};
        //downLoadManager.DownLoadTextureWithTaskAD(downLoadImageTask3);


        System.GC.Collect();
        List<DownLoadImageTask> downTaskList = new List<DownLoadImageTask>();
        int count = urlList.Count;
        for (int i = 0; i < urlList.Count; i++)
        {
            int index = i;
            DownLoadImageTask downLoadImageTask = new DownLoadImageTask();
            downLoadImageTask.url = urlList[i];

            downLoadImageTask.downHandle = (Texture2D tex) =>
            {
                Debug.Log($"主线程回调{index}================");
                rawImageList[index].texture = tex;
            };
            downTaskList.Add(downLoadImageTask);
            //downLoadManager.DownLoadTextureWithTaskAD(downLoadImageTask);
        }

        downLoadManager.DownLoadTextureWithTaskADs(downTaskList, () =>
        {
            Debug.Log($"所有下载任务执行完成================");
        });

        //DownLoadFileTask downLoadFileTask = new DownLoadFileTask();
        //downLoadFileTask.url = "https://timgsa.baidu.com/timg?image&quality=80&size=b9999_10000&sec=1603373024024&di=73f4f9b862472df1c73688bc529ff08f&imgtype=0&src=http%3A%2F%2Ft8.baidu.com%2Fit%2Fu%3D1484500186%2C1503043093%26fm%3D79%26app%3D86%26f%3DJPEG%3Fw%3D1280%26h%3D853";
        //downLoadFileTask.downHandle = (byte[] bytes) =>
        //{
        //    Debug.Log("主线程 downLoadFileTask======");
        //};
        //downLoadManager.DownLoadFileWithTaskAD(downLoadFileTask);


        //全部用主线程下 测试是否是因为多线程引起的
        //var ConstSavePath = Path.Combine(Application.dataPath, "DownLoadTest"); //TODO 需要修改成Application.persistentDataPath
        //ConstSavePath = ConstSavePath.Replace(@"\", @"/");
        //for (int i = 0; i < urlList.Count; i++)
        //{

        //    //if (i != 1)
        //    //    continue;
        //    int index = i;
        //    var url = urlList[i];
        //    var totalLength = downLoadManager.GetLength(url);
        //    if (totalLength < 0)
        //    {
        //        Debug.LogError($"totalLength = -1 =====url == {url}");
        //        continue;
        //    }

        //    string fileName = downLoadManager.GetMD5(url);
        //    string filePath = $"{ConstSavePath}/{fileName}.png";
        //    //获取文件现在的长度
        //    FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        //    long fileLength = fs.Length;
        //    if (fileLength < totalLength)
        //    {

        //        //断点续传核心，设置本地文件流的起始位置
        //        fs.Seek(fileLength, SeekOrigin.Begin);

        //        HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
        //        request.ReadWriteTimeout = 5 * 1000;
        //        request.Timeout = 2 * 1000;
        //        //request.ServicePoint.Expect100Continue = false;
        //        request.KeepAlive = false;
        //        //request.ServicePoint.ConnectionLimit = 100;
        //        Stream stream = null;
        //        //断点续传核心，设置远程访问文件流的起始位置
        //        request.AddRange((int)fileLength);
        //        var response = request.GetResponse();
        //        try
        //        {
        //            stream = response.GetResponseStream();
        //        }
        //        catch (Exception ex)
        //        {
        //            Debug.LogError($"index========={index}");
        //            Debug.LogError($"url == {url} |||| {ex.ToString()}");
        //            continue;
        //        }

        //        byte[] buffer = new byte[1024];
        //        //使用流读取内容到buffer中
        //        //返回值代表读取的实际长度,并不是buffer有多大，stream就会读进去多少
        //        int length = stream.Read(buffer, 0, buffer.Length);

        //        while (length > 0)
        //        {

        //            //将内容再写入本地文件中
        //            fs.Write(buffer, 0, length);
        //            //计算进度
        //            fileLength += length;

        //            //类似尾递归
        //            length = stream.Read(buffer, 0, buffer.Length);

        //        }

        //        stream.Close();
        //        stream.Dispose();

        //        if (request != null)
        //        {
        //            request.Abort();
        //            request = null;
        //        }
        //        if (response != null)
        //        {
        //            response.Close();
        //            response = null;
        //        }
        //    }
        //    fs.Close();
        //    fs.Dispose();
        //}
    }


    void DownLoadDone(Texture2D texture)
    {
        Debug.Log($"主线程回调0================");
        rawImageList[0].texture = texture;
    }


    void DownLoadFileUseUnityWebRequest(string url, string downloadFilePathAndName, Action<UnityWebRequest> actionResult)
    {
        //错误实例 UnityWebRequest不能放到子线程中
        Task task = new Task(()=> {
            var uwr = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
            uwr.downloadHandler = new DownloadHandlerFile(downloadFilePathAndName);
            uwr.SendWebRequest().completed += delegate (AsyncOperation t)
            {
                if (!(uwr.isNetworkError || uwr.isHttpError))
                {
                    if (t.isDone)
                    {
                        if (actionResult != null)
                        {
                            actionResult(uwr);
                        }
                    }
                   
                }
                else
                {
                    Debug.Log("下载失败，请检查网络，或者下载地址是否正确 ");
                }
            };
        });

        task.Start();


    }

    /// <summary>
    /// 获取下载进度
    /// </summary>
    /// <returns></returns>
    public float GetProcess()
    {
        //webRequest 为UnityWebRequest对象
        //if (webRequest != null)
        //{
        //    return webRequest.downloadProgress;
        //}
        return 0;
    }

    /// <summary>
    /// 获取当前下载内容长度
    /// </summary>
    /// <returns></returns>
    public long GetCurrentLength()
    {
        //webRequest 为UnityWebRequest对象
        //if (webRequest != null)
        //{
        //    return (long)webRequest.downloadedBytes;
        //}
        return 0;
    }
}
