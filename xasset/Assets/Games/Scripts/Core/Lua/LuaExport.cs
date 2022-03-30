//------------------------------------------------------------------------------
// Copyright (c) 2018-2019 Beijing Bytedance Technology Co., Ltd.
// All Right Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
//------------------------------------------------------------------------------
#define LuaOptimize
namespace Game
{
    using System;
    using UnityObject = UnityEngine.Object;

    /// <summary>
    /// Export the GameObject to the lua.
    /// </summary>
    [Serializable]
    public struct LuaExport
    {
        /// <summary>
        /// The name of the export object name.
        /// </summary>
#if LuaOptimize
        public string Name
        {
            get
            {
                if(Object!=null)
                    return Object.name;
                return string.Empty;
            }
        }
#else
        public string Name;
#endif


        /// <summary>
        /// The index of this field.
        /// </summary>
        public int Index;

        /// <summary>
        /// The exported GameObject to lua.
        /// </summary>
        public UnityObject Object;

    }
}
