using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace libx
{
    public class LuaLoader
    {
    
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

            string updatePath = Assets.updatePath + "/Lua/" + filePath;
            string luaFilePath;
            //判断文件是否存在
            if(File.Exists(updatePath))
            {
                luaFilePath = updatePath;
            }
            else
            {
                string streamPath = Application.streamingAssetsPath+"/Lua/"+ filePath;
                if (!File.Exists(streamPath))
                {
                    Debug.LogError($"lua文件不存在：{streamPath}");
                    return byArrayReturn;
                }

                luaFilePath = streamPath;
            }
            string strLuaContent = File.ReadAllText(luaFilePath);
            byArrayReturn = System.Text.Encoding.UTF8.GetBytes(strLuaContent);
            return byArrayReturn;
 
        }

    }
}

