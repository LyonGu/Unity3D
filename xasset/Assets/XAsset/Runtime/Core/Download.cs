//
// Download.cs
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
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

/*
 
Download.cs继承DownloadHandlerScript实现了一套自己的下载处理逻辑，
而Downloader.cs就是用来管理所有的Download对象的，并且做了一些附加功能，比如记录当前下载进度，用于做断点续传     
     
*/
namespace libx
{
    public class Download : DownloadHandlerScript, IDisposable, ICloneable
    {
        #region ICloneable implementation

        public object Clone()
        {
            return new Download()
            {
                id = id,
                hash = hash,
                url = url,
                len = len,
                savePath = savePath,
                completed = completed,
                name = name
            };
        }

        #endregion

        public int id { get; set; }

        public string error { get; private set; }

        public long len { get; set; }

        public string hash { get; set; }

        public string url { get; set; }

        public long position { get; private set; }

        public string name { get; set; }

        public string tempPath
        {
            get
            { 
                var dir = Path.GetDirectoryName(savePath); //savePath是包含文件名字的
                string tPath = string.Format("{0}/{1}", dir, hash);
                return tPath;
            }
        }

        public string savePath;

        public Action<Download> completed { get; set; }

        private UnityWebRequest _request;
        private FileStream _stream;
        private bool _running;
        private bool _finished = false;

        protected override float GetProgress()
        {
            return position * 1f / len;
        }

        protected override byte[] GetData()
        {
            return null;
        }

        protected override void ReceiveContentLength(int contentLength)
        {
        }

        //Callback, invoked as data is received from the remote server.
        protected override bool ReceiveData(byte[] buffer, int dataLength)
        {
            if (!string.IsNullOrEmpty(_request.error))
            {
                //下载数据发生错误 直接Complete
                error = _request.error;
                Complete();
                return true;
            }
            //每次接收到数据都往文件里写
            _stream.Write(buffer, 0, dataLength);
            position += dataLength;
            return _running;
        }

        //下载完成会调用这个方法  重写的DownloadHandler方法
        protected override void CompleteContent()
        {
            Complete();
        }

        public override string ToString()
        {
            return string.Format("{0}, size:{1}, hash:{2}", url, len, hash);
        }

        public void Start()
        {
            if (_running)
            {
                return;
            }

            error = null;
            finished = false;
            _running = true;
            
            //先下载到本地临时文件，最后下载完成后会把临时文件内容复制到正式文件，然后删除临时文件
            _stream = new FileStream(tempPath, FileMode.OpenOrCreate, FileAccess.Write);
            position = _stream.Length; //先读取本地文件的大小，预防未下完的情况
            if (position < len)
            {
                _stream.Seek(position, SeekOrigin.Begin); //设置文件写入的起始位置，如果已经下载过，就是断点续传的位置
                _request = UnityWebRequest.Get(url);
                _request.SetRequestHeader("Range", "bytes=" + position + "-");
                _request.downloadHandler = this;  //下载完成会调用CompleteContent
                _request.SendWebRequest();
                Debug.Log("Start Download：" + url); 
            }
            else
            {
                Complete();
            }
        }

        public void Update()
        {
            if (_running)
            {
                if (_request.isDone && _request.downloadedBytes < (ulong)len)
                {
                    error = "unknown error: downloadedBytes < len";
                }
                if (!string.IsNullOrEmpty(_request.error))
                {
                    error = _request.error;
                } 
            }
        }

        public new void Dispose()
        {
            if (_stream != null)
            {
                _stream.Close();
                _stream.Dispose();
                _stream = null;
            } 
            if (_request != null)
            {
                _request.Abort();
                _request.Dispose();
                _request = null;
            }  
            base.Dispose();
            _running = false;
            finished = true;
        }

        public void Complete(bool stop = false)
        {
            //是否一些非托管内存，设置该loader下载完成
            Dispose(); 
            if (stop)
            {
                return;   
            } 
            
            //下载完成后再检测下异常
            CheckError();
        }

        private void CheckError()
        {
            if (File.Exists(tempPath))
            {
                if (string.IsNullOrEmpty(error))
                {
                    using (var fs = File.OpenRead(tempPath))
                    {
                        if (fs.Length != len)
                        {
                            error = "下载文件长度异常:" + fs.Length;
                        } 
                        if (Versions.verifyBy == VerifyBy.Hash)
                        {
                            const StringComparison compare = StringComparison.OrdinalIgnoreCase;
                            if (!hash.Equals(Utility.GetCRC32Hash(fs), compare))
                            {
                                error = "下载文件哈希异常:" + hash;
                            }
                        }
                    }  
                } 
                if (string.IsNullOrEmpty(error))
                {
                    //成功下载后把临时文件拷贝到最终文件里
                    File.Copy(tempPath, savePath, true);
                    //删除临时文件
                    File.Delete(tempPath); 
                    Debug.Log("Complete Download：" + url);
                    if (completed == null)
                        return;

                    //成功下载后调用completed，即Downloader.OnFinished 方法
                    completed.Invoke(this);
                    completed = null; 
                }
                else
                {
                    //如果下载发生错误，直接删除临时文件
                    File.Delete(tempPath);
                } 
            }
            else
            {
                error = "文件不存在"; 
            }
        }

        public void Retry()
        {
            Dispose(); 
            Start();
        }

        public bool finished
        {
            get { return _finished; }
            private set { _finished = value; }
        }
    }

}