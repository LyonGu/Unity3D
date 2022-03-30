//------------------------------------------------------------------------------
// Copyright (c) 2018-2019 Beijing Bytedance Technology Co., Ltd.
// All Right Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
//------------------------------------------------------------------------------
#define LuaOptimize
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game
{
    /// <summary>
    /// The editor for <see cref="LuaBehavior"/>.
    /// </summary>
    [CustomEditor(typeof(LuaBehavior))]
    public sealed class LuaBehaviorEditor : UnityEditor.Editor
    {
        enum BindType
        {
            Normal,
            UI,
            UIRender
        }

        private SerializedProperty module;
        private SerializedProperty exports;
#if CLIENT_RUNTIME
        private SerializedProperty executeInEditMode;
#endif
        private ReorderableList exportList;

        private AnimBool tipsToggle;
        private double tipsStartTime;
        private float tipsDuration;
        private string tips;

        private BindType _bindType;
        private Transform _root;


        private HashSet<Object> _checkRefSet = new HashSet<Object>();
        private HashSet<string> _checkNameSet = new HashSet<string>();

        private GUIStyle GuistyleBoxDND;
        private bool openDragBox;

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();
            this._checkRefSet.Clear();
            _checkNameSet.Clear();
            LuaBehavior behavior = this.target as LuaBehavior;
            #if CLIENT_RUNTIME
            behavior.exeInEditor = _bindType == BindType.Normal;
#endif

            // if (_bindType == BindType.UI)
            // {
            //     _root.anchorMax = Vector2.one;
            //     _root.anchorMin = Vector2.zero;
            //     _root.anchoredPosition = Vector2.zero;
            //     _root.sizeDelta = Vector2.zero;
            // }
                DrawDragAndDropBox();
            // EditorGUILayout.BeginHorizontal();
            // EditorGUILayout.PrefixLabel("Module");

            // if (_bindType == BindType.Normal)
            // {
            //     // var path = this.module.stringValue;
            //     // var existed = IsExisted(path);
            //     // var defaultColor = GUI.color;
            //     // if (!existed)
            //     // {
            //     //     GUI.color = Color.red;
            //     // }

            //     // this.module.stringValue = EditorGUILayout.TextField(path);
            //     // GUI.color = defaultColor;

            //     // GUI.enabled = existed;
            //     // if (GUILayout.Button("Open"))
            //     // {
            //     //     OpenModule(path);
            //     // }

            //     // GUI.enabled = true;
            // }
            // else
            // {
            //     // string modlueName = "DL" + _root.name;
            //     // if (module.stringValue != modlueName)
            //     // {
            //     //     module.stringValue = modlueName;
            //     // }

            //     // EditorGUILayout.TextField(module.stringValue);
            // }

            // EditorGUILayout.EndHorizontal();


            this.exportList.DoLayoutList();
#if CLIENT_RUNTIME
            if (this._bindType == BindType.Normal)
                EditorGUILayout.PropertyField(this.executeInEditMode);
#endif
			this.serializedObject.ApplyModifiedProperties();

            if (this._bindType == BindType.Normal)
            {
                this.tipsToggle.target = !string.IsNullOrEmpty(this.tips);
                if (EditorGUILayout.BeginFadeGroup(this.tipsToggle.faded))
                {
                    EditorGUILayout.HelpBox(this.tips, MessageType.Error);
                }

                EditorGUILayout.EndFadeGroup();
            }
#if CLIENT_RUNTIME
            if (GUILayout.Button("Copy export lua code"))
            {
                behavior.CopeExportLuaCode();
            }

            if (GUILayout.Button("Create Lua View File"))
            {
                MVCTools.ShowWindow();
            }
#endif

            //
            if (GUILayout.Button("Create Luabehavior Cfg"))
            {
                GameObject obj = behavior.gameObject;
                LuaExport[] exports = behavior.exports;
                LuaBehavior.CreateLuabehaviorCfg(obj, exports);
            }
        }

        private static bool IsExisted(string path)
        {
#if !CLIENT_RUNTIME
			return true;
#else
            if (!path.EndsWith(".lua", StringComparison.Ordinal))
            {
                path += ".lua";
            }
            var luaPath = Path.Combine(LuaConst.LuaScriptsFolder, path);
            return File.Exists(luaPath);
#endif
        }

        private static void OpenModule(string path)
        {
#if CLIENT_RUNTIME
            if (!path.EndsWith(".lua", StringComparison.Ordinal))
            {
                path += ".lua";
            }

            var luaPath = Path.Combine(LuaConst.LuaScriptsFolder, path);
            if (!LuaEditor.OpenText(luaPath, 0, 0))
            {
                Application.OpenURL(luaPath);
            }
#endif
        }

        private void OnEnable()
        {
            if (this.target == null)
            {
                return;
            }


            var serObj = this.serializedObject;
            var n = this.target.name;

            if (n.EndsWith("RenderUIView"))
                this._bindType = BindType.UIRender;
            else if (n.EndsWith("UIView"))
                this._bindType = BindType.UI;
            else
                this._bindType = BindType.Normal;

            this.module = serObj.FindProperty("module");
            this.exports = serObj.FindProperty("exports");
#if CLIENT_RUNTIME
            this.executeInEditMode = serObj.FindProperty("executeInEditMode");
#endif
            this.exportList = new ReorderableList(serObj, this.exports);
            this.exportList.drawHeaderCallback += rect =>
                GUI.Label(rect, "Export Objects");
            this.exportList.elementHeight =
                EditorGUIUtility.singleLineHeight;
            this.exportList.drawElementCallback =
                (rect, index, isActive, isFocused) =>
                    this.DrawExport(this.exports, rect, index);

            this.tipsToggle = new AnimBool(!string.IsNullOrEmpty(this.tips));
            this.tipsToggle.valueChanged.AddListener(this.Repaint);

            var luaBehavior = this.target as LuaBehavior;
            _root = luaBehavior.GetComponent<Transform>();
        }

        private void OnDisable()
        {
            this.ClearTips();
            this.tipsToggle.valueChanged.RemoveListener(this.Repaint);
        }

        private void DrawExport(
            SerializedProperty property, Rect rect, int index)
        {
            var element = property.GetArrayElementAtIndex(index);

#if LuaOptimize
#else

            var nameProp = element.FindPropertyRelative("Name");
#endif
            var indexProp = element.FindPropertyRelative("Index");
            var objProp = element.FindPropertyRelative("Object");



            if (objProp.objectReferenceValue)
            {
                if (this._checkRefSet.Contains(objProp.objectReferenceValue))
                {
                    objProp.objectReferenceValue = null;
                }
                else
                {
                    this._checkRefSet.Add(objProp.objectReferenceValue);
                }
            }

#if LuaOptimize
            string nameProp = string.Empty;
            if (objProp.objectReferenceValue)
                nameProp = objProp.objectReferenceValue.name;
#endif

            var halfWidth = rect.width / 2;
            var rectLeft = new Rect(
                rect.x,
                rect.y,
                halfWidth,
                rect.height);
            string text;
            if (indexProp.intValue > 0)
            {
#if LuaOptimize
                text = $"{nameProp}[{indexProp.intValue}]";
#else
                text = $"{nameProp.stringValue}[{indexProp.intValue}]";
#endif

            }
            else
            {
#if LuaOptimize
                text = nameProp;
#else
                text = nameProp.stringValue;
#endif

            }

            EditorGUI.BeginChangeCheck();

            GUI.SetNextControlName("LuaBehavior.Export.Name");
            if (objProp != null && objProp.objectReferenceValue)
            {
                text = objProp.objectReferenceValue.name;
                if (this._checkNameSet.Contains(text))
                {
                    objProp.objectReferenceValue = null;
                    text = "";
                }
                else
                {
                    this._checkNameSet.Add(text);
                }
            }
            else
                text = "";

            text = EditorGUI.TextField(rectLeft, text);
#if LuaOptimize

#else
                nameProp.stringValue = text;
#endif
            if (objProp.objectReferenceValue)
            {
                objProp.objectReferenceValue.name = text;
            }



            if (EditorGUI.EndChangeCheck())
            {
                var startIndex = text.IndexOf('[');
                if (startIndex < 0)
                {
#if LuaOptimize
#else
                nameProp.stringValue = text;
#endif

                    if (objProp.objectReferenceValue)
                        objProp.objectReferenceValue.name = text;
                    indexProp.intValue = 0;
                    this.ClearTips();
                }
                else if (text.EndsWith("]", StringComparison.Ordinal))
                {
#if LuaOptimize
#else
                nameProp.stringValue = text.Substring(0, startIndex);
#endif

                    if (objProp.objectReferenceValue)
                        objProp.objectReferenceValue.name = text.Substring(0, startIndex);
                    var newIndexText = text.Substring(
                        startIndex + 1, text.Length - startIndex - 2);
                    if (int.TryParse(newIndexText, out var newIndex))
                    {
                        indexProp.intValue = newIndex;
                        this.ClearTips();
                    }
                    else
                    {
                        this.ShowTips("Invalid name format.");
                    }
                }
                else
                {
                    this.ShowTips("Invalid name format.");
                }
            }

            var rectRight = new Rect(
                rect.x + halfWidth,
                rect.y,
                halfWidth,
                rect.height);
            EditorGUI.PropertyField(rectRight, objProp, GUIContent.none);
        }

        private void ShowTips(string tips)
        {
            this.tips = tips;
            this.tipsDuration = 5.0f;
            this.tipsStartTime = EditorApplication.timeSinceStartup;
            EditorApplication.update += this.TipsUpdate;
        }

        private void ClearTips()
        {
            this.tips = null;
            this.tipsDuration = 0.0f;
            EditorApplication.update -= this.TipsUpdate;
            this.Repaint();
        }

        #region 拖动到区域

        private void TipsUpdate()
        {
            var passTime =
                EditorApplication.timeSinceStartup - this.tipsStartTime;
            if (passTime >= this.tipsDuration)
            {
                this.ClearTips();
            }
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            var pix = new Color[width * height];

            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = col;
            }

            var result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private void InitBoxStyle()
        {
            if (this.GuistyleBoxDND != null) return;
            GUIStyle GuistyleBoxDND = new GUIStyle(GUI.skin.box);
            GuistyleBoxDND.alignment = TextAnchor.MiddleCenter;
            GuistyleBoxDND.fontStyle = FontStyle.Italic;
            GuistyleBoxDND.fontSize = 12;
            GUI.skin.box = GuistyleBoxDND;
            GuistyleBoxDND.normal.background = MakeTex(2, 2, Color.gray);

            this.GuistyleBoxDND = GuistyleBoxDND;
        }

        private void DrawDragAndDropBox()
        {
            InitBoxStyle();
            var preOpenDragBox = openDragBox;
            openDragBox = EditorGUILayout.Foldout(openDragBox, "Drag Box", true);
            if (preOpenDragBox && !openDragBox)
            {
                ActiveEditorTracker.sharedTracker.isLocked = false;
                Selection.activeObject  = target;
            } else if (!preOpenDragBox && openDragBox)
            {
                ActiveEditorTracker.sharedTracker.isLocked = true;
            }

            if (!openDragBox)
                return;
            Rect myRect = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));
            GUI.Box(myRect, "Drag and Drop Prefabs to this Box!", GuistyleBoxDND);
            if (myRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.DragUpdated)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.DragPerform)
                {
                    Debug.Log(DragAndDrop.objectReferences.Length);
                    for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                    {
                        // myTarget.m_GameObjectGroups[groupIndex].Add(DragAndDrop.objectReferences[i] as GameObject);
                        var go = DragAndDrop.objectReferences[i] as GameObject;
                        var name = go.name;
                        if (_checkNameSet.Contains(name)) continue;
                        var newIdx = exports.arraySize;
                        exports.InsertArrayElementAtIndex(newIdx);
                        var element = exports.GetArrayElementAtIndex(newIdx);
                        var objProp = element.FindPropertyRelative("Object");
                        objProp.objectReferenceValue = go;
                    }
                    Event.current.Use();
                }
            }
        }

        #endregion

    }
}
