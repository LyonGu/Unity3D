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
		//服务器文件列表信息 LoadVersions里会处理
		private static readonly Dictionary<string, VFile> _updateData = new Dictionary<string, VFile> ();
		//本地文件列表信息 streamAssets文件夹
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
			//BuildScript.outputPath + "/" + "ver"
			var path = outputPath + "/" + Filename;
			if (File.Exists (path)) {
				File.Delete (path);
			} 
			//BuildScript.outputPath + "/" + "res"
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
			//把files存储的所有的VFile文件相关信息写入res文件中，并通过WriteFile把文件具体内容写到res文件中
			disk.Save ();

            //VFS 相关逻辑
			using (var stream = File.OpenWrite (path)) {
				//版本记录文件
				var writer = new BinaryWriter (stream);
				//往ver文件里写入版本号以及文件总数量
				writer.Write (version);
				writer.Write (disk.files.Count + 1);
				//res文件记录，用于开启VFS下载
				using (var fs = File.OpenRead (dataPath)) {
					var file = new VFile { name = Dataname, len = fs.Length, hash = Utility.GetCRC32Hash (fs) };
					//往ver文件里写入res信息{文件名字，文件长度，内容哈希值}
					file.Serialize (writer);
				} 
				foreach (var file in disk.files) {
					//往ver文件里写入单个bundle信息{文件名字，文件长度，内容哈希值}
					file.Serialize (writer);
				}
			}
			
			/*
			 * 这一步后，res文件和ver文件
			 *
			 * res文件内容：先把文件信息存储完再存储文件内容
			 * {
			 * 	  文件总数量
			 * 	  bundle1名字，bundle1文件大小，bundel1内容hash值，
			 * 	  bundle2名字，bundle2文件大小，bundel2内容hash值，
			 * 	  bundle3名字，bundle3文件大小，bundel3内容hash值，
			 * 	  ......
			 * 	  bundle1内容，bundle2内容，bundle2内容，
			 * }
			 *
			 * ver文件内容
			 * {
			 * 		当前版本号
			 * 		文件总数量
			 * 		res文件名字，res文件长度，res内容hash值
			 * 		bundle1名字，bundle1文件大小，bundel1内容hash值，
			 * 	  	bundle2名字，bundle2文件大小，bundel2内容hash值，
			 * 	  	bundle3名字，bundle3文件大小，bundel3内容hash值，
			 * 		......
			 * }
			 * 
			 */
		}
		
        //加载版本号
		public static int LoadVersion (string filename)
		{
			if (!File.Exists (filename))
				return -1;
			try
			{
				using (var stream = File.OpenRead (filename)) {
					var reader = new BinaryReader (stream);
					return reader.ReadInt32 (); //ver文件里头4个字节记录的是版本号
				}
			}
			catch (Exception e)
			{
				Debug.LogException(e);
				return -1;
			} 
		}
        //加载版本信息 返回对应的版本的所有文件列表数据
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename">一般是ver文件</param>
        /// <param name="update"></param>
        /// <returns></returns>
        public static List<VFile> LoadVersions (string filename, bool update = false)
		{
            var rootDir = Path.GetDirectoryName(filename);
			var data = update ? _updateData : _baseData;
			data.Clear ();
			using (var stream = File.OpenRead (filename)) {
				var reader = new BinaryReader (stream); //二进制读取
				var list = new List<VFile> ();
				var ver = reader.ReadInt32 (); //第一个4个字节为版本号
				Debug.Log ("LoadVersions:" + ver); 
                var count = reader.ReadInt32 (); //第二个4个字节为文件数量,包含了res文件
				for (var i = 0; i < count; i++) {
                    //根据ver文件信息重新构建VFile信息：文件名字，文件大小，文件内容对应哈希值
					var version = new VFile ();
					version.Deserialize (reader);
					list.Add (version);
					//<bundle名字，VFile>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="savePath">res本地文件，Application.persistentDataPath</param>
        /// <param name="newFiles">更新过的文件信息</param>
		public static void UpdateDisk(string savePath, List<VFile> newFiles)
		{
			var saveFiles = new List<VFile> ();
			var files = _disk.files; //本地文件列表信息
			foreach (var file in files) {
				if (_updateData.ContainsKey (file.name)) {
					//_updateData存的是服务器记录的文件列表信息
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
				//_baseData 判断StreamAssets本地文件是否存在
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