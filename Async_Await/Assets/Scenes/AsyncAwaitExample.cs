using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class AsyncAwaitExample : MonoBehaviour
{

    public class MessageBox : IEnumerator
    {
        public bool isOk { get; private set; }

        private bool _visible = true;

        #region IEnumerator implementation

        public bool MoveNext()
        {
            //每一帧都调用MoveNext 返回true的话 就会调用Current方法 返回里面的值
            return _visible;
        }

        public void Reset()
        {
        }

        public object Current
        {
            get { return null; }
        }
        
        #endregion

        public void Hide()
        {
            _visible = false; //只有_visible为false的时候 MoveNext返回为false，迭代器不进行执行退出，
        }

    }

    
    public GameObject UIWindow;

    private MessageBox messageBox;
    private bool isClickBtn = false;
    private bool isContinue = false;
    const string AssetBundleSampleAssetName = "Teapot";
    const string AssetBundleSampleUrl = "http://www.stevevermeulen.com/wp-content/uploads/2017/09/teapot.unity3d";
    // Start is called before the first frame update
    async void Start()
    {
        UIWindow.SetActive(false);
        
        //虽然被标记async异步关键字，但是，还是在当前线程执行 主线程执行
        Debug.Log($"测试 Async await ====1 ThreadId:{Thread.CurrentThread.ManagedThreadId}");
        bool result = await StopProgram();
        Debug.Log($"测试 Async await ====2 ThreadId:{Thread.CurrentThread.ManagedThreadId}");
        
        //使用 async await 工具库
//        RunAsyncOperationAsyncFile();
        UIWindow.SetActive(false);
        await GameProgress(); //异步方法不加await，后面的逻辑不会等待子线程完成就会执行

        
        //还有一种方式可以控制流程 使用迭代器对象 重载MoveNext和Current方法
        StartCoroutine(Checking());
        
    }
    
    private IEnumerator Checking()
    {
        Debug.Log($"使用协程 迭代器对象测试流程控制========start " + Time.frameCount);
        Debug.Log($"使用协程 打开一个窗口========start " + Time.frameCount);
        yield return CreateMessBox();
        Debug.Log($"玩家按了A 使用协程 打开一个窗口========end " + Time.frameCount);
        Debug.Log($"使用协程 迭代器对象测试流程控制========end " + Time.frameCount);
    }
    
    private IEnumerator CreateMessBox()
    {
        messageBox = new MessageBox();

        //为什么这里会暂停程序，上面的逻辑代码不是执行完了吗
        /*
            mb是个迭代器对象，每一帧都会调用mb的MoveNext,只要返回true，就会调用mb的Current方法，mb.Current返回null，意味着 yield return null;
         */
        yield return messageBox; //返回的是mb.Current
    }
    

    async Task GameProgress()
    {
        Debug.Log($"测试 ==========游戏流程开始===="+ Time.frameCount);
        await UpdateAsset();
        await InitConfigs();
        await WaitPlayerClick();
        await LoadAssset();
        Debug.Log($"测试 ==========游戏流程结束===="+ Time.frameCount);
    }

    async Task<bool> WaitPlayerClick()
    {
        UIWindow.SetActive(true);
        Debug.Log($"等待玩家点击按钮====="+ Time.frameCount);
        bool isOk = await GetIsClickBtn();
        Debug.Log($"玩家点击了按钮====="+ Time.frameCount);
        UIWindow.SetActive(false);
        return isOk;
    }

    public void ClickBtn()
    {
        isClickBtn = true;
    }

    async Task UpdateAsset()
    {
        Debug.Log($"测试 热更流程 start====" + Time.frameCount);
        var bytes = (await new WWW(AssetBundleSampleUrl)).bytes;
        Debug.Log("Downloaded " + (bytes.Length / 1024) + " kb  " +   Time.frameCount);
        Debug.Log($"测试 热更流程 End====" + Time.frameCount);
    }
    
    async Task InitConfigs()
    {
        Debug.Log($"测试 配置表初始化 start====" + Time.frameCount );
        await new WaitForSeconds(2.0f);
        Debug.Log($"测试 配置表初始化 End===="+ Time.frameCount);
    }
    
    async Task LoadAssset()
    {
        Debug.Log($"测试 加载资源 start===="+ Time.frameCount);
        await RunAsyncOperationAsyncFile();
        Debug.Log($"测试 加载资源 End===="+ Time.frameCount);
    }

    async Task RunAsyncOperationAsyncFile()
    {
        await InstantiateAssetBundleAsyncFile(AssetBundleSampleAssetName);
    }
    
    async Task InstantiateAssetBundleAsyncFile(string assetName)
    {
            
        Debug.Log($"Load AB File async...  {Time.frameCount}");
        var assetBundle = await AssetBundle.LoadFromFileAsync("Assets/AsyncAwaitUtil/Tests/teapot.unity3d");
        Debug.Log($"Load AB File async Done... {Time.frameCount}");
            
        Debug.Log($"Load AB File asset async... {Time.frameCount}");
        var prefab = (GameObject)(await assetBundle.LoadAssetAsync<GameObject>(assetName));
        Debug.Log($"Load AB File asset async done... {Time.frameCount}");
            
        GameObject.Instantiate(prefab);
        assetBundle.Unload(false);
        Debug.Log("Asset bundle File instantiated");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            isContinue = true;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            messageBox.Hide();
        }
    }

    //异步获取(实际是开一个线程轮询结果)
    private async Task<bool> GetIsContinue()
    {
        return await Task.Run<bool>(() => {
                while (true) {
                    if (isContinue) 
                        return true; 
                }
            });
    }
    
    private async Task<bool> GetIsClickBtn()
    {
        return await Task.Run<bool>(() => {
            while (true) {
                if (isClickBtn) 
                    return true; 
            }
        });
    }

    private async Task<bool> StopProgram()
    {
        bool result = await GetIsContinue();
        Debug.Log($"Program is to==== ThreadId:{Thread.CurrentThread.ManagedThreadId}");
        return result;
        
    }
}

