using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using ABFW;
/*
	1 下载校验文件到客户端
	2 根据校验文件客户端逐条读取，跟本地的文件md5值进行对比
	3 如果客户端没有服务器上的文件，直接下载服务器端文件
	4 客户端存在相同文件，但是md5不同，说明文件有更新，客户端下载更新文件
 */

 namespace HotUpdateModel
 {
	public class UpdateFromServer : MonoBehaviour 
	{
		public bool enableSelf = true;  //是否启用

		//资源（包含assetBundle）的下载路径
		private string downPath = string.Empty;

		//http的请求地址
		private string  httpUrl = "http://127.0.0.1:8080/UpdateAssets";


		void Awake() {
			if (enableSelf)
			{
				downPath = PathTools.GetABOutPath();
				StartCoroutine(downLoadResAndCheck(httpUrl));
			}
			else
			{
				//不启用了更
				Debug.Log(GetType() + "/ 热更新禁用");
				//通知其他游戏主逻辑开始运行,固定写法
				BroadcastMessage("ReceiveInfoStartRuning",SendMessageOptions.DontRequireReceiver);
			}
		}

		IEnumerator downLoadResAndCheck(string url)
		{
			//1 下载校验文件到客户端
			if (string.IsNullOrEmpty(url))
			{
				yield break;
			}

			//服务器校验文件
			string verifyFileName = "ProjectVerifyFile.txt";
			string serverVerifyFile = url + "/" + verifyFileName;
			WWW www =  new WWW(serverVerifyFile);
			yield return www;

			if (www.error!=null && !string.IsNullOrEmpty(www.error))
			{
				Debug.Log("服务器校验文件下载失败=====error:" + www.error);
				yield break;
			}

			//客户端是否有下载目录
			if (!Directory.Exists(downPath))
			{
				Directory.CreateDirectory(downPath);
			}

			//写入本地,www下载完，数据会存在www.bytes里
			File.WriteAllBytes(downPath + "/" + verifyFileName, www.bytes);

			//2 根据校验文件客户端逐条读取，跟本地的文件md5值进行对比

			string strServerTxt = www.text;  //读取资源文件里的内容
			string[] lines = strServerTxt.Split('\n'); //换行截取
			for (int i = 0; i < lines.Length; i++)
			{
				if (string.IsNullOrEmpty(lines[i]))
				{
					continue;
				}

				string[] fileAndMd5 = lines[i].Split('|');
				string name = fileAndMd5[0].Trim();
				string md5 = fileAndMd5[1].Trim();

				//本地文件
				string clientFile = downPath + "/" + name;

				//3 如果客户端没有服务器上的文件，直接下载服务器端文件
				if (!File.Exists(clientFile))
				{
					//创建本地不存在的文件夹
					string dir = Path.GetDirectoryName(clientFile);
					if (!string.IsNullOrEmpty(dir))
					{
						Directory.CreateDirectory(dir);
					}

					//通过www,开始正式下载服务器端的文件，且写入本地
					yield return  StartCoroutine(downLoadFile(httpUrl + "/" + name, clientFile));	
				}
				else
				{
					//4 客户端存在相同文件，但是md5不同，说明文件有更新，客户端下载更新文件

					//得到本地文件的md5
					string clientMd5 = Helps.GetMd5Values(clientFile);
					if (!clientMd5.Equals(md5))
					{
						File.Delete(clientFile);
						yield return  StartCoroutine(downLoadFile(httpUrl + "/" + name, clientFile));	
						Debug.Log("更新文件完成===="+ name);
					}
				}
				
				

			}
			
			yield return new WaitForEndOfFrame();
			Debug.Log("热更完成====");

			//通知其他游戏主逻辑开始运行,固定写法
			BroadcastMessage("ReceiveInfoStartRuning",SendMessageOptions.DontRequireReceiver);
		}

		IEnumerator downLoadFile(string url, string localPath)
		{
			WWW www = new WWW(url);
			yield return www;
			if (www.error!=null && !string.IsNullOrEmpty(www.error))
			{
				Debug.Log("文件下载失败=====error:" + www.error + "/" + localPath);
				yield break;
			}
            Debug.Log("下载成功=====" + localPath);
			File.WriteAllBytes(localPath, www.bytes);
			yield return null;
		}
	}
 }
