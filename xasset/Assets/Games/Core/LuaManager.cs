using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

namespace libx
{
    public class LuaManager
    {
        private static LuaEnv _env = null;
        private static LuaManager _instance = null;

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
        }

        public static void StartLua()
        {
            _env.DoString("require 'Main'");
        }


    }
}

