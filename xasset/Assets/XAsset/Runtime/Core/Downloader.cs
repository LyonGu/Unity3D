//
// Downloader.cs
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
using System.Collections.Generic;
using System.IO;
using UnityEngine;
//Downloader 管理了所有的downLoad
namespace libx
{
    public class Downloader : MonoBehaviour
    {
        private const float BYTES_2_MB = 1f / (1024 * 1024);
        
        public int maxDownloads = 3;
        
        private readonly List<Download> _downloads = new List<Download>(); //需要下载的downLoad
        private readonly List<Download> _tostart = new List<Download>();  // 准备开始下载的downLoad
        private readonly List<Download> _progressing = new List<Download>(); //正在下载的downLoad
        public Action<long, long, float> onUpdate;
        public Action onFinished;

        private int _finishedIndex;
        private int _downloadIndex;
        private float _startTime;
        private float _lastTime;
        private long _lastSize;

        public long size { get; private set; }

        public long position { get; private set; }

        public float speed { get; private set; }

        public List<Download> downloads { get { return _downloads; } }

        private long GetDownloadSize()
        {
            var len = 0L;
            var downloadSize = 0L;
            foreach (var download in _downloads)
            {
                downloadSize += download.position; //每次有数据更新都会去设置download.position
                len += download.len;
            }

            //感觉挺绕的
            //len 表示服务器记录的所有文件大小
            //downloadSize 表示已经下载过的文件大小+再次下载的大小 ==》 总下载大小
            //size 本地所有文件大小
            return downloadSize - (len - size);
        }

        private bool _started;
        [SerializeField]private float sampleTime = 0.5f;

        public void StartDownload()
        {
            //开始下载
            _tostart.Clear(); 
            _finishedIndex = 0; 
            _lastSize = 0L;
            Restart();
        }

        public void Restart()
        {
            _startTime = Time.realtimeSinceStartup;
            _lastTime = 0;
            _started = true;
            _downloadIndex = _finishedIndex; //记录下当下载完成的下标
            var max = Math.Min(_downloads.Count, maxDownloads); //最大三个
            for (var i = _finishedIndex; i < max; i++)
            {
                var item = _downloads[i];  //把数据从_downloads列表放入_tostart列表中 update方法里会检测_tostart列表
                _tostart.Add(item);
                _downloadIndex++;
            }
        }

        public void Stop()
        {
            _tostart.Clear();
            foreach (var download in _progressing)
            {
                download.Complete(true); 
                _downloads[download.id] = download.Clone() as Download;

            } 
            _progressing.Clear();
            _started = false;
        }

        public void Clear()
        {
            size = 0;
            position = 0;
            
            _downloadIndex = 0;
            _finishedIndex = 0;
            _lastTime = 0f;
            _lastSize = 0L;
            _startTime = 0;
            _started = false; 
            foreach (var item in _progressing)
            {
                item.Complete(true);
            }
            _progressing.Clear();
            _downloads.Clear();
            _tostart.Clear();
        }

        public void AddDownload(string url, string filename, string savePath, string hash, long len)
        {
            var download = new Download
            {
                id = _downloads.Count,
                url = url,
                name = filename,
                hash = hash,
                len = len,
                savePath = savePath, //_savePath = string.Format("{0}/DLC/", Application.persistentDataPath) + item.name
                completed = OnFinished  //单个文件下载完回调
            };
            _downloads.Add(download); //加入待下载列表
            var info = new FileInfo(download.tempPath); //判断临时文件是否存在
            if (info.Exists)
            {
                //临时文件存在，计算再次下载的大小时需要用 总大小 - 已经下载过的大小
                size += len - info.Length; 
            }
            else
            {
                //临时文件不存在，需要下载的大小直接就是中大小
                size += len; 
            }

            //size 本地所有文件下载总和
        }

        //单个文件下载完成后会调用到这里
        private void OnFinished(Download download)
        {
            if (_downloadIndex < _downloads.Count) 
            {
                //因为Restart里设置了最大下载量，每个文件下完之后继续把后续文件加入到_tostart里，Update方法里继续遍历
                _tostart.Add(_downloads[_downloadIndex]);
                _downloadIndex++;    
            } 
            _finishedIndex++;
            Debug.Log(string.Format("OnFinished:{0}, {1}", _finishedIndex, _downloads.Count));
            if (_finishedIndex != downloads.Count)
                return;
            
            //所有文件下载完毕,并且没有出错
            if (onFinished != null)
            {
                //所有文件下载完毕，执行业务层回调
                onFinished.Invoke(); //Updater.OnComplete
            } 
            _started = false;
        }

        public static string GetDisplaySpeed(float downloadSpeed)
        {
            if (downloadSpeed >= 1024 * 1024)
            {
                return string.Format("{0:f2}MB/s", downloadSpeed * BYTES_2_MB);
            }
            if (downloadSpeed >= 1024)
            {
                return string.Format("{0:f2}KB/s", downloadSpeed / 1024);
            }
            return string.Format("{0:f2}B/s", downloadSpeed);
        }

        public static string GetDisplaySize(long downloadSize)
        {
            if (downloadSize >= 1024 * 1024)
            {
                return string.Format("{0:f2}MB", downloadSize * BYTES_2_MB);
            }
            if (downloadSize >= 1024)
            {
                return string.Format("{0:f2}KB", downloadSize / 1024);
            }
            return string.Format("{0:f2}B", downloadSize);
        }


        private void Update()
        {
            if (!_started)
                return; 
            
            if (_tostart.Count > 0)
            {
                //限制最大下载数量maxDownloads
                for (var i = 0; i < Math.Min(maxDownloads, _tostart.Count); i++)
                {
                    var item = _tostart[i]; //Download对象
                    item.Start();  //开始每一个任务下载
                    _tostart.RemoveAt(i);  //从开始列表里移除，加入到正在进行中列表里
                    _progressing.Add(item);
                    i--;
                }
            }
        
            for (var index = 0; index < _progressing.Count; index++)
            {
                var download = _progressing[index];
                download.Update(); //检测下载中是否有异常 出现异常怎么办?
                if (!download.finished) //finished字段在下载完成后或者下载出现错误会置为true
                    continue;
                if (!string.IsNullOrEmpty(download.error))
                {
                    //下载出现错误,重新下载一遍
                    download.Retry();
                    continue;
                }

                _progressing.RemoveAt(index); //下载完从_progressing列表里删除，边遍历边删除
                index--;
            }

            position = GetDownloadSize(); //获取已下载大小
            
            //0.5秒更新一次
            var elapsed = Time.realtimeSinceStartup - _startTime;
            if (elapsed - _lastTime < sampleTime)
                return;
            
            var deltaTime = elapsed - _lastTime; 
            speed = (position - _lastSize) / deltaTime;
            if (onUpdate != null)
            {
                //更新进度UI， size为总下载大小
                onUpdate(position, size, speed);
            }
            
            _lastTime = elapsed;  
            _lastSize = position;
        }
    }
}