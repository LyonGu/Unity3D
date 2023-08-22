using HybridCLR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using YooAsset;

/// <summary>
/// 远端资源地址查询服务类 这个类是临时拷贝来的 原始的引用不到 TODO
/// </summary>
public class RemoteServices : IRemoteServices
{
    private readonly string _defaultHostServer;
    private readonly string _fallbackHostServer;

    public RemoteServices(string defaultHostServer, string fallbackHostServer)
    {
        _defaultHostServer = defaultHostServer;
        _fallbackHostServer = fallbackHostServer;
    }
    string IRemoteServices.GetRemoteFallbackURL(string fileName)
    {
        return $"{_defaultHostServer}/{fileName}";
    }
    string IRemoteServices.GetRemoteMainURL(string fileName)
    {
        return $"{_fallbackHostServer}/{fileName}";
    }
}


/*
 *
 * //脚本工作流程
1.下载资源  ==》 使用YooAssets进行资源加载TODO
    1)资源文件，ab包等
    2)热更新dll
    3)A0T泛型补充元数据dll
2.给AOTd1补充元数据，过RuntimeAp.LoadMe ladataForAOTAssembly)
 */
public class LoadDll : MonoBehaviour
{

    public EPlayMode PlayMode = EPlayMode.HostPlayMode;
    
    void Start()
    {
        
        // StartCoroutine(DownLoadAssets(this.StartGame));
        
        StartCoroutine(DownLoadAssetsByYooAssets());
    }

    #region download assets

    private static Dictionary<string, byte[]> s_assetDatas = new Dictionary<string, byte[]>();

    public static byte[] ReadBytesFromStreamingAssets(string dllName)
    {
        return s_assetDatas[dllName];
    }

    private string GetWebRequestPath(string asset)
    {
        var path = $"{Application.streamingAssetsPath}/{asset}";
        if (!path.Contains("://"))
        {
            path = "file://" + path;
        }
        return path;
    }
    private static List<string> AOTMetaAssemblyFiles { get; } = new List<string>()
    {
        "mscorlib.dll",
        "System.dll",
        "System.Core.dll",
    };

    IEnumerator DownLoadAssetsByYooAssets()
    {
        yield return null;
        //1 初始化 YooAssets
        
        // 初始化资源系统
        YooAssets.Initialize();

        // 创建默认的资源包
        var package = YooAssets.CreatePackage("DefaultPackage");

        // 设置该资源包为默认的资源包，可以使用YooAssets相关加载接口加载该资源包内容。
        YooAssets.SetDefaultPackage(package);

        if (PlayMode == EPlayMode.EditorSimulateMode)
        {
            var initParameters = new EditorSimulateModeParameters();
            initParameters.SimulateManifestFilePath  = EditorSimulateModeHelper.SimulateBuild("DefaultPackage");
            yield return package.InitializeAsync(initParameters);
        }
        else if (PlayMode == EPlayMode.OfflinePlayMode)
        {
            var initParameters = new OfflinePlayModeParameters();
            yield return package.InitializeAsync(initParameters);
        }
        else if (PlayMode == EPlayMode.HostPlayMode)
        {
            string defaultHostServer = "http://127.0.0.1/HttpServer/CDN/PC/v1.0";
            string fallbackHostServer = "http://127.0.0.1/HttpServer/CDN/PC/v1.0";
            var initParameters = new HostPlayModeParameters();
            initParameters.QueryServices = new GameQueryServices(); //太空战机DEMO的脚本类，详细见StreamingAssetsHelper
            // initParameters.DecryptionServices = new GameDecryptionServices();
            initParameters.RemoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
            var initOperation = package.InitializeAsync(initParameters);
            yield return initOperation;
    
            if(initOperation.Status == EOperationStatus.Succeed)
            {
                Debug.Log("资源包初始化成功！");
            }
            else 
            {
                Debug.LogError($"资源包初始化失败：{initOperation.Error}");
            }
        }
        
        
        //2 获取资源版本
        var tpackage = YooAssets.GetPackage("DefaultPackage");
        var operation = tpackage.UpdatePackageVersionAsync();
        yield return operation;
        string packageVersion = string.Empty;
        if (operation.Status == EOperationStatus.Succeed)
        {
            //更新成功
            packageVersion = operation.PackageVersion;
            Debug.Log($"Updated package Version : {packageVersion}");
        }
        else
        {
            //更新失败
            Debug.LogError(operation.Error);
            yield break;
        }
        
        //3 更新资源清单
        // 更新成功后自动保存版本号，作为下次初始化的版本。
        // 也可以通过operation.SavePackageVersion()方法保存。
        bool savePackageVersion = true;
        var operation1 = package.UpdatePackageManifestAsync(packageVersion, savePackageVersion);
        yield return operation1;

        if (operation1.Status == EOperationStatus.Succeed)
        {
            //更新成功
        }
        else
        {
            //更新失败
            Debug.LogError(operation.Error);
            yield break;
        }
        
        //4资源包下载
        int downloadingMaxNum = 10;
        int failedTryAgain = 3;
        var downloader = tpackage.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);
    
        //没有需要下载的资源
        if (downloader.TotalDownloadCount == 0)
        {
            StartCoroutine(LoadHotUpdateDLL());
            yield break;
        }

        //需要下载的文件总数和总大小
        int totalDownloadCount = downloader.TotalDownloadCount;
        long totalDownloadBytes = downloader.TotalDownloadBytes;    

        //注册回调方法
        downloader.OnDownloadErrorCallback = OnDownloadErrorFunction;
        downloader.OnDownloadProgressCallback = OnDownloadProgressUpdateFunction;
        downloader.OnDownloadOverCallback = OnDownloadOverFunction;
        downloader.OnStartDownloadFileCallback = OnStartDownloadFileFunction;

        //开启下载
        downloader.BeginDownload();
        yield return downloader;

        //检测下载结果
        if (downloader.Status == EOperationStatus.Succeed)
        {
            //下载成功
            
            //################# 加载dll，实现c#代码热更, dll为原生文件，使用加载原生文件接口
            StartCoroutine(LoadHotUpdateDLL());



        }
        else
        {
            //下载失败
        }
    }

    IEnumerator LoadHotUpdateDLL()
    {
        //加载对应dll
        var assets = new List<string>
        {
            "HotUpdate.dll"
        }.Concat(AOTMetaAssemblyFiles);
        var package = YooAssets.GetPackage("DefaultPackage");
        foreach (var asset in assets)
        {
            string location = asset;
            RawFileOperationHandle handle = package.LoadRawFileAsync(location);
            yield return handle;
            byte[] assetData = handle.GetRawFileData();
                
            Debug.Log($"dll:{asset}  size:{assetData.Length}");
            s_assetDatas[asset] = assetData;

            // string fileText = handle.GetRawFileText();
            // string filePath = handle.GetRawFilePath();
        }
            
        //加载对应AOT dll
        LoadMetadataForAOTAssemblies();
#if !UNITY_EDITOR
            _hotUpdateAss = Assembly.Load(ReadBytesFromStreamingAssets("HotUpdate.dll"));
#else
        _hotUpdateAss = System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "HotUpdate");
#endif
        //加载资源测试热更代码是否生效
        
        Type entryType = _hotUpdateAss.GetType("Entry");
        entryType.GetMethod("Start").Invoke(null, null);
            
        // 通过实例化assetbundle中的资源，还原资源上的热更新脚本
       
        AssetOperationHandle ghandle = package.LoadAssetAsync<GameObject>("Cube");
        ghandle.Completed += Handle_Completed;
            
        // AssetBundle ab = AssetBundle.LoadFromMemory(LoadDll.ReadBytesFromStreamingAssets("prefabs"));
        // GameObject cube = ab.LoadAsset<GameObject>("Cube");
        // GameObject.Instantiate(cube);
    }

    void Handle_Completed(AssetOperationHandle handle)
    {
        GameObject go = handle.InstantiateSync();
        Debug.Log($"Prefab name is {go.name}");
    }
    
    private void OnDownloadErrorFunction(string fileName, string error)
    {
        Debug.Log($"DownloadError :{fileName}  error:{error}");
    }

    private void OnDownloadProgressUpdateFunction(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes,
        long currentDownloadBytes)
    {
        Debug.Log($"DownloadProgress :totalDownloadCount {totalDownloadCount}  currentDownloadCount: {currentDownloadCount} " +
                  $"totalDownloadBytes:{totalDownloadBytes} currentDownloadBytes:{currentDownloadBytes}");
    }

    private void OnDownloadOverFunction(bool isSucceed)
    {
        Debug.Log($"DownloadOver :isSucceed {isSucceed}");
    }

    private void OnStartDownloadFileFunction(string fileName, long sizeBytes)
    {
        Debug.Log($"StartDownload :fileName {fileName}  sizeBytes {sizeBytes}");
    }

    IEnumerator DownLoadAssets(Action onDownloadComplete)
    {
        var assets = new List<string>
        {
            "prefabs",
            "HotUpdate.dll.bytes",
        }.Concat(AOTMetaAssemblyFiles);

        foreach (var asset in assets)
        {
            string dllPath = GetWebRequestPath(asset);
            Debug.Log($"start download asset:{dllPath}");
            UnityWebRequest www = UnityWebRequest.Get(dllPath);
            yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
#else
            if (www.isHttpError || www.isNetworkError)
            {
                Debug.Log(www.error);
            }
#endif
            else
            {
                // Or retrieve results as binary data
                byte[] assetData = www.downloadHandler.data;
                Debug.Log($"dll:{asset}  size:{assetData.Length}");
                s_assetDatas[asset] = assetData;
            }
        }

        onDownloadComplete();
    }

    #endregion

    private static Assembly _hotUpdateAss;

    /// <summary>
    /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
    /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
    /// </summary>
    private static void LoadMetadataForAOTAssemblies()
    {
        /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
        /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
        /// 
        HomologousImageMode mode = HomologousImageMode.SuperSet;
        foreach (var aotDllName in AOTMetaAssemblyFiles)
        {
            byte[] dllBytes = ReadBytesFromStreamingAssets(aotDllName);
            // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
            LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
            Debug.Log($"LoadMetadataForAOTAssembly:{aotDllName}. mode:{mode} ret:{err}");
        }
    }

    void StartGame()
    {
        LoadMetadataForAOTAssemblies();
#if !UNITY_EDITOR
        _hotUpdateAss = Assembly.Load(ReadBytesFromStreamingAssets("HotUpdate.dll.bytes"));
#else
        _hotUpdateAss = System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "HotUpdate");
#endif
        Type entryType = _hotUpdateAss.GetType("Entry");
        entryType.GetMethod("Start").Invoke(null, null);

        Run_InstantiateComponentByAsset();
    }

    private static void Run_InstantiateComponentByAsset()
    {
        // 通过实例化assetbundle中的资源，还原资源上的热更新脚本
        AssetBundle ab = AssetBundle.LoadFromMemory(LoadDll.ReadBytesFromStreamingAssets("prefabs"));
        GameObject cube = ab.LoadAsset<GameObject>("Cube");
        GameObject.Instantiate(cube);
    }

   
}
