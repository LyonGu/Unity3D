using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Security.Cryptography;  //md5
using System.Text;

namespace HotUpdateModel
{
    public static class Helps
    {
        public static string GetMd5Values(string filePath)
        {
            StringBuilder sBu = new StringBuilder();
            filePath = filePath.Trim();
            using(FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] result = md5.ComputeHash(fs);
                for (int i = 0; i < result.Length; i++)
                {
                    sBu.Append(result[i].ToString("x2")); //byte[] 转成字符串，x2表示按照16进制，且为2位对齐输出
                }
            }
            return sBu.ToString();

        }
    }
}