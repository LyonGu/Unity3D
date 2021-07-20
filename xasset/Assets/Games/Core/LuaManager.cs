using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XLua;

namespace libx
{
    public class LuaManager
    {
        private static LuaEnv _env = null;
        private static LuaManager _instance = null;
        private readonly static string rootLuaPath = string.Format("{0}/DLC/Lua", Application.persistentDataPath);
        
        /// <summary>
        /// 把一个文件夹下所有文件复制到另一个文件夹下 
        /// </summary>
        /// <param name="sourceDirectory">源目录</param>
        /// <param name="targetDirectory">目标目录</param>
        public void DirectoryCopy(string sourceDirectory, string targetDirectory)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(sourceDirectory);
                //获取目录下（不包含子目录）的文件和子目录
                FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();
                foreach (FileSystemInfo i in fileinfo)
                {
                    if (i is DirectoryInfo)     //判断是否文件夹
                    {
                        if (!Directory.Exists(targetDirectory + "/"+ i.Name))
                        {
                            //目标目录下不存在此文件夹即创建子文件夹
                            Directory.CreateDirectory(targetDirectory + "/"+i.Name);
                        }
                        //递归调用复制子文件夹
                        DirectoryCopy(i.FullName, targetDirectory + "/"+ i.Name);
                    }
                    else
                    {
                        //不是文件夹即复制文件，true表示可以覆盖同名文件
                        var ext = Path.GetExtension (i.FullName);
                        if (ext != ".meta")
                        {
                            string soureFilePath = i.FullName.Replace("\\", "/");
                            string targetFilePath = targetDirectory + "/" + i.Name;
                            Debug.Log($"复制lua文件 {soureFilePath} To  {targetFilePath}");
                            File.Copy(soureFilePath, targetFilePath, true);
                        }

                        
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"复制文件出现异常:{ ex.Message}   sourceDirectory = {sourceDirectory}  targetDirectory = {targetDirectory}");
            }
        }
        
        public static LuaManager GetInstance()
        {
            if(_instance == null)
                _instance = new LuaManager();
            return _instance;
        }

        private LuaManager()
        {
            _env = new LuaEnv();
            _env.AddLoader(LuaLoader.Load);
//            if (!Directory.Exists(rootLuaPath))
//                Directory.CreateDirectory(rootLuaPath);
//            //把streamAssets下的lua文件拷贝到热更新文件
//            string localLuaDir = Application.streamingAssetsPath + "/Lua";
//            Debug.Log($"复制lua文件Dir {localLuaDir} To  {rootLuaPath}");
//            DirectoryCopy(localLuaDir, rootLuaPath);
        }

        public static void StartLua()
        {
            _env.DoString("require 'Main'");
        }


    }
}

