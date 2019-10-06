
//拷贝所有编辑区中的lua文件到发布区

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using ABFW;

namespace HotUpdateModel
{
    public class CopyLuaFile
    {
        private static string originLuaPath = Application.dataPath + "/LuaScript/src";
        private static string targetLuaPath = PathTools.GetABOutPath() + "/lua";

        [MenuItem("AssetBundelTools/Copy LuaFile To")]
        public static void CopyLuaFileTo()
        {
            DirectoryInfo dirInfo = new DirectoryInfo(originLuaPath);
            FileInfo[] fileInfos = dirInfo.GetFiles();

            if (!Directory.Exists(targetLuaPath))
            {
                Directory.CreateDirectory(targetLuaPath);
            }

            foreach (FileInfo item in fileInfos)
            {
                File.Copy(item.FullName, targetLuaPath + "/" + item.Name, true);
            }

            Debug.Log("lua文件拷贝完成====");
            AssetDatabase.Refresh();
        }
    }
}