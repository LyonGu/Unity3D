
#define LuaOptimize
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;
using GamePool;
using libx;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using XLua;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Game
{
    /// <summary>
    /// The lua behavior is used to associate the lua class and GameObject.
    /// </summary>
    [ExecuteInEditMode]
    public sealed class LuaBehavior : MonoBehaviour
    {
        
        [BlackList]
        [SerializeField][Tooltip("The export object that lua need access.")]
        public LuaExport[] exports;

        private LuaTable instance;
        private Dictionary<string, LuaTable> instanceDic = DictionaryPool<string, LuaTable>.Get();
        private Action onEnable;
        private Action onDisable;

        private Action update;

        //private Action fixedUpdate;
        private Action lateUpdate;
        
        static Dictionary<string, Dictionary<string, int>> _ComObjectsNameIndexDic = new Dictionary<string, Dictionary<string, int>>();
        
#if UNITY_EDITOR
        private Action onDrawGizmo;
        [BlackList] public bool exeInEditor { get; set; }
#endif
        
        [BlackList]
        public static Dictionary<int, List<MonoBehaviour>> dicMonoBehavior = new Dictionary<int, List<MonoBehaviour>>();

        //是否绑定lua层update回调，true代表跟随Active变化而删除添加
        private bool _isBindeLuaUpdate = true;
        [BlackList]
        public Action<bool> OnOutOrInCamera = null;

        [BlackList]
        public static void AddMonoBehevior(int instanceId, MonoBehaviour behaviour)
        {
            if (dicMonoBehavior.TryGetValue(instanceId, out var lm))
            {
                lm.Add(behaviour);
            }
            else
            {
                List<MonoBehaviour> lM = ListPool<MonoBehaviour>.Get();
                lM.Add(behaviour);
                dicMonoBehavior.Add(instanceId, lM);
            }
        }

        [BlackList]
        public static void RemoveMonoBehevior(int instanceId)
        {
            if (dicMonoBehavior.TryGetValue(instanceId, out var lm))
            {
                lm.Clear();
                ListPool<MonoBehaviour>.Release(lm);
                dicMonoBehavior.Remove(instanceId);
            }
        }

        [BlackList]
        public static void RemoveMonoBehevior(int instanceId, MonoBehaviour behaviour)
        {
            if (dicMonoBehavior.TryGetValue(instanceId, out var lm))
            {
                lm.RemoveBySwap(behaviour);
                if(lm.Count == 0)
                {
                    ListPool<MonoBehaviour>.Release(lm);
                    dicMonoBehavior.Remove(instanceId);
                }
            }
        }

        public static void EnableMonobehavior(int instanceId)
        {
            if (dicMonoBehavior.TryGetValue(instanceId, out var lm))
            {
                foreach (var item in lm)
                {
                    item.enabled = true;
                    //把激活的字节点的enable也开下
                    List<MonoBehaviour> list = ListPool<MonoBehaviour>.Get();
                    item.gameObject.GetComponentsInChildren<MonoBehaviour>(false, list);
                    if (list.Count > 0)
                    {
                        foreach (var monoBehaviour in list)
                        {
                            monoBehaviour.enabled = true;

                        }
                    }
                    ListPool<MonoBehaviour>.Release(list);
                }
            }
        }

        public static void DissableMonobehavior(int instanceId)
        {
            if (dicMonoBehavior.TryGetValue(instanceId, out var lm))
            {
                foreach (var item in lm)
                {
                    item.enabled = false;
                    //把激活的字节点的enable也关闭下
                    List<MonoBehaviour> list = ListPool<MonoBehaviour>.Get();
                    item.gameObject.GetComponentsInChildren<MonoBehaviour>(false, list);
                    if (list.Count > 0)
                    {
                        foreach (var monoBehaviour in list)
                        {
                            monoBehaviour.enabled = false;

                        }
                    }
                    ListPool<MonoBehaviour>.Release(list);
                }
            }
        }

        /// <summary>
        /// Gets the lua table.
        /// </summary>
        public LuaTable Instance
        {
            get
            {
                LuaTable luatable = null;
                foreach (var item in instanceDic)
                {
                    luatable = item.Value;
                    break;
                }

                return luatable;
            }
        }

        public LuaTable GetLuaTable(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            if (instanceDic.TryGetValue(name, out LuaTable luaInstance))
            {
                return luaInstance;
            }
            return null;
        }


        private void InvokeMethodByLuatable(LuaTable luaTable, string method)
        {
            var instance = luaTable;
            if (instance == null)
            {
                return;
            }
            var func = instance.Get<UnityAction<LuaTable>>(method);
            if (func != null)
            {
                func(instance);
            }

            else
            {
//                Logger.Error(
//                    this,
//                    "Can not find lua method: {0} for view {1}",
//                    method,
//                    this.name);
            }
        }

        [BlackList]
        /// <summary>
        /// Invoke the member method in lua.
        /// </summary>
        public void InvokeMethod(string method)
        {
            // foreach (var item in instanceDic)
            // {
            //     var instance = item.Value;
            //     this.InvokeMethodByLuatable(instance, method);
            // }

//            XDebug.LogWarning($"错误的使用方式={method}");
            var list = ListPool<LuaTable>.Get();
            list.AddRange(instanceDic.Values);
            for (int i = 0; i < list.Count; i++)
            {
                this.InvokeMethodByLuatable(list[i], method);
            }
            ListPool<LuaTable>.Release(list);
        }

        //add by hxp

        private Dictionary<string, Object> nameMap;
        public Object GetObjectByName(string name, out bool activeSelf, out int InstanceID, out float sx)
        {

            Object obj = null;
            if (nameMap == null)
                nameMap = DictionaryPool<string,Object>.Get();
            if (nameMap.Count == 0)
            {
                for (int i = 0; i < exports.Length; i++)
                {
                    var eobj = exports[i].Object;
                    if (eobj != null)
                    {
                        nameMap.Add(eobj.name, eobj);
                        if (eobj.name == name)
                        {
                            obj = eobj;
                        }
                    }

                }
            }
            else
            {
                if (nameMap.TryGetValue(name, out var objt))
                {
                    obj = objt;
                }
            }
            if(obj == null)
            {
                activeSelf = false;
                InstanceID = -1;
                sx = 0;
                return obj;
            }
            var gOject = obj as GameObject;
            activeSelf = gOject.activeSelf;
            InstanceID = gOject.GetInstanceID();
            sx = gOject.transform.localScale.x;
            return obj;

        }
        
        public Object GetObjectByIndex(int index, out bool activeSelf, out int InstanceID, out float sx)
        {
            int length = exports.Length;
            if (index > length - 1)
            {
                activeSelf = false;
                InstanceID = -1;
                sx = 0;
                return null;
            }
            var obj = exports[index].Object;
            if (obj == null)
            {
                activeSelf = false;
                InstanceID = -1;
                sx = 0;
                return null;
            }
            GameObject GObj = obj as GameObject;
            activeSelf = GObj.activeSelf;
            InstanceID = GObj.GetInstanceID();
            sx = GObj.transform.localScale.x;
            return obj;

        }

        public GameObject GetGameObject(out bool activeSelf, out int InstanceID, out string name)
        {
            GameObject obj = this.gameObject;
            activeSelf = obj.activeSelf;
            InstanceID = obj.GetInstanceID();
            name = obj.name;
            return obj;
        }
        
        public void ResetInstance()
        {

            if (instanceDic.Count > 0)
            {
                foreach (var item in instanceDic)
                {
                    var instance = item.Value;
                    if (instance != null)
                    {
                        instance.Dispose();
                        instance = null;
                    }
                }
                instanceDic.Clear();
            }
        }

        public void SetInstance(LuaTable ins)

        {
            
            string tableName;
            ins.Get<string, string>("_name", out tableName);
            tableName = string.Intern(tableName);
            if (!instanceDic.ContainsKey(tableName))
            {
                instanceDic.Add(tableName, ins);
            }
            
        }
        
  
        /// <summary>
        /// Release all lua part instance.
        /// </summary>
        public void Release()
        {
            
            // Release all lua reference.
            this.onEnable = null;
            this.onDisable = null;
            this.update = null;
            //this.fixedUpdate = null;
            this.lateUpdate = null;
#if UNITY_EDITOR
            this.onDrawGizmo = null;
#endif
            // Logger.Info(this, "free object==============.");
            // Delete the lua instance.

            var Instance = LuaManager.GetInstance();
            if (Instance.Env != null)
            {
                if (instanceDic.Count > 0)
                {
                    List<KeyValuePair<string, LuaTable>> temp = ListPool<KeyValuePair<string, LuaTable>>.Get();
                    temp.AddRange(this.instanceDic);
                    foreach (var item in temp)
                    {
                        var instance = item.Value;
                        if (instance != null)
                        {
                            using (var func = instance.Get<LuaFunction>("Delete"))
                            {
                                func?.Call(instance);
                            }
                            instance.Dispose();
                        }
                    }
                    ListPool<KeyValuePair<string, LuaTable>>.Release(temp);
                    instanceDic.Clear();

                }
            }

        }

#if CLIENT_RUNTIME

        public static void ClearAll()
        {
#if LuaOptimize
            // _ComObjectsNameIndexDic.Clear();
#endif
            LuaManager.Instance.UnRegisterAllClickFunc();
        }
#endif

        private string _gObjectName;

        private void SetGameObjectName()
        {
            string gName = string.Intern(gameObject.name);
            gName = gName.Replace("(Clone)", string.Empty);
            gameObject.name = gName;
            _gObjectName = gName;

        }

        private bool _IsCreateObjectCachDone;
        private void CreateObjectCache()
        {

            SetGameObjectName();
            if (!_ComObjectsNameIndexDic.ContainsKey(_gObjectName))
            {
               Dictionary<string, int> indexDic = new Dictionary<string, int>();
               _ComObjectsNameIndexDic.Add(_gObjectName, indexDic);
                if (this.exports != null && this.exports.Length > 0)
                {
                    int count = exports.Length;
                    for (int i = 0; i < count; ++i)
                    {
                        var export = this.exports[i];
                        var obj = export.Object;
                        if (obj != null)
                        {
                            string _name = string.Intern(obj.name);
                            if(!indexDic.ContainsKey(_name))
                                indexDic.Add(_name, i);
                        }
                    }
                }
            }


            _IsCreateObjectCachDone = true;


        }

        private void OnDestroy()
        {
            if (nameMap != null)
            {
                nameMap.Clear();
                DictionaryPool<string,Object>.Release(nameMap);
                nameMap = null;
            }


            var behaviours = ListPool<UIBehaviour>.Get();
            gameObject.GetComponentsInChildren<UIBehaviour>(true, behaviours);
            foreach (var behaviour in behaviours)
            {
                switch (behaviour)
                {
                case Button b:
                    b.onClick.RemoveAllListeners();
                    break;
                case Toggle t:
                    t.onValueChanged.RemoveAllListeners();
                    break;
                case Slider s:
                    s.onValueChanged.RemoveAllListeners();
                    break;
                case InputField i:
                    i.onValueChanged.RemoveAllListeners();
                    break;
                }
            }
            ListPool<UIBehaviour>.Release(behaviours);

            this.Release();
        }

        void Awake()
        {
            // if (module != null)
            //     this.module = string.Intern(this.module);

        }
        private void Start()
        {
            
        }

        private void OnEnable()
        {
            
#if UNITY_EDITOR
         m_EditorLuaBehaviorList.Add(this);
#endif
        }

        private void OnDisable()
        {
 
#if UNITY_EDITOR
            m_EditorLuaBehaviorList.Remove(this);
#endif
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            try
            {
                this.onDrawGizmo?.Invoke();
            } catch(Exception e)
            {
//                Logger.Error(this, e, "OnDrawGizmos event occur exception.");
            }
        }
#endif

#if UNITY_EDITOR

        #if CLIENT_RUNTIME
        [BlackList]
        #endif
        public static void CreateLuabehaviorCfg(GameObject gameObject, LuaExport[] exports, bool refresh = true)
        {
            string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);
            // string DirPath = Path.GetDirectoryName(prefabPath);
            string fileName = Path.GetFileNameWithoutExtension(prefabPath).ToLower();


            bool isSoftLink= false;
            string linkFold = Application.dataPath + "/_LinkRes";
            if(Directory.Exists(linkFold))
            {
                isSoftLink = true;
            }
            string DirPath = Application.dataPath + (isSoftLink? "/_LinkRes/Data/View/LuabehaviorCfg":"/_Data/View/LuabehaviorCfg");
            DirPath = DirPath.Replace("\\", "/");
            if(!Directory.Exists(DirPath))
            {
                Directory.CreateDirectory(DirPath);
            }

            string targetFilePath = $"{DirPath}/{fileName}_vars.txt";
            Debug.Log($"targetFilePath=============={targetFilePath}");

            if (exports != null && exports.Length > 0)
            {
                string keyName = fileName;
                List<string> lines_single = new List<string>();
                lines_single.Add("local "+keyName + " = {");
                for (int m = 0; m < exports.Length; m++)
                {
                    var export = exports[m];
                    var cname = export.Name;
                    if (cname != string.Empty)
                    {
                        lines_single.Add($"    ['{cname}'] = {m},");
                    }
                }

                lines_single.Add(" }");
                lines_single.Add(" ");
                lines_single.Add($"LuabehaviorNameConstantEx['{keyName}'] = {keyName}");

                File.WriteAllLines(targetFilePath, lines_single, new System.Text.UTF8Encoding(false));

                if(refresh)
                {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }

            }
            else
            {
                if(File.Exists(targetFilePath))
                {
                    File.Delete(targetFilePath);
                }
            }

        }
        #if CLIENT_RUNTIME
        [BlackList]
        #endif
        public void CopeExportLuaCode()
        {
            var sb = new StringBuilder();
            try
            {
                var existArrays = new HashSet<string>();
                foreach (var export in this.exports)
                {
                    if (export.Index == 0)
                    {
                        sb.Append("self.");
                        sb.Append(export.Name);
                        sb.Append(" = self:GetObject('");
                        sb.Append(export.Name);
                        sb.Append("')\n");
                    }
                    else
                    {
                        //if (!existArrays.Contains(export.Name))
                        //{
                        //    existArrays.Add(export.Name);
                        //    sb.Append("for k,v in pairs(self.");
                        //    sb.Append(export.Name);
                        //    sb.Append(") do self.");
                        //    sb.Append(export.Name);
                        //    sb.Append("[k] = U3DObject(v) end\n");
                        //}
                    }
                }
                var outputStr = sb.ToString();
                GUIUtility.systemCopyBuffer = outputStr;
                Debug.Log(outputStr);
            }
            finally
            {
                
            }
        }

#if CLIENT_RUNTIME
        [BlackList]
#endif
        public void CreateLuaViewFile(string rtargetPath, string rfileName, string author, bool isCellView, bool isModalView)
        {
            string fileName = "DL" + gameObject.name;
            if (!string.IsNullOrEmpty(rfileName))
                fileName = rfileName;
            const string saveFolderPath = "Assets/Game/Lua/Game/View/";
            string targetPath = Path.Combine(Application.dataPath, saveFolderPath.Replace("Assets/", ""));
            if (!string.IsNullOrEmpty(rtargetPath))
                targetPath = rtargetPath;
            targetPath = targetPath + fileName + ".lua";

            List<string> lines = new List<string>();
            if (!string.IsNullOrEmpty(author))
            {
                lines.Add($"--[[\r\n    Filename: {fileName} \r\n    Author: {author} \r\n    Time: {System.DateTime.Now} \r\n    Description: \r\n--]]\r\n");
            }


            lines.Add("local GameEvents = require('GameEvents')");
            lines.Add("local UIUtils = UIUtils");
            string classStr = string.Format("local {0} = BaseUIView.CreateDLView('{1}')", fileName, fileName);
            if (isCellView)
            {
                classStr = string.Format("local {0} = BaseCellView.CreateCellView('{1}')", fileName, fileName);
            }
            else if (isModalView)
            {
                classStr = string.Format("local {0} = BaseModalUIView.CreateDLView('{1}')", fileName, fileName);
            }
            lines.Add(classStr);
            lines.Add("");

            //_Init
            string _InitFuncStr = "";
            if (gameObject.name.Contains("RenderUIView"))
            {
                _InitFuncStr = string.Format("function {0}:_Init() \n\n", fileName);
                _InitFuncStr = _InitFuncStr + "\tself._viewType = ViewInstanceType.multy \n\nend";
            }
            else
            {
                _InitFuncStr = string.Format("function {0}:_Init() \n\n\nend", fileName);
            }

            lines.Add(_InitFuncStr);
            lines.Add("");

            //InitData
            string InitDataFunStr = string.Format("function {0}:InitData() \n\n\nend", fileName);
            lines.Add(InitDataFunStr);
            lines.Add("");

            //OnCreate
            string OnCreateFunStr = string.Format("function {0}:OnCreate() \n\n\tself:LoadResAsync('{1}') \n\nend",
                fileName, gameObject.name);

            lines.Add(OnCreateFunStr);
            lines.Add("");

            //OnView
            string OnViewFunStr = string.Format("function {0}:OnView() \n", fileName);

            //获取组件
            var sb = new StringBuilder();;
            try

            {
                var existArrays = new HashSet<string>();
                foreach (var export in this.exports)
                {
                    if (export.Index == 0)
                    {
                        sb.Append("\tself.");
                        sb.Append(export.Name);
                        sb.Append(" = self:GetObject('");
                        sb.Append(export.Name);
                        sb.Append("')\n");
                    }
                }
            }
            finally
            {
                
            }

            //按钮事件注册
            //AddBtnEvents
            string AddBtnEventsFunStr = string.Format("function {0}:AddBtnEvents() \n", fileName);
            List<string> BtnEventsStr = new List<string>();

            List<string> BtnEventsFuncStr = new List<string>();
            foreach (var export in this.exports)
            {
                if (export.Index == 0)
                {
                    if (export.Name.Contains("Btn") || export.Name.Contains("btn"))
                    {
                        //注册按钮事件
                        string eStr =
                            string.Format(
                                "\n\tself:AddUnityLinster(UnityEventType.Btn, self.{0}, function(obj)\n \t\tself:OnClick{1}(obj)\n\tend)\n",
                                export.Name, export.Name);
                        BtnEventsStr.Add(eStr);
                        string funcStr = string.Format("function {0}:OnClick{1}(obj) \n\n\nend", fileName, export.Name);
                        BtnEventsFuncStr.Add(funcStr);
                    }
                }
            }

            OnViewFunStr = OnViewFunStr + "\n" + sb.ToString() + "\n";
            if (BtnEventsFuncStr.Count > 0)
            {
                OnViewFunStr = OnViewFunStr + "\n\tself:AddBtnEvents()\n\nend";
            }
            else
            {
                OnViewFunStr = OnViewFunStr + "end";
            }

            lines.Add(OnViewFunStr);
            lines.Add("");

            //OnEvents
            string OnEventsFunStr = string.Format("function {0}:OnEvents() \n\n\nend", fileName);
            lines.Add(OnEventsFunStr);
            lines.Add("");



            //OnShow
            string OnShowFunStr = string.Format("function {0}:OnShow() \n", fileName);
            OnShowFunStr = OnShowFunStr + "\n\n\nend";
            lines.Add(OnShowFunStr);
            lines.Add("");
            if (BtnEventsFuncStr.Count > 0)
            {
                for (int i = 0; i < BtnEventsStr.Count; i++)
                {
                    AddBtnEventsFunStr = AddBtnEventsFunStr + BtnEventsStr[i];
                }

                AddBtnEventsFunStr = AddBtnEventsFunStr + "\nend";
                lines.Add(AddBtnEventsFunStr);
                lines.Add("");

                for (int i = 0; i < BtnEventsFuncStr.Count; i++)
                {
                    lines.Add(BtnEventsFuncStr[i]);
                    lines.Add("");
                }
            }

            //OnColse
            string OnColseFunStr = string.Format("function {0}:OnClose() \n\n\nend", fileName);
            lines.Add(OnColseFunStr);
            lines.Add("");
            ;
            lines.Add("");
            string returnStr = string.Format("return {0}", fileName);
            lines.Add(returnStr);
            File.WriteAllLines(targetPath, lines, new UTF8Encoding(false));
#if CLIENT_RUNTIME
            DXGame.XDebug.Log(targetPath);
#else
            Debug.Log(targetPath);
#endif
        }
#endif

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void load()
        {
            UnityEditor.EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
            PrefabUtility.prefabInstanceUpdated -= OnPrefabInstanceUpdated;
            PrefabUtility.prefabInstanceUpdated += OnPrefabInstanceUpdated;
        }


        static void OnPrefabInstanceUpdated(GameObject instance)
        {

            Debug.Log($"OnPrefabInstanceUpdated instance ======{instance.name}");
            LuaBehavior t = instance.GetComponent<LuaBehavior>();
            if (t != null)
            {
                bool isHaveEmptyObject = false;
                var exports = t.exports;
                for (int i = 0; i < exports.Length; i++)
                {
                    if (exports[i].Object == null)
                    {
                        isHaveEmptyObject = true;
                        break;

                    }
                }
                if(isHaveEmptyObject)
                {
                    EditorUtility.DisplayDialog("删除组件提示",
                        "LuaBehavior的绑定列表exports里不能有空对象,请跟对应程序确定后再删除", "确定");
                }
                else
                {
                    CreateLuabehaviorCfg(instance, exports);
                }
            }

        }

        private readonly static List<LuaBehavior> m_EditorLuaBehaviorList = new List<LuaBehavior>();

#if CLIENT_RUNTIME
        [BlackList]
#endif
        public static int GetAltasCount(GameObject obj)
        {
            if(obj == null)
                return 0;
            // 《路径名，《图集名，是否存在》》
            string resName = obj.name;
            int atalsCount = 0;
            Dictionary<string, Dictionary<string, bool>> altasMap = new Dictionary<string, Dictionary<string, bool>>(300);
            Image[] Images = obj.GetComponentsInChildren<Image>(true);
            if (Images.Length > 0)
            {

                for (int j = 0; j < Images.Length; j++)
                {
                    Image img = Images[j];
                    Sprite sp = img.sprite;
                    if(sp!=null)
                    {
                        //编辑器模式下引用的时单图
                        if(!Application.isPlaying)
                        {
                            string spritePath = AssetDatabase.GetAssetPath(sp.GetInstanceID());
                            if(spritePath.IndexOf("Assets/_LinkRes/UI") !=-1 || spritePath.IndexOf("Assets/_UGUI") !=-1)
                            {
                                //获取当前目录名称

                                string DirPath = Path.GetDirectoryName(spritePath);
                                DirPath = DirPath.Replace("\\","/");
                                int ix = DirPath.LastIndexOf("/");
                                string DirName = DirPath.Substring(ix+1);
                                string atlasName = DirName.ToLower();
                                // Debug.Log($"LookUpNodeCountAndAltasCount  Finish===== {DirName}&&  {spritePath}");
                                if(!altasMap.ContainsKey(resName))
                                {
                                    Dictionary<string, bool> altasD = new Dictionary<string, bool>();
                                    altasMap.Add(resName, altasD);
                                    if(!altasD.ContainsKey(atlasName))
                                    {
                                        altasD.Add(atlasName, true);
                                    }
                                }
                                else
                                {
                                    Dictionary<string, bool> altasD = altasMap[resName];
                                    if(!altasD.ContainsKey(atlasName))
                                    {

                                        altasD.Add(atlasName, true);
                                    }
                                }
                            }


                        }
                        else
                        {
                            if(sp.packed)
                            {
                                string atlasName = sp.texture.name;
                                if(!altasMap.ContainsKey(resName))
                                {
                                    Dictionary<string, bool> altasD = new Dictionary<string, bool>();
                                    altasMap.Add(resName, altasD);
                                    if(!altasD.ContainsKey(atlasName))
                                    {
                                        altasD.Add(atlasName, true);
                                    }
                                }
                                else
                                {
                                    Dictionary<string, bool> altasD = altasMap[resName];
                                    if(!altasD.ContainsKey(atlasName))
                                    {

                                        altasD.Add(atlasName, true);
                                    }
                                }
                            }

                        }

                    }

                }
            }

            if(altasMap.ContainsKey(resName))
            {
                atalsCount = altasMap[resName].Count;

            }
            return atalsCount;
        }

        private static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            var obj = UnityEditor.EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (obj == null)
            {
                return;
            }
            //判断obj上是否挂了luabehvior
            LuaBehavior luabavior = obj.GetComponent<LuaBehavior>();
            if(luabavior!=null)
            {
                var r = new Rect(selectionRect);
                r.x = r.width - 30;
                r.width = 80;
                int totalNodeCount = obj.GetComponentsInChildren<Transform>(true).Length;
                GUI.Label(r, totalNodeCount.ToString());

                int altasCount = GetAltasCount(obj);
                r = new Rect(selectionRect);
                r.x = r.width;
                r.width = 80;
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.yellow;
                style.active.textColor = Color.red;
                if (style != null)
                {
                    GUI.Label(r, altasCount.ToString(), style);
                }
            }

            foreach (var c in m_EditorLuaBehaviorList)
            {
                LuaExport[] exports = c.exports;
                if (exports != null)
                {
                    bool isUse = false;
                    for (int i = 0; i < exports.Length; i++)
                    {
                        if (obj == exports[i].Object)
                        {
                            isUse = true;
                            break;
                        }
                    }
                    if (isUse)
                    {
                        var r = new Rect(selectionRect);
                        r.x = 34;
                        r.width = 80;
                        GUIStyle style = new GUIStyle();
                        style.normal.textColor = Color.yellow;
                        style.active.textColor = Color.red;
                        if (style != null && obj != null)
                        {
                            GUI.Label(r, "★", style);
                        }
                    }
                }


            }
        }

#endif

    }
}
