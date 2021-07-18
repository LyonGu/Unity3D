//
// Updater.cs
//
// Author:
//       fjy <jiyuan.feng@live.com>
//
// Copyright (c) 2020 fjy
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace libx
{
    public interface IUpdater
    {
        void OnStart();

        void OnMessage(string msg);

        void OnProgress(float progress);

        void OnVersion(string ver);

        void OnClear();
    }

    [RequireComponent(typeof(Downloader))]
    [RequireComponent(typeof(NetworkMonitor))]
    public class Updater : MonoBehaviour, IUpdater, INetworkMonitorListener
    {
        enum Step
        {
            Wait,
            Copy,
            Coping,
            Versions,
            Prepared,
            Download,
        }

        private Step _step;

        [SerializeField] private string baseURL = "http://127.0.0.1:7888/DLC/";
        [SerializeField] private string gameScene = "Game.unity";
        [SerializeField] private bool enableVFS = true;
        [SerializeField] private bool development;

        public IUpdater listener { get; set; }

        private Downloader _downloader;
        private NetworkMonitor _monitor;
        private string _platform;
        private string _savePath;
        private List<VFile> _versions = new List<VFile>();


        public void OnMessage(string msg)
        {
            if (listener != null)
            {
                listener.OnMessage(msg);
            }
        }

        public void OnProgress(float progress)
        {
            if (listener != null)
            {
                listener.OnProgress(progress);
            }
        }

        public void OnVersion(string ver)
        {
            if (listener != null)
            {
                listener.OnVersion(ver);
            }
        }

        private void Start()
        {
            //初始化downloder并绑定委托
            _downloader = gameObject.GetComponent<Downloader>();
            _downloader.onUpdate = OnUpdate;
            _downloader.onFinished = OnComplete;

            _monitor = gameObject.GetComponent<NetworkMonitor>();
            _monitor.listener = this;

            //获取版本信息文件保存的本地位置
            _savePath = string.Format("{0}/DLC/", Application.persistentDataPath);
            _platform = GetPlatformForAssetBundles(Application.platform);

            _step = Step.Wait;

            Assets.baseURL = baseURL;
            Assets.updatePath = _savePath;
        }

        //private void OnApplicationFocus(bool hasFocus)
        //{
        //    if (_reachabilityChanged || _step == Step.Wait)
        //    {
        //        return;
        //    }

        //    if (hasFocus)
        //    {
        //        MessageBox.CloseAll();
        //        if (_step == Step.Download)
        //        {
        //            _downloader.Restart();
        //        }
        //        else
        //        {
        //            StartUpdate();
        //        }
        //    }
        //    else
        //    {
        //        if (_step == Step.Download)
        //        {
        //            _downloader.Stop();
        //        }
        //    }
        //}

        private bool _reachabilityChanged;

        //网络状态发生变化
        public void OnReachablityChanged(NetworkReachability reachability)
        {
            if (_step == Step.Wait)
            {
                return;
            }

            _reachabilityChanged = true;
            if (_step == Step.Download)
            {
                _downloader.Stop();
            }

            if (reachability == NetworkReachability.NotReachable)
            {
                MessageBox.Show("提示！", "找不到网络，请确保手机已经联网", "确定", "退出").onComplete += delegate(MessageBox.EventId id)
                {
                    if (id == MessageBox.EventId.Ok)
                    {
                        if (_step == Step.Download)
                        {
                            _downloader.Restart();
                        }
                        else
                        {
                            StartUpdate();
                        } 
                        _reachabilityChanged = false;
                    }
                    else
                    {
                        Quit();
                    }
                };
            }
            else
            {
                if (_step == Step.Download)
                {
                    _downloader.Restart();
                }
                else
                {
                    StartUpdate();
                } 
                _reachabilityChanged = false;
                MessageBox.CloseAll();
            }
        }

        private void OnUpdate(long progress, long size, float speed)
        {
            OnMessage(string.Format("下载中...{0}/{1}, 速度：{2}",
                Downloader.GetDisplaySize(progress),
                Downloader.GetDisplaySize(size),
                Downloader.GetDisplaySpeed(speed)));

            OnProgress(progress * 1f / size);
        }

        public void Clear()
        {
            MessageBox.Show("提示", "清除数据后所有数据需要重新下载，请确认！", "清除").onComplete += id =>
            {
                if (id != MessageBox.EventId.Ok)
                    return;
                OnClear();
            };
        }

        public void OnClear()
        {
            OnMessage("数据清除完毕");
            OnProgress(0);
            _versions.Clear();
            _downloader.Clear();
            _step = Step.Wait;
            _reachabilityChanged = false;

            Assets.Clear();

            if (listener != null)
            {
                listener.OnClear();
            }

            if (Directory.Exists(_savePath))
            {
                Directory.Delete(_savePath, true);
            }
        }

        public void OnStart()
        {
            if (listener != null)
            {
                listener.OnStart();
            }
        } 

        private IEnumerator _checking;

        //点击 TOUCH TO START 按钮时，会执行
        public void StartUpdate()
        {
            Debug.Log("StartUpdate.Development:" + development);
#if UNITY_EDITOR
            if (development)
            {
                Assets.runtimeMode = false;
                StartCoroutine(LoadGameScene());
                return;
            }
#endif
            //告知UI进行初始化
            OnStart();

            //如果当前Check协程不为空，就终止
            if (_checking != null)
            {
                StopCoroutine(_checking);
            }

            _checking = Checking();

            StartCoroutine(_checking);
        }

        private void AddDownload(VFile item)
        {
            _downloader.AddDownload(GetDownloadURL(item.name), item.name, _savePath + item.name, item.hash, item.len);
        }

        private void PrepareDownloads()
        {
            if (enableVFS)
            {
                //如果开启VFS，就直接下载res文件
                var path = string.Format("{0}{1}", _savePath, Versions.Dataname);
                if (!File.Exists(path))
                {
                    AddDownload(_versions[0]);//只把res文件加入下载列表
                    return;
                }
                //如果res文件存在，刷新本地文件信息
                Versions.LoadDisk(path);
            }
            //第一个数据是版本信息 下标从1开始是文件信息
            for (var i = 1; i < _versions.Count; i++)
            {
                var item = _versions[i];
                //与本地的文件进行对比
                if (Versions.IsNew(string.Format("{0}{1}", _savePath, item.name), item.len, item.hash))
                {
                    //加入下载列表,Downloader管理需要所有下载的Download信息
                    AddDownload(item);
                }
            }
        }

        private IEnumerator RequestVFS()
        {
            var mb = MessageBox.Show("提示", "是否开启VFS？开启有助于提升IO性能和数据安全。", "开启");

            //为什么这里会暂停程序，上面的逻辑代码不是执行完了吗
            /*
                mb是个迭代器对象，每一帧都会调用mb的MoveNext,只要返回true，就会调用mb的Current方法，mb.Current返回null，意味着 yield return null;
             */
            yield return mb; //返回的是mb.Current
            enableVFS = mb.isOk;
        }

        private static string GetPlatformForAssetBundles(RuntimePlatform target)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (target)
            {
                case RuntimePlatform.Android:
                    return "Android";
                case RuntimePlatform.IPhonePlayer:
                    return "iOS";
                case RuntimePlatform.WebGLPlayer:
                    return "WebGL";
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    return "Windows";
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    return "iOS"; // OSX
                default:
                    return null;
            }
        }

        private string GetDownloadURL(string filename)
        {
            return string.Format("{0}{1}/{2}", baseURL, _platform, filename);
        }

        private IEnumerator Checking()
        {
            if (!Directory.Exists(_savePath))
            {
                Directory.CreateDirectory(_savePath);
            }

            //询问是否开启VFS
            if (_step == Step.Wait)
            {
                yield return RequestVFS();
                _step = Step.Copy;
            }

            if (_step == Step.Copy)
            {
                //加载本地版本信息文件，如果StreamingAssets下面有资源会询问是否复制资源
                yield return RequestCopy();
            }

            if (_step == Step.Coping)
            {
                var path = _savePath + Versions.Filename + ".tmp";
                var versions = Versions.LoadVersions(path);
                var basePath = GetStreamingAssetsPath() + "/";
                yield return UpdateCopy(versions, basePath);
                _step = Step.Versions;
            }

            if (_step == Step.Versions)
            {
                //请求并加载云端版本信息文件
                yield return RequestVersions();
               
            }

            if (_step == Step.Prepared)
            {
                OnMessage("正在检查版本信息...");
                var totalSize = _downloader.size;
                if (totalSize > 0)
                {
                    var tips = string.Format("发现内容更新，总计需要下载 {0} 内容", Downloader.GetDisplaySize(totalSize));
                    var mb = MessageBox.Show("提示", tips, "下载", "退出");
                    yield return mb;
                    if (mb.isOk)
                    {
                        //开始正式下载资源，并记录当前的下载进度，用于做断点续传
                        _downloader.StartDownload();
                        _step = Step.Download;
                    }
                    else
                    {
                        Quit();
                    } 
                }
                else
                {
                    //所有文件更新完成，再次更新本地版本信息文件
                    OnComplete();
                }
            } 
        }

        //向服务器请求版本信息
        private IEnumerator RequestVersions()
        {
            OnMessage("正在获取版本信息...");
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                var mb = MessageBox.Show("提示", "请检查网络连接状态", "重试", "退出");
                yield return mb;
                if (mb.isOk)
                {
                    StartUpdate();
                }
                else
                {
                    Quit();
                } 
                yield break;
            }

            
            //把服务器版本文件下载到本地，版本文件里记录的所有文件列表
            string remoteVerPath = GetDownloadURL(Versions.Filename); //服务器版本文件路径
            string localVerPath = _savePath + Versions.Filename; //本地版本文件存储路径
            var request = UnityWebRequest.Get(remoteVerPath);//加载资源服务器版本文件
            request.downloadHandler = new DownloadHandlerFile(localVerPath); //设置本地版本文件存储路径
            yield return request.SendWebRequest();
            var error = request.error;
            request.Dispose();
            if (!string.IsNullOrEmpty(error))
            {
                var mb = MessageBox.Show("提示", string.Format("获取服务器版本失败：{0}", error), "重试", "退出");
                yield return mb;
                if (mb.isOk)
                {
                    StartUpdate();
                }
                else
                {
                    Quit();
                } 
                yield break; 
            } 
            try
            {
                _versions = Versions.LoadVersions(localVerPath, true); //localVerPath 已经从服务器下载完成的版本文件 版本文件里有需要下载的文件列表数据
                if (_versions.Count > 0)
                {
                    PrepareDownloads();//检测是否有需要下载的
                    _step = Step.Prepared;
                }
                else
                {
                    OnComplete();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                MessageBox.Show("提示", "版本文件加载失败", "重试", "退出").onComplete +=
                    delegate(MessageBox.EventId id)
                    {
                        if (id == MessageBox.EventId.Ok)
                        {
                            StartUpdate();
                        }
                        else
                        {
                            Quit();
                        }
                    };
            }
        }

        public static string GetStreamingAssetsPath()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                return Application.streamingAssetsPath;
            }

            if (Application.platform == RuntimePlatform.WindowsPlayer ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return "file:///" + Application.streamingAssetsPath;
            }

            return "file://" + Application.streamingAssetsPath;
        }

        private IEnumerator RequestCopy()
        {
            var v1 = Versions.LoadVersion(_savePath + Versions.Filename);
            var basePath = GetStreamingAssetsPath() + "/";
            var request = UnityWebRequest.Get(basePath + Versions.Filename);
            var path = _savePath + Versions.Filename + ".tmp";
            request.downloadHandler = new DownloadHandlerFile(path);//path 储存文件的路径和文件名 这句代码会生成一个对应文件
            yield return request.SendWebRequest();
            if (string.IsNullOrEmpty(request.error))
            {
                var v2 = Versions.LoadVersion(path);
                if (v2 > v1)
                {
                    var mb = MessageBox.Show("提示", "是否将资源解压到本地？", "解压", "跳过");
                    yield return mb;
                    _step = mb.isOk ? Step.Coping : Step.Versions;
                }
                else
                {
                    Versions.LoadVersions(path);
                    _step = Step.Versions;
                }
            }
            else
            {
                _step = Step.Versions;
            } 
            request.Dispose();
        }

        private IEnumerator UpdateCopy(IList<VFile> versions, string basePath)
        {
            var version = versions[0];
            if (version.name.Equals(Versions.Dataname))
            {
                var request = UnityWebRequest.Get(basePath + version.name);
                request.downloadHandler = new DownloadHandlerFile(_savePath + version.name);
                var req = request.SendWebRequest();
                while (!req.isDone)
                {
                    OnMessage("正在复制文件");
                    OnProgress(req.progress);
                    yield return null;
                }

                request.Dispose();
            }
            else
            {
                for (var index = 0; index < versions.Count; index++)
                {
                    var item = versions[index];
                    var request = UnityWebRequest.Get(basePath + item.name);
                    request.downloadHandler = new DownloadHandlerFile(_savePath + item.name);
                    yield return request.SendWebRequest();
                    request.Dispose();
                    OnMessage(string.Format("正在复制文件：{0}/{1}", index, versions.Count));
                    OnProgress(index * 1f / versions.Count);
                }
            }
        }

        private void OnComplete()
        {
            if (enableVFS)
            {
                var dataPath = _savePath + Versions.Dataname;
                var downloads = _downloader.downloads;
                if (downloads.Count > 0 && File.Exists(dataPath))
                {
                    OnMessage("更新本地版本信息");
                    var files = new List<VFile>(downloads.Count);
                    foreach (var download in downloads)
                    {
                        files.Add(new VFile
                        {
                            name = download.name,
                            hash = download.hash,
                            len = download.len,
                        });
                    }

                    var file = files[0];
                    if (!file.name.Equals(Versions.Dataname))
                    {
                        Versions.UpdateDisk(dataPath, files);
                    }
                }
                //开启VFS后，需要更新本地信息
                Versions.LoadDisk(dataPath);
            }

            OnProgress(1);
            OnMessage("更新完成");

            //读取最新版本号
            var version = Versions.LoadVersion(_savePath + Versions.Filename);
            if (version > 0)
            {
                //显示最新版本号再UI上
                OnVersion(version.ToString());
            }

            //进入游戏场景
            StartCoroutine(LoadGameScene());
        }

        private IEnumerator LoadGameScene()
        {
            // OnMessage("正在初始化");
            //初始化AB系统，加载Manifest文件 ==> 初始化信息 
            /*
             *
             *     //存储bundle的依赖信息   Assets._bundleToDependencies
                        //key：当前bundle
                        //value: 当前bundle依赖的bundle
                        
                //asset与bundle的对应关系 Assets._assetToBundles
                //Key: asset的全路径
                //value：对应的bundleName
             */
            
            var init = Assets.Initialize(); //
            yield return init;
            if (string.IsNullOrEmpty(init.error))
            {
                //添加搜索路径
                Assets.AddSearchPath("Assets/XAsset/Demo/Scenes");
                init.Release();
                OnProgress(0);
                OnMessage("加载游戏场景");
                //异步加载 Game.unity 这里使用了更智能的寻址模式，在上一个版本中 需要输出 Assets/XAsset/Demo/Scenes/Game.unity, 具体参考 SearchPath
                var scene = Assets.LoadSceneAsync(gameScene, false);
                while (!scene.isDone)
                {
                    OnProgress(scene.progress);
                    yield return null;
                }
            }
            else
            {
                init.Release();
                var mb = MessageBox.Show("提示", "初始化异常错误：" + init.error + "请联系技术支持");
                yield return mb;
                Quit();
            }
        }

        private void OnDestroy()
        {
            MessageBox.Dispose();
        }

        private void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
