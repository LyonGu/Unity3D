using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using GameLog;
using UnityEngine;
using XLua;

namespace libx
{
    public class LuaManager
    {
        private LuaEnv _env = null;
        private static LuaManager _instance = null;
        private readonly static string rootLuaPath = string.Format("{0}/DLC/Lua", Application.persistentDataPath);
        #if UNITY_EDITOR
            public static Dictionary<string, string> luaName2Path =  new Dictionary<string, string>(200);
        #endif

        public  LuaEnv Env
        {
            get { return _env; }
        }

        public void Init()
        {
#if UNITY_EDITOR
            initPathData();
#endif
        }

        public static LuaManager GetInstance()
        {
            if(_instance == null)
                _instance = new LuaManager();
            return _instance;
        }
#if UNITY_EDITOR
        private void initPathData()
        {
            string rootLuaPath = string.Format("{0}/Games/Lua", Application.dataPath);
            
            //递归所有的文件夹
            var files = Directory.GetFiles(rootLuaPath, "*.lua", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (Directory.Exists(file)) continue;
                var asset = file.Replace("\\", "/");
                string flieName = Path.GetFileNameWithoutExtension(asset);
                //Debug.Log($"LuA initPathData flieName = {flieName} asset = {asset}");
                if(!luaName2Path.ContainsKey(flieName))
                    luaName2Path.Add(flieName, asset);
            }
        }

        public string GetFilePath(string name)
        {
            if (luaName2Path.TryGetValue(name, out string filePath))
                return filePath;
            return string.Empty;
        }
#endif
        private LuaManager()
        {
            _env = new LuaEnv();
            _env.AddLoader(LuaLoader.Load);

            _env.AddBuildin("rapidjson", XLua.LuaDLL.Lua.LoadRapidJson);
            _env.AddBuildin("cjson", XLua.LuaDLL.Lua.LoadcJson);
        }

        public void StartLua()
        {
            _env.DoString("require 'Main'");
        }

        public void Stop()
        {
            if (_env != null)
            {
                // GC and clear first.
                _env.FullGc();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                _env.Tick();

                // Check the unreleased reference.
                _env.DoString(
                    "require('util').print_func_ref_by_csharp()");

                // Dispose the lua environment.
                try
                {
                    _env.Dispose();
                }
                catch (Exception e)
                {
                    LogUtils.Error("Dispose the lua environment failed. " + e.ToString());
                }
                finally
                {
                    _env = null;
                }
            }
        }


    }
}

