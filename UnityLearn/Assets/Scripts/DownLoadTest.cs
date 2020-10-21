using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class DownLoadTest : MonoBehaviour
{
    private HttpDownLoad httpDownLoad;
    private System.Object loomParms = new System.Object();

    public GameObject ImageObj;
    void Start()
    {
        httpDownLoad = new HttpDownLoad();
        string url = "https://timgsa.baidu.com/timg?image&quality=80&size=b9999_10000&sec=1603262053359&di=aaefa8e8a14e968682aa9dd3b9882e81&imgtype=0&src=http%3A%2F%2Ft7.baidu.com%2Fit%2Fu%3D378254553%2C3884800361%26fm%3D79%26app%3D86%26f%3DJPEG%3Fw%3D1280%26h%3D2030";
        string savePath = Path.Combine(Application.dataPath, "DownLoadTest");
        string saveFileName = "TestDownLoadOne.jpg";
        //httpDownLoad.DownLoad(url, savePath, saveFileName, ()=> {
        //    Debug.Log($"HttpDownLoad {saveFileName}================");

        //    //这个回调时在子线程，不能操作相关东西
        //    //ImageObj.SetActive(false);
        //});

        url = "https://timgsa.baidu.com/timg?image&quality=80&size=b9999_10000&sec=1603263696553&di=c74c035d556e94d2514d4479cf7e6087&imgtype=0&src=http%3A%2F%2Ft8.baidu.com%2Fit%2Fu%3D3571592872%2C3353494284%26fm%3D79%26app%3D86%26f%3DJPEG%3Fw%3D1200%26h%3D1290";
        saveFileName = "TestDownLoadTwo.jpg";
        httpDownLoad.DownLoadWithTask(url, savePath, saveFileName, () =>
        {
            Debug.Log($"HttpDownLoad {saveFileName}================");
            
        });

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
        Loom.Initialize();

        url = "https://timgsa.baidu.com/timg?image&quality=80&size=b9999_10000&sec=1603263696553&di=c74c035d556e94d2514d4479cf7e6087&imgtype=0&src=http%3A%2F%2Ft8.baidu.com%2Fit%2Fu%3D3571592872%2C3353494284%26fm%3D79%26app%3D86%26f%3DJPEG%3Fw%3D1200%26h%3D1290";
        saveFileName = "TestDownLoad2.jpg";
        httpDownLoad.DownLoadWithTask(url, savePath, saveFileName, () =>
        {
            Loom.QueueOnMainThread((System.Object t) =>
            {
                ImageObj.SetActive(false);
            }, loomParms);

        });

        //直接使用Loom开启多线程
        Loom.RunAsync(() =>
        {
            //TODO
        });


    }

    void DownLoadDone()
    {


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
