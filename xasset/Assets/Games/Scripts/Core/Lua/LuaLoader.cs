using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using libx;
using UnityEngine;

namespace Game
{
    public class LuaLoader
    {
    
        private readonly static string rootLuaPath = string.Format("{0}/DLC/Lua/", Application.persistentDataPath);
        private readonly static string developPath = Application.dataPath;

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
#if UNITY_EDITOR
            //Editor下
            filePath = LuaManager.GetInstance().GetFilePath(filePath);
            string strLuaContent = File.ReadAllText(filePath);
            byte[] byArrayReturn = System.Text.Encoding.UTF8.GetBytes(strLuaContent);
            return byArrayReturn;
#endif
           
            filePath = Assets.GetAssetPathByName(filePath);
            var luarequest = Assets.LoadAsset(filePath, typeof(TextAsset));
            TextAsset asset = luarequest.asset as TextAsset;
            return asset.bytes;

        }

    }
}

