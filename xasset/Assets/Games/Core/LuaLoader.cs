using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace libx
{
    public class LuaLoader
    {
    
        private readonly static string rootLuaPath = string.Format("{0}/DLC/Lua/", Application.persistentDataPath);
        

        public static string GetStreamingAssetsPath()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                return Application.streamingAssetsPath;
            }

            if (Application.platform == RuntimePlatform.WindowsPlayer ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
//                return "file:///" + Application.streamingAssetsPath;
                return Application.streamingAssetsPath;
            }

            return "file://" + Application.streamingAssetsPath;
        }
    
        public static byte[] Load(ref string filePath)
        {
            filePath = filePath.Replace(".", "/") + ".lua";
            byte[] byArrayReturn = null; //返回数据
            string luaFilePath = rootLuaPath + filePath;
            string strLuaContent = File.ReadAllText(luaFilePath);
            byArrayReturn = System.Text.Encoding.UTF8.GetBytes(strLuaContent);
            return byArrayReturn;

        }

    }
}

