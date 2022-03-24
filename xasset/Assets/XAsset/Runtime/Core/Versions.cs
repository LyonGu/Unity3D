//
// Versions.cs
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

/*
 主要是负责资源版本信息的构建与加载的，并且在构建和加载版本信息时就已经用到了VFS

 Versions因为底层实现依赖了VFS，所以支持任意格式的资源文件的版本管理，可以非常方便的对Wwise、Fmod等自定义格式的文件进行版本控制。
*/

namespace libx
{
	public enum VerifyBy
	{
		Size,
		Hash,
	}

	public static class Versions
	{
		public const string Dataname = "res";
		public const string Filename = "ver";
		public static  readonly  VerifyBy verifyBy = VerifyBy.Hash;
		private static readonly VDisk _disk = new VDisk ();
		private static readonly Dictionary<string, VFile> _updateData = new Dictionary<string, VFile> ();
		private static readonly Dictionary<string, VFile> _baseData = new Dictionary<string, VFile> ();

		public static AssetBundle LoadAssetBundleFromFile (string url)
		{
			if (!File.Exists (url)) {
				if (_disk != null) {
					var name = Path.GetFileName (url);
					var file = _disk.GetFile (name, string.Empty);
					if (file != null) {
						return AssetBundle.LoadFromFile (_disk.name, 0, (ulong)file.offset);
					}
				}	
			}   
			return AssetBundle.LoadFromFile (url);
		}

		public static AssetBundleCreateRequest LoadAssetBundleFromFileAsync (string url)
		{
			if (!File.Exists (url)) {
				if (_disk != null) {
					var name = Path.GetFileName (url);
					var file = _disk.GetFile (name, string.Empty);
					if (file != null) {
						return AssetBundle.LoadFromFileAsync (_disk.name, 0, (ulong)file.offset);
					}
				}	
			} 
			return AssetBundle.LoadFromFileAsync (url);
		}

        //构建版本信息
        public static void BuildVersions (string outputPath, string[] bundles, int version)
		{
			var path = outputPath + "/" + Filename;
			if (File.Exists (path)) {
				File.Delete (path);
			} 
			var dataPath = outputPath + "/" + Dataname;
			if (File.Exists (dataPath)) {
				File.Delete (dataPath);
			}  

			var disk = new VDisk (); 
			foreach (var file in bundles) {
				using (var fs = File.OpenRead (outputPath + "/" + file)) {
					//file其实就是bundle文件的名字
					disk.AddFile (file, fs.Length, Utility.GetCRC32Hash (fs));
				}
			} 

			disk.name = dataPath;
			disk.Save ();   

            //VFS 相关逻辑
			using (var stream = File.OpenWrite (path)) {
				//版本文件记录
				var writer = new BinaryWriter (stream);
				writer.Write (version);
				writer.Write (disk.files.Count + 1);
				//res文件记录，用于开启VFS下载
				using (var fs = File.OpenRead (dataPath)) {
					var file = new VFile { name = Dataname, len = fs.Length, hash = Utility.GetCRC32Hash (fs) };
					file.Serialize (writer);
				} 
				foreach (var file in disk.files) {
					file.Serialize (writer);
				}
			}
		}

		public static int LoadVersion (string filename)
		{
			if (!File.Exists (filename))
				return -1;
			try
			{
				using (var stream = File.OpenRead (filename)) {
					var reader = new BinaryReader (stream);
					return reader.ReadInt32 ();
				}
			}
			catch (Exception e)
			{
				Debug.LogException(e);
				return -1;
			} 
		}
        //加载版本信息 返回对应的版本的所有文件列表数据
        public static List<VFile> LoadVersions (string filename, bool update = false)
		{
            var rootDir = Path.GetDirectoryName(filename);
			var data = update ? _updateData : _baseData;
			data.Clear ();
			using (var stream = File.OpenRead (filename)) {
				var reader = new BinaryReader (stream); //二进制读取
				var list = new List<VFile> ();
				var ver = reader.ReadInt32 (); //第一个4个字节为版本号
				Debug.Log ("LoadVersions:" + ver); //第二个4个字节为文件数量
                var count = reader.ReadInt32 ();
				for (var i = 0; i < count; i++) {
                    //构建VFile信息：文件名字，文件大小，哈希值
					var version = new VFile ();
					version.Deserialize (reader);
					list.Add (version);
					data [version.name] = version;
                    var dir = string.Format("{0}/{1}", rootDir, Path.GetDirectoryName(version.name));
                    if (! Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
				} 
				return list;
			}
		} 
		public static void UpdateDisk(string savePath, List<VFile> newFiles)
		{
			var saveFiles = new List<VFile> ();
			var files = _disk.files;
			foreach (var file in files) {
				if (_updateData.ContainsKey (file.name)) {
					saveFiles.Add (file);
				}
			}  
			_disk.Update(savePath, newFiles, saveFiles);
		}

		public static bool LoadDisk (string filename)
		{
			return _disk.Load (filename);
		}


        /// </summary>
        /// <param name="path">本地文件路径</param>
        /// <param name="len">服务器记录的文件大小</param>
        /// <param name="hash">服务器记录的文件hash值</param>
        /// <returns></returns>
		public static bool IsNew (string path, long len, string hash)
		{
			VFile file;
			var key = Path.GetFileName (path);
			if (_baseData.TryGetValue (key, out file)) {
				//res文件，本地文件长度和哈希值都与服务器记录的相等，就不用更新
				if (key.Equals (Dataname) ||
				    file.len == len && file.hash.Equals (hash, StringComparison.OrdinalIgnoreCase)) {
					return false;
				}
			}

			if (_disk.Exists ()) {
				var vdf = _disk.GetFile (path, hash);
				if (vdf != null && vdf.len == len && vdf.hash.Equals (hash, StringComparison.OrdinalIgnoreCase)) {
					return false;
				}
			}

            //本地文件不存在 需要更新
			if (!File.Exists (path)) {
				return true;
			}

            //读取本地文件内容进行比对
			using (var stream = File.OpenRead (path)) {
                //本地文件大小和服务器文件大小不一致 需要更新
				if (stream.Length != len) {
					return true;
				} 
				if (verifyBy != VerifyBy.Hash)
					return false;
                //比较本地文件和服务器文件的hash值，不等的话需要更新
                bool isSameHash = Utility.GetCRC32Hash(stream).Equals(hash, StringComparison.OrdinalIgnoreCase);
                return !isSameHash;
			}
		} 
	}
}