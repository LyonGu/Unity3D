using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathTool {

	public const string AB_RESROOTPATH = "AB_Res";

	//得到AB资源的输入目录
	public static string GetABResourcesPath()
	{
		return Application.dataPath + "/" + AB_RESROOTPATH;
	}

	//获取AB输出路径
	public static string GetABOutPath()
	{
		return GetPlatformPath() + "/" + GetPlatformName();
	}

	/// 获取平台的路径
	private static string GetPlatformPath()
	{
		string strReturnPlatformPath = string.Empty;
		switch (Application.platform)
		{
			case RuntimePlatform.WindowsPlayer:
			case RuntimePlatform.WindowsEditor:
			case RuntimePlatform.OSXEditor:
				strReturnPlatformPath = Application.streamingAssetsPath;
				break;
			case RuntimePlatform.IPhonePlayer:
			case RuntimePlatform.Android:
				strReturnPlatformPath = Application.persistentDataPath;
				break;
			default:
				break;
		}

		return strReturnPlatformPath;
	}

	/// 获取平台的名称
	private static string GetPlatformName()
	{
		string strReturnPlatformName = string.Empty;

		switch (Application.platform)
		{
			case RuntimePlatform.WindowsPlayer:
			case RuntimePlatform.WindowsEditor:
				strReturnPlatformName = "Windows";
				break;
			case RuntimePlatform.IPhonePlayer:
				strReturnPlatformName = "Iphone";
				break;
			case RuntimePlatform.Android:
				strReturnPlatformName = "Android";
				break;
			case RuntimePlatform.OSXEditor:
				strReturnPlatformName = "MACOS";
				break;
			default:
				break;
		}

		return strReturnPlatformName;
	}



}
