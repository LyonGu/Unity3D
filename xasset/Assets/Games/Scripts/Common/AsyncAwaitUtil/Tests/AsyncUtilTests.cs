using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace UnityAsyncAwaitUtil
{
    public class AsyncUtilTests : MonoBehaviour
    {
        const string AssetBundleSampleUrl = "http://www.stevevermeulen.com/wp-content/uploads/2017/09/teapot.unity3d";
        const string AssetBundleSampleAssetName = "Teapot";

        [SerializeField]
        TestButtonHandler.Settings _buttonSettings = null;

        TestButtonHandler _buttonHandler;

        public void Awake()
        {
            _buttonHandler = new TestButtonHandler(_buttonSettings);
        }

        public void OnGUI()
        {
            _buttonHandler.Restart();

            if (_buttonHandler.Display("Test await seconds"))
            {
                RunAwaitSecondsTestAsync();
            }

            if (_buttonHandler.Display("Test return value"))
            {
                RunReturnValueTestAsync();
            }

            if (_buttonHandler.Display("Test try-catch exception"))
            {
                RunTryCatchExceptionTestAsync();
            }

            if (_buttonHandler.Display("Test unhandled exception"))
            {
                // Note: Without WrapErrors here this wouldn't log anything
                RunUnhandledExceptionTestAsync().WrapErrors();
            }

            if (_buttonHandler.Display("Test IEnumerator"))
            {
                RunIEnumeratorTestAsync();
            }

            if (_buttonHandler.Display("Test IEnumerator with return value (untyped)"))
            {
                
                RunIEnumeratorUntypedStringTestAsync();
            }

            if (_buttonHandler.Display("Test IEnumerator with return value (typed)"))
            {
                RunIEnumeratorStringTestAsync();
            }

            if (_buttonHandler.Display("Test IEnumerator unhandled exception"))
            {
                RunIEnumeratorUnhandledExceptionAsync().WrapErrors();
            }

            if (_buttonHandler.Display("Test IEnumerator try-catch exception"))
            {
                RunIEnumeratorTryCatchExceptionAsync().WrapErrors();
            }

            if (_buttonHandler.Display("Load assetbundle"))
            {
                //这个流程控制才是我最想要的
                RunAsyncOperationAsync().WrapErrors();
//                RunAsyncOperationAsyncFile();

            }

            if (_buttonHandler.Display("Test opening notepad"))
            {
                RunOpenNotepadTestAsync().WrapErrors();
            }

            if (_buttonHandler.Display("Test www download"))
            {
                //网络下载最好加上WrapErrors，可以Unity捕获异常
                RunWwwAsync().WrapErrors();
            }

            if (_buttonHandler.Display("Test Call Async from coroutine"))
            {
                //协程中使用异步
                StartCoroutine(RunAsyncFromCoroutineTest());
            }

            if (_buttonHandler.Display("Test multiple threads"))
            {
                //多线程测试 这个也挺有用
                RunMultipleThreadsTestAsync().WrapErrors();
            }
        }

        IEnumerator RunAsyncFromCoroutineTest()
        {
            Debug.Log("Waiting 1 second...");
            yield return new WaitForSeconds(1.0f);
            Debug.Log("Waiting 1 second again...");
            yield return RunAsyncFromCoroutineTest2().AsIEnumerator();
            Debug.Log("Done");
        }

        async Task RunMultipleThreadsTestAsync()
        {
            //主线程执行
            PrintCurrentThreadContext("Start");
            await Task.Delay(TimeSpan.FromSeconds(1.0f));
            PrintCurrentThreadContext("After delay");
            
            //开了一个独立线程
            await new WaitForBackgroundThread();
            PrintCurrentThreadContext("After WaitForBackgroundThread");
            Debug.Log("Waiting 1 second...");
            
            //在开一个独立线程
            await Task.Delay(TimeSpan.FromSeconds(1.0f));
            // We will still running from the threadpool after the delay here
            PrintCurrentThreadContext("After Waiting");
            
            //返回主线程
            // We can place any unity yield instruction here instead and it will return to the unity thread
            await new WaitForUpdate();
            PrintCurrentThreadContext("After WaitForUpdate");
        }

        async Task RunMultipleThreadsTestAsyncWait()
        {
            PrintCurrentThreadContext("RunMultipleThreadsTestAsyncWait1");
            await new WaitForSeconds(1.0f);
            PrintCurrentThreadContext("RunMultipleThreadsTestAsyncWait2");
        }

        void PrintCurrentThreadContext(string prefix = null)
        {
            Debug.Log(string.Format("{0}Current Thread: {1}, Scheduler: {2}",
                prefix == null ? "" : prefix + ": ", Thread.CurrentThread.ManagedThreadId, SynchronizationContext.Current == null ? "null" : SynchronizationContext.Current.GetType().Name));
        }

        async Task RunAsyncFromCoroutineTest2()
        {
            await new WaitForSeconds(1.0f);
        }

        async Task RunWwwAsync()
        {
            Debug.Log("Downloading asset bundle using WWW");
            var bytes = (await new WWW(AssetBundleSampleUrl)).bytes;
            Debug.Log("Downloaded " + (bytes.Length / 1024) + " kb");
            throw new Exception("foo");
        }

        async Task RunOpenNotepadTestAsync()
        {
            Debug.Log("Waiting for user to close notepad...");
            await Process.Start("notepad.exe");
            Debug.Log("Closed notepad");
        }

        async Task RunUnhandledExceptionTestAsync()
        {
            // This should be logged when using WrapErrors
            await WaitThenThrowException();
        }

        async Task RunTryCatchExceptionTestAsync()
        {
            try
            {
                await NestedRunAsync();
            }
            catch (Exception e)
            {
                Debug.Log("Caught exception! " + e.Message);
            }
        }

        async Task NestedRunAsync()
        {
            await new WaitForSeconds(1);
            throw new Exception("foo");
        }

        async Task WaitThenThrowException()
        {
            await new WaitForSeconds(1.5f);
            throw new Exception("asdf");
        }

        async Task RunAsyncOperationAsync()
        {
            await InstantiateAssetBundleAsync(AssetBundleSampleUrl, AssetBundleSampleAssetName);
        }

        async Task InstantiateAssetBundleAsync(string abUrl, string assetName)
        {
            // We could use WWW here too which might be easier
            Debug.Log("Downloading asset bundle data...");
            byte[] data = await DownloadRawDataAsync(abUrl);
            Debug.Log("Downloading asset bundle data Done...");
            
            Debug.Log("Load AB async...");
            var assetBundle = await AssetBundle.LoadFromMemoryAsync(data);
            Debug.Log("Load AB async Done...");
            
            Debug.Log("Load AB asset async...");
            var prefab = (GameObject)(await assetBundle.LoadAssetAsync<GameObject>(assetName));
            Debug.Log("Load AB asset async done...");
            
            GameObject.Instantiate(prefab);
            assetBundle.Unload(false);
            Debug.Log("Asset bundle instantiated");
        }
        
        async Task RunAsyncOperationAsyncFile()
        {
            await InstantiateAssetBundleAsyncFile(AssetBundleSampleAssetName);
        }
        
        
        async Task InstantiateAssetBundleAsyncFile(string assetName)
        {
            
            Debug.Log($"Load AB File async...  {Time.frameCount}");
            var assetBundle = await AssetBundle.LoadFromFileAsync("Assets/Plugins/AsyncAwaitUtil/Tests/teapot.unity3d");
            Debug.Log($"Load AB File async Done... {Time.frameCount}");
            
            Debug.Log($"Load AB File asset async... {Time.frameCount}");
            var prefab = (GameObject)(await assetBundle.LoadAssetAsync<GameObject>(assetName));
            Debug.Log($"Load AB File asset async done... {Time.frameCount}");
            
            GameObject.Instantiate(prefab);
            assetBundle.Unload(false);
            Debug.Log("Asset bundle File instantiated");
        }

        async Task<byte[]> DownloadRawDataAsync(string url)
        {
            var request = UnityWebRequest.Get(url);
            await request.Send();
            return request.downloadHandler.data;
        }

        async Task RunIEnumeratorTryCatchExceptionAsync()
        {
            try
            {
                await WaitThenThrow();
            }
            catch (Exception e)
            {
                Debug.Log("Caught exception! " + e.Message);
            }
        }

        async Task RunIEnumeratorUnhandledExceptionAsync()
        {
            await WaitThenThrow();
        }

        IEnumerator WaitThenThrow()
        {
            yield return WaitThenThrowNested();
        }

        IEnumerator WaitThenThrowNested()
        {
            Debug.Log("Waiting 1 second...");
            yield return new WaitForSeconds(1.0f);
            throw new Exception("zxcv");
        }

        async Task RunIEnumeratorStringTestAsync()
        {
            Debug.Log("Waiting for ienumerator...");
            Debug.Log("Done! Result: " + await WaitForString());
        }

        async Task RunIEnumeratorUntypedStringTestAsync()
        {
            Debug.Log("Waiting for ienumerator...");
            string result = (string)(await WaitForStringUntyped());
            Debug.Log("Done! Result: " + result);
        }

        async Task RunIEnumeratorTestAsync()
        {
            Debug.Log("Waiting for ienumerator...");
            await WaitABit();
            Debug.Log("Done!");
        }

        IEnumerator<string> WaitForString()
        {
            var startTime = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - startTime < 2)
            {
                yield return null;
            }
            yield return "bsdfgas";
        }

        IEnumerator WaitForStringUntyped()
        {
            yield return WaitABit();
            yield return "qwer";
        }

        IEnumerator WaitABit()
        {
            yield return new WaitForSeconds(2.0f);
        }

        async Task RunReturnValueTestAsync()
        {
            Debug.Log("Waiting to get value...");
            var result = await GetValueExampleAsync();
            Debug.Log("Got value: " + result);
        }

        async Task<string> GetValueExampleAsync()
        {
            await new WaitForSeconds(1.0f);
            return "asdf";
        }
        
        async Task RunAwaitSecondsTestAsync()
        {
            Debug.Log("Waiting 1 second...");
            await new WaitForSeconds(1.0f);
            Debug.Log("Done!");
        }
    }
}
