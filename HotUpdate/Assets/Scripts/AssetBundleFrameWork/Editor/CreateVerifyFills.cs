
/* 创建项目的校验文件，针对各种资源进行标记 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using ABFW;


namespace HotUpdateModel
{
    public class CreateVerifyFiles
    {
        [MenuItem("AssetBundelTools/Create Verify Files")]
        public static void CreateVerifyMethod()
        {
            //AB的输出路径
            string abOutPath = PathTools.GetABOutPath();

            //校验文件路径
            string verifyFileOutPath = abOutPath + "/ProjectVerifyFile.txt";
            if(File.Exists(verifyFileOutPath))
            {
                File.Delete(verifyFileOutPath);
            }

            List<string> fileList = new List<string>();
            ListFiles(new DirectoryInfo(abOutPath), ref fileList);

            //md5写入校验文件
            WriteVerifyFile(verifyFileOutPath, abOutPath, fileList);

        }

        private static void ListFiles(FileSystemInfo fileSysInfo, ref List<string> fileList)
        {
            //文件系统转化成目录系统
            DirectoryInfo dirInfo = fileSysInfo as DirectoryInfo;

            //获取文件夹下所有信息，包括文件夹和文件
            FileSystemInfo[] fileSysInfos = dirInfo.GetFileSystemInfos();
            foreach (FileSystemInfo item in fileSysInfos)
            {
                FileInfo fileInfo = item as FileInfo;
                //如果是文件就写入集合
                if(fileInfo!=null)
                {  
                    //文件
                    string strFileFullName = fileInfo.FullName.Replace("\\","/");
                    //过滤无效文件
                    string fileExt = Path.GetExtension(strFileFullName);
                    if (fileExt.EndsWith(".meta") || fileExt.EndsWith(".bak"))
                    {
                        continue;
                    }

                    fileList.Add(strFileFullName);
                }
                else
                {
                    //目录, 递归调用
                    ListFiles(item, ref fileList);
                }
            }
        }

        private static void WriteVerifyFile(string path, string abOutPath, List<string> fileList)
        {
            using(FileStream fs = new FileStream(path, FileMode.CreateNew))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    for (int i = 0; i < fileList.Count; i++)
                    {
                        string strFile = fileList[i];
                        //生成md5值
                        string strMd5 = Helps.GetMd5Values(strFile); //todo

                        //路径保留相对路径
                        string realFilePath = strFile.Replace(abOutPath + "/", string.Empty);
                        sw.WriteLine(realFilePath + "|" + strMd5);

                    }
                }
            }

            Debug.Log("创建校验文件成功=====");
            AssetDatabase.Refresh();
        }


    }
}