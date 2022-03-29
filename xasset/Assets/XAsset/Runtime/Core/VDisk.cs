//
// VDisk.cs
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
using System.Net;
using UnityEngine;

namespace libx
{
	public class VFile
	{
	
		//bundle文件内容生成对应的hash值
		public string hash { get; set; }

		public long id { get; set; }
		
		//bundle文件大小
		public long len { get; set; }
		
		//bundle文件名字
		public string name { get; set; }
		
		//相当于文件头的文件偏移量
		public long offset { get; set; }

		public VFile ()
		{
			offset = -1;
		}

		public void Serialize (BinaryWriter writer)
		{
			writer.Write (name);
			writer.Write (len);
			writer.Write (hash);
		}

		public void Deserialize (BinaryReader reader)
		{
			name = reader.ReadString ();
			len = reader.ReadInt64 ();
			hash = reader.ReadString ();
		}
	}

	public class VDisk
	{
		private readonly byte[] _buffers = new byte[1024 * 4];
		
		//<BundleName, VFile>
		private readonly Dictionary<string, VFile> _data = new Dictionary<string, VFile> ();
		private readonly List<VFile> _files = new List<VFile>();
		public  List<VFile> files { get { return _files; }}
		
		//BuildScript.outputPath + "/" + "res"
		public string name { get; set; } 
		private long _pos; //当前读取到的位置下标
		private long _len;

		public VDisk ()
		{
		}

		public bool Exists ()
		{
			return files.Count > 0;
		}

		private void AddFile (VFile file)
		{
			//file.name 为bundle的名字
			_data [file.name] = file;
			files.Add (file);
		}
		
		/// <summary>
		/// 构建一个VFile对象，加入到VFDisk中管理
		/// </summary>
		/// <param name="path">bundle文件名字</param>
		/// <param name="len">bundle文件大小</param>
		/// <param name="hash">bundle文件内容生成对应的hash值</param>
		public void AddFile (string path, long len, string hash)
		{
			var file = new VFile{ name = path, len = len, hash = hash };
			AddFile (file);
		}

		private void WriteFile (string path, BinaryWriter writer)
		{
			using (var fs = File.OpenRead (path)) {
				var len = fs.Length; //单个文件大小
				WriteStream (len, fs, writer);
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="len">单个文件大小</param>
		/// <param name="stream">单个文件的文件流</param>
		/// <param name="writer">写入目标二进制流，这里其实就是res文件</param>
		/// 
		private void WriteStream (long len, Stream stream, BinaryWriter writer)
		{
			//_buffers 只是一个中间缓存数组，先把数据读到_buffers里，然后从_buffers里读出来再写入目标文件
			var count = 0L;
			while (count < len) {
				//一次最多读取1024*4 字节
				var read = (int)Math.Min (len - count, _buffers.Length);
				stream.Read (_buffers, 0, read);
				writer.Write (_buffers, 0, read);
				count += read;
			}
		}

		public bool Load (string path)
		{
			if (!File.Exists (path))
				return false;

			Clear (); //清理下

			name = path;
			using (var reader = new BinaryReader (File.OpenRead (path))) {
				var count = reader.ReadInt32 (); //文件总数
				for (var i = 0; i < count; i++) {
					var file = new VFile { id = i };
					file.Deserialize (reader); //从res文件里读取每个文件的信息
					AddFile (file); //加入文件列表
				} 
				_pos = reader.BaseStream.Position;  
			}
			Reindex ();
			return true;
		}

		public void Reindex ()
		{
			_len = 0L;
            //记录每个文件偏移量
			for (var i = 0; i < files.Count; i++) {
				var file = files [i]; //VFile文件
				file.offset = _pos + _len;
				_len += file.len;
			}
		} 

		public VFile GetFile (string path, string hash)
		{
			var key = Path.GetFileName (path);
			VFile file;
			_data.TryGetValue (key, out file);
			return file;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dataPath">res本地文件，Application.persistentDataPath</param>
		/// <param name="newFiles">更新过的文件信息</param>
		/// <param name="saveFiles"></param>
		public void Update(string dataPath, List<VFile> newFiles, List<VFile> saveFiles)
		{
			var dir = Path.GetDirectoryName(dataPath); 
			using (var stream = File.OpenRead(dataPath))
			{
				foreach (var item in saveFiles)
				{
					var path = string.Format("{0}/{1}", dir, item.name);
					if (File.Exists(path)) { continue; }  
					//文件不存在
					stream.Seek(item.offset, SeekOrigin.Begin); 
					using (var fs = File.OpenWrite(path))
					{
						//内容写入目标文件
						var count = 0L;
						var len = item.len;
						while (count < len)
						{
							var read = (int) Math.Min(len - count, _buffers.Length);
							stream.Read(_buffers, 0, read);
							fs.Write(_buffers, 0, read);
							count += read;
						}
					} 
					//写入过的文件列表
					newFiles.Add(item);
				}
			}

			if (File.Exists(dataPath))
			{
				File.Delete(dataPath);
			}
			//更新res文件信息
			using (var stream = File.OpenWrite (dataPath)) {
				var writer = new BinaryWriter (stream);
				writer.Write (newFiles.Count); //文件总数量
				foreach (var item in newFiles) {
					item.Serialize (writer); //文件信息
				}  
				foreach (var item in newFiles) {
					var path = string.Format("{0}/{1}", dir, item.name);
					WriteFile (path, writer); //文件内容
					File.Delete (path);
					Debug.Log ("Delete:" + path);
				} 
			} 
		}

		/// <summary>
		/// 把files存储的所有的VFile文件相关信息写入res文件中，并通过WriteFile把文件具体内容写到res文件中
		/// </summary>
		public void Save ()
		{
			//name = BuildScript.outputPath + "/" + "res"
			var dir = Path.GetDirectoryName (name);   
			using (var stream = File.OpenWrite (name)) {
				var writer = new BinaryWriter (stream);
				writer.Write (files.Count); //文件总数
				foreach (var item in files) {
					//把VFile对应的bundle名字，bundle大小，bundle内容hash值写入res文件中
					item.Serialize (writer);
				}  
				foreach (var item in files) {
					var path = dir + "/" + item.name;
					//把VFile对应的bundle文件的具体二进制内容写到res文件中
					WriteFile (path, writer);
				}
			} 
		}

		public void Clear ()
		{
			_data.Clear ();
			files.Clear ();
		}
	}
}