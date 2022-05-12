using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.IO;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


/*
        路径在资源处理工具开发的时候非常重要，我们必须要分清楚绝对路径和相对路径。
        Unity内置的接口使用相对路径，以Assets开头。
        而如果使用C#的IO方法就需要使用绝对路径如。
        *****例如AssetDatabase的方法都是相对路径，File的方法都是绝对路径****
     
*/
public class PathFinder
{
    public GUIStyle TextFieldRoundEdge;
    public GUIStyle TextFieldRoundEdgeCancelButton;
    public GUIStyle TextFieldRoundEdgeCancelButtonEmpty;
    public GUIStyle TransparentTextField;

    public string RelativeAssetPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            path = RelativeAssetPathTextField(path);
            return path;
        }
        path = path.Replace("Assets/", "");
        path = RelativeAssetPathTextField(path);
        path = path.Insert(0, "Assets/");
        return path;

    }

    private string RelativeAssetPathTextField(string path)
    {

        if (TextFieldRoundEdge == null)
        {
            TextFieldRoundEdge = new GUIStyle("SearchTextField");
            TextFieldRoundEdgeCancelButton = new GUIStyle("SearchCancelButton");
            TextFieldRoundEdgeCancelButtonEmpty = new GUIStyle("SearchCancelButtonEmpty");
            TransparentTextField = new GUIStyle(EditorStyles.whiteLabel);
            TransparentTextField.normal.textColor = EditorStyles.textField.normal.textColor;
        }

        Rect position = EditorGUILayout.GetControlRect();
        GUIStyle textFieldRoundEdge = TextFieldRoundEdge;
        GUIStyle transparentTextField = TransparentTextField;
        GUIStyle gUIStyle = (path != "") ? TextFieldRoundEdgeCancelButton : TextFieldRoundEdgeCancelButtonEmpty;
        position.width -= gUIStyle.fixedWidth;

        Rect rect = position;
        float num = textFieldRoundEdge.CalcSize(new GUIContent("Assets/")).x - 2f;
        rect.x += num;
        rect.y += 1f;
        rect.width -= num;
        EditorGUI.BeginChangeCheck();
        path = EditorGUI.TextField(rect, path, transparentTextField);
        if (EditorGUI.EndChangeCheck())
        {
            path = path.Replace('\\', '/');
        }

        position.x += position.width;
        position.width = gUIStyle.fixedWidth;
        position.height = gUIStyle.fixedHeight;
        if (GUI.Button(position, GUIContent.none, gUIStyle) && path != "")
        {
            path = "";
            GUI.changed = true;
            GUIUtility.keyboardControl = 0;
        }

        return path;
    }
}


public class C_Tool : EditorWindow
{
    private static C_Tool s_Window = null;
    private static PathFinder mPathFinder = null;
    private string mPath;

    private int renameIndex = 0;
    private void OnGUI()
    {
        if (mPathFinder == null)
            mPathFinder = new PathFinder();
        if (string.IsNullOrEmpty(mPath))
        {
            mPath = "Resources/ToolTest";
        }
        mPath = mPathFinder.RelativeAssetPath(mPath);

        

        if (GUILayout.Button("Unity内置相对路径"))
        {
            Debug.Log(mPath);
        }

        if (GUILayout.Button("获取相对路径下所有贴图路径"))
        {
            string[] LookFor = { mPath };

            /*
                这个AssetDatabase.FindAssets("t:material", LookFor)接口时根据t的类型字符串来找不同种类的资源的，我这里例举几个常用的：
                AssetDatabase.FindAssets("t:texture", LookFor);
                AssetDatabase.FindAssets("t:material", LookFor);
                AssetDatabase.FindAssets("t:Prefab", LookFor);
             */
            //AssetDatabase.FindAssets 即可以根据类型也可以根据名字查询 https://blog.csdn.net/keneyr/article/details/87885305
            //AssetDatabase的接口获取的是相对路径
            string[] guids = AssetDatabase.FindAssets("t:texture", LookFor);
            foreach (var guid in guids)
            {
                //Find texture 
                //AssetDatabase.GUIDToAssetPath  guid 转成 相对路径
                string texturePath = AssetDatabase.GUIDToAssetPath(guid);
                Debug.Log(texturePath);
            }
        }

        if (GUILayout.Button("获取绝对路径下所有贴图路径"))
        {
            string[] LookFor = { mPath };

            /*
                这个AssetDatabase.FindAssets("t:material", LookFor)接口时根据t的类型字符串来找不同种类的资源的，我这里例举几个常用的：
                AssetDatabase.FindAssets("t:texture", LookFor);
                AssetDatabase.FindAssets("t:material", LookFor);
                AssetDatabase.FindAssets("t:Prefab", LookFor);
             */
            //AssetDatabase.FindAssets 即可以根据类型也可以根据名字查询 https://blog.csdn.net/keneyr/article/details/87885305
            //AssetDatabase的接口获取的是相对路径
            string[] guids = AssetDatabase.FindAssets("t:texture", LookFor);
            foreach (var guid in guids)
            {
                //Find texture 
                //AssetDatabase.GUIDToAssetPath  guid 转成 相对路径
                string texturePath = AssetDatabase.GUIDToAssetPath(guid);
                string absTexturePath = ConvertRelativePathToAbsolutePath(texturePath);
                Debug.Log(absTexturePath);

                //File类需要的都是绝对路径
                if (File.Exists(absTexturePath))
                {
                    Debug.Log($"{absTexturePath} 文件资源存在！！！！！");
                }
            }
        }

        if (GUILayout.Button("AssetDatabase文件拷贝移动 速度较慢"))
        {
            /*
            【3】高速文件拷贝，移动

             Unity的AssetDatabase.CopyAsset(oldpath, newpath);方法可以对资源进行拷贝操作，需要注意的是这里的path是全路径名。

            */
            string[] LookFor = { mPath };
            string[] guids = AssetDatabase.FindAssets("t:material", LookFor);

           
            foreach (string guid in guids)
            {
                //Find Material
                string OldMatPath = AssetDatabase.GUIDToAssetPath(guid);
                string NewMatParh = OldMatPath.Remove(OldMatPath.LastIndexOf(".")) + "NewMat.mat";


                /*
                 但是这样实在太慢了！如果资源量巨大，Unity的AssetDatabase的拷贝方法会耗费大量时间。所以这里就需要使用更快的办法。Unity的材质，Prefab，Animation，脚本等资源全部都是文本，
                 所以我们可以直接用文本的方法处理它们，需要注意这里必须使用绝对路径。
                 */
                AssetDatabase.CopyAsset(OldMatPath, NewMatParh);
                

            }
        }

        if (GUILayout.Button("File Write文件拷贝移动 速度较快"))
        {
            /*
            【3】高速文件拷贝，移动

             Unity的AssetDatabase.CopyAsset(oldpath, newpath);方法可以对资源进行拷贝操作，需要注意的是这里的path是全路径名。

            */
            string[] LookFor = { mPath };
            string[] guids = AssetDatabase.FindAssets("t:material", LookFor);


            foreach (string guid in guids)
            {
                //Find Material
                string OldMatPath = AssetDatabase.GUIDToAssetPath(guid);
                string NewMatParh = OldMatPath.Remove(OldMatPath.LastIndexOf(".")) + "NewMat_File.mat";

                string abs_OldMatPath = ConvertRelativePathToAbsolutePath(OldMatPath);
                string abs_NewMatParh = ConvertRelativePathToAbsolutePath(NewMatParh);

                //直接用file文件write 速度很快
                /*
                    只要是文本类的资源，如Material，Prefab，Animation等都可以使用这种方法拷贝，比Unity自带的AssetDatabase方法快很多。

                    如果书拷贝二进制类的资源如贴图，模型等只需要调用File.ReadAllBytes和File.WriteAllBytes即
                 */
                CopyFileByText(abs_OldMatPath, abs_NewMatParh);
            }

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        if (GUILayout.Button("File Bytes文件拷贝移动 速度较快"))
        {
            /*
            【3】高速文件拷贝，移动

             Unity的AssetDatabase.CopyAsset(oldpath, newpath);方法可以对资源进行拷贝操作，需要注意的是这里的path是全路径名。

            */
            string[] LookFor = { mPath };
            string[] guids = AssetDatabase.FindAssets("t:texture", LookFor);

            
            foreach (string guid in guids)
            {
                //Find Material
                string OldTexPath = AssetDatabase.GUIDToAssetPath(guid);
                string NewTexParh = OldTexPath.Remove(OldTexPath.LastIndexOf("/")) + "/1.png";
                string TargetNewTexParh = OldTexPath.Remove(OldTexPath.LastIndexOf("/")) + "/1_New.png";

                string abs_NewTexParh = ConvertRelativePathToAbsolutePath(NewTexParh);
                string abs_TargetNewTexParh = ConvertRelativePathToAbsolutePath(TargetNewTexParh);
                if (File.Exists(abs_NewTexParh))
                {
                    CopyFileByBytes(abs_NewTexParh, abs_TargetNewTexParh);
                }
            }

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        if (GUILayout.Button("高速文件引用替换"))
        {
            /*
            【3】高速文件引用替换

             如果需要替换上万个Material的Texture资源，如果使用Unity的接口那会是非常麻烦的事情，我们需要遍历所有材质，然后拿到贴图然后各种Set，还要考虑名字等等非常麻烦而且非常慢。
             不仅如此，比如想要拿到Animator内的AnimationClip，引擎根本就没有暴露改方法，如果我们想要修改Animation就非常麻烦了，所以我们需要寻找其它办法。

            Unity的资源是文本的特性给了我们很多操作空间。
            我们可以先找到改资源引用到的老资源的GUID，然后拿到新资源的GUID，然后用新资源的GUID替换掉老资源的GUID这样就可以完成快速资源引用的处理。

            */
            string[] LookFor = { mPath };
            string[] guids = AssetDatabase.FindAssets("t:material", LookFor);


            foreach (string guid in guids)
            {
                //Find Material
                string OldMatPath = AssetDatabase.GUIDToAssetPath(guid);
                string NewTexParh = OldMatPath.Remove(OldMatPath.LastIndexOf("/")) + "/1.png";
                string TargetNewTexPath = OldMatPath.Remove(OldMatPath.LastIndexOf("/")) + "/3.png";

                string NewTexParh_GUID = AssetDatabase.AssetPathToGUID(NewTexParh);
                string TargetNewTexPath_GUID = AssetDatabase.AssetPathToGUID(TargetNewTexPath);

                string OldMatPath_abs = ConvertRelativePathToAbsolutePath(OldMatPath);
                if (File.Exists(OldMatPath_abs))
                {

                    string oldMatAssetContent = File.ReadAllText(OldMatPath_abs);
                    string MaterialAssetContentModified = oldMatAssetContent;
                    MaterialAssetContentModified = MaterialAssetContentModified.Replace(NewTexParh_GUID, TargetNewTexPath_GUID);
                    File.WriteAllText(OldMatPath_abs, MaterialAssetContentModified);
                }
            }

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        if (GUILayout.Button("创建文件路径及资源"))
        {
            string newFoldPath = "Assets/Resources/ToolTest/NewFold";
            if (Directory.Exists(newFoldPath) == false)
                Directory.CreateDirectory(newFoldPath);


            //AssetDatabase方式创建文件 http://www.manew.com/youxizz/343.html
            var material = new Material(Shader.Find("Specular"));
            string matSavePath = newFoldPath + "/MyMaterial.mat";
            string matSavePath_abs = ConvertRelativePathToAbsolutePath(matSavePath);
            if(File.Exists(matSavePath_abs))
            {
                // AssetDatabase 删除文件
                AssetDatabase.DeleteAsset(matSavePath);
            }
            AssetDatabase.CreateAsset(material, newFoldPath + "/MyMaterial.mat");

            //File文件读写方式创建
            string originTexPath = "Assets/Resources/ToolTest/1.png";
            string saveTexPath = newFoldPath + "/MyTexture.png";
            string originTexPath_abs = ConvertRelativePathToAbsolutePath(originTexPath);
            string saveTexPath_abs = ConvertRelativePathToAbsolutePath(saveTexPath);
            CopyFileByBytes(originTexPath_abs, saveTexPath_abs);

            //修改texture格式 TextureImporter
            TextureImporter textureImport = AssetImporter.GetAtPath(saveTexPath) as TextureImporter;
            textureImport.npotScale = TextureImporterNPOTScale.None;
            AssetDatabase.ImportAsset(saveTexPath);

            string originMatPath = "Assets/Resources/ToolTest/A.mat";
            string saveMatPath = newFoldPath + "/A_New.mat";
            string originMatPath_abs = ConvertRelativePathToAbsolutePath(originMatPath);
            string saveMatPath_abs = ConvertRelativePathToAbsolutePath(saveMatPath);
            CopyFileByText(originMatPath_abs, saveMatPath_abs);


            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        if (GUILayout.Button("依赖查询"))
        {
            /*
               Unity提供了一个非常方便的方法AssetDatabase.GetDependencies

                该方法可以获取到一个资源所引用到的所有资源，会返回这些资源的相对路径。对于通用的资源使用上述方法查找所有资源的引用    
                但是对于Shader有一个特殊方法
         */
            string originMatPath = "Assets/Resources/ToolTest/A.mat";
            string[] DependencyAssetPaths = AssetDatabase.GetDependencies(originMatPath); //包含本身路径
            foreach (var assetPath in DependencyAssetPaths)
            {
                if(originMatPath.CompareTo(assetPath) != 0)
                    Debug.Log(assetPath);
            }

        }

        if (GUILayout.Button("Shader依赖查询"))
        {
            /*
               Unity提供了一个非常方便的方法AssetDatabase.GetDependencies

                该方法可以获取到一个资源所引用到的所有资源，会返回这些资源的相对路径。对于通用的资源使用上述方法查找所有资源的引用    
                但是对于Shader有一个特殊方法
         */
            //string originMatPath = "Assets/Resources/ToolTest/A.mat";
            //string[] DependencyAssetPaths = AssetDatabase.GetDependencies(originMatPath); //包含本身路径
            //foreach (var assetPath in DependencyAssetPaths)
            //{
            //    if (originMatPath.CompareTo(assetPath) != 0)
            //        Debug.Log(assetPath);
            //}
            Material mat = AssetDatabase.LoadAssetAtPath("Assets/Resources/ToolTest/A.mat", typeof(Material)) as Material;
            int count = ShaderUtil.GetPropertyCount(mat.shader);

            Material CleanMat = new Material(mat.shader);
            string savePath = "Assets/Resources/ToolTest/NewFold/CleanMat.mat";
            string matSavePath_abs = ConvertRelativePathToAbsolutePath(savePath);
            if (File.Exists(matSavePath_abs))
            {
                // AssetDatabase 删除文件
                AssetDatabase.DeleteAsset(savePath);
                
                //File 文件删除
                //File.Delete(matSavePath_abs);
            }
            AssetDatabase.CreateAsset(CleanMat, savePath);
            

            //打印shader属性
            Shader shader = mat.shader;
            for (int i = 0; i < count; i++)
            {
                ShaderUtil.ShaderPropertyType type = ShaderUtil.GetPropertyType(shader, i);
                if (type == ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    //纹理
                    string propertyName = ShaderUtil.GetPropertyName(shader,i);
                    Texture tex = mat.GetTexture(propertyName);
                    CleanMat.SetTexture(propertyName, tex);
                    Debug.Log($"Texture propertyName  {propertyName}");
                }
                else if(type == ShaderUtil.ShaderPropertyType.Float)
                {
                    //float 类型
                    string propertyName = ShaderUtil.GetPropertyName(shader, i);
                    float f = mat.GetFloat(propertyName);
                    CleanMat.SetFloat(propertyName, f);
                    Debug.Log($"float propertyName  {propertyName}");
                }
                else if (type == ShaderUtil.ShaderPropertyType.Color)
                {
                    //Color 类型
                    string propertyName = ShaderUtil.GetPropertyName(shader, i);
                    Color c = mat.GetColor(propertyName);
                    CleanMat.SetColor(propertyName, c);
                    Debug.Log($"Color propertyName  {propertyName}");
                }
                else if (type == ShaderUtil.ShaderPropertyType.Vector)
                {
                    //Vector 类型
                    string propertyName = ShaderUtil.GetPropertyName(shader, i);
                    Vector4 v = mat.GetVector(propertyName);
                    CleanMat.SetVector(propertyName, v);
                    Debug.Log($"Vector propertyName  {propertyName}");
                }
                else if (type == ShaderUtil.ShaderPropertyType.Range)
                {
                    //Range 类型 本质也是float类型
                    string propertyName = ShaderUtil.GetPropertyName(shader, i);
                    float f = mat.GetFloat(propertyName);
                    CleanMat.SetFloat(propertyName, f);
                    Debug.Log($"Range propertyName  {propertyName}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        if (GUILayout.Button("遍历指定目录所有文件"))
        {
            /*
                使用Directory 然后遍历
             */
            string mPath_abs = ConvertRelativePathToAbsolutePath(mPath);
            TraversalDir(mPath_abs);
        }
        if (GUILayout.Button("删除指定目录所有文件"))
        {
            string mPath_abs = ConvertRelativePathToAbsolutePath(mPath);
            DeleteDir(mPath_abs);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        if (GUILayout.Button("文件名修改"))
        {
            /*
                方法1：直接修改原始文件的名字 推荐  File.move
                方法2：把原始文件里的内容拷贝到新文件 
            */

            string savePath = "Assets/Resources/ToolTest/NewFold/CleanMat.mat";
            string saveNewPath = "Assets/Resources/ToolTest/NewFold/CleanMat_new.mat";
            string matSavePath_abs = ConvertRelativePathToAbsolutePath(savePath);
            string matSaveNewPath_abs = ConvertRelativePathToAbsolutePath(saveNewPath);
            if (File.Exists(matSavePath_abs))
            {
                File.Move(matSavePath_abs, matSaveNewPath_abs);
            }

            string renamePath = "Assets/Resources/ToolTest/Rename";
            string[] LookFor = { renamePath };
            string[] guids = AssetDatabase.FindAssets("t:texture", LookFor);
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string oldPath_abs = ConvertRelativePathToAbsolutePath(path);
                string extension = Path.GetExtension(oldPath_abs);
                path = path.Remove(path.LastIndexOf(".")) + $"_{guid}_{renameIndex}{extension}";
                string path_abs = ConvertRelativePathToAbsolutePath(path);
                File.Move(oldPath_abs, path_abs);
                renameIndex++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }


        if (GUILayout.Button("替换组件 text为textMeshPro"))
        {

            string AfontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
            string BfontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset";
            string[] LookFor = { mPath };
            string[] guids = AssetDatabase.FindAssets("t:prefab", LookFor);
            foreach (string guid in guids)
            {
                //Find Material
                string Path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath(Path, typeof(GameObject)) as GameObject;


                //GameObject rootG = GameObject.Instantiate(prefab); OK
                //PrefabUtility.SaveAsPrefabAssetAndConnect(prefab, Path, InteractionMode.AutomatedAction);

                var TextCom = prefab.GetComponentInChildren<Text>(true);
                if (TextCom != null)
                {
                    string msg = TextCom.text;
                    ///Debug.Log($"{TextCom.text}");
                    if (!TextCom.gameObject.GetComponent<TextMeshProUGUI>())
                    {

                        GameObject targetObj = TextCom.gameObject;
                        DestroyImmediate(TextCom, true);
                        TextMeshProUGUI TextMeshProUGUICom = targetObj.AddComponent<TextMeshProUGUI>();
                        TextMeshProUGUICom.text = msg;

                        var font = AssetDatabase.LoadAssetAtPath(BfontPath, typeof(TMP_FontAsset)) as TMP_FontAsset;
                        TextMeshProUGUICom.font = font;

                        PrefabUtility.SavePrefabAsset(prefab);

                        //不能直接替换，因为需要apply后才能找到对应的guid
                        //再替换GUID
                        //string AFont_GUID = AssetDatabase.AssetPathToGUID(AfontPath);
                        //string BFont_GUID = AssetDatabase.AssetPathToGUID(BfontPath);

                        //Debug.Log($"A == {AFont_GUID}");
                        //Debug.Log($"B == {BFont_GUID}");
                        //string Path_abs = ConvertRelativePathToAbsolutePath(Path);
                        //if (File.Exists(Path_abs))
                        //{

                        //    string oldContent = File.ReadAllText(Path_abs);
                        //    string oldContentModified = oldContent;

                        //    oldContentModified = oldContentModified.Replace(AFont_GUID, BFont_GUID);
                        //    File.WriteAllText(Path_abs, oldContentModified);
                        //}
                    }

                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }


        if (GUILayout.Button("查找挂有某个脚本的所有prefab"))
        {
            string[] LookFor = { mPath };
            string[] guids = AssetDatabase.FindAssets("t:prefab", LookFor);
            foreach (string guid in guids)
            {
                //Find Material
                string Path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath(Path, typeof(GameObject)) as GameObject;
                //Debug.Log($"prefabName === {prefab.name}");

                //FindScriptTest findScriptTest = prefab.GetComponent<FindScriptTest>();
                //if(findScriptTest!=null)
                //    findScriptTest.PrintIndex();

                //FindScriptTest[] FindScriptTestAry = prefab.GetComponentsInChildren<FindScriptTest>();

                //for (int i = 0; i < FindScriptTestAry.Length; i++)
                //{
                //    FindScriptTest findScriptTest = FindScriptTestAry[i];
                //    Debug.Log($"prefabName === {findScriptTest.gameObject.name}, index = {findScriptTest.index}");
                //}
            }
        }

    }


    
    [MenuItem("Tools/MyTool")]
    public static void PrecomputedLighting()
    {
        s_Window = EditorWindow.GetWindow<C_Tool>(false, "MyTool", true);
        s_Window.Show();
    }

    private string ConvertRelativePathToAbsolutePath(string RelativePath)
    {
        string DataPath = Application.dataPath;
        string AbsolutePath = DataPath.Remove(DataPath.LastIndexOf("/")) + "/" + RelativePath;
        AbsolutePath = AbsolutePath.Replace(@"\", "/");

        return AbsolutePath;
    }

    //文本拷贝：Material，Prefab，Animation，。。
    //绝对路径
    private void CopyFileByText(string OldPath, string NewPath)
    {
        string OldAssetContent = File.ReadAllText(OldPath);
        File.WriteAllText(NewPath, OldAssetContent);
    }


    //二进制拷贝
    //绝对路径
    private void CopyFileByBytes(string OldPath, string NewPath)
    {
        byte[] OldAssetContent = File.ReadAllBytes(OldPath);
        File.WriteAllBytes(NewPath, OldAssetContent);
    }

    // 遍历目录以及子目录中的所有文件
    private void TraversalDir(string path_abs)
    {
        if (Directory.Exists(path_abs))
        {
            string[] fileList = System.IO.Directory.GetFileSystemEntries(path_abs);

            foreach (string file in fileList)
            {
                // 先当作目录处理如果存在这个目录就重新调用GetFileNum(string srcPath)
                string filePath = file;
                filePath = filePath.Replace(@"\", "/");
                string exsion = Path.GetExtension(filePath);
                if (exsion != ".meta")
                {
                    if (Directory.Exists(filePath))
                        TraversalDir(filePath);
                    else
                        Debug.Log($"{filePath}");
                }
               
            }

        }

        /*
            //遍历指定目录下所有lua文件，包含子目录
            const string LuaScriptsFolder = "Assets/Game/Lua";
            string dirPath = Path.Combine(Application.dataPath, LuaScriptsFolder.Replace("Assets/", ""));
            var dir = new DirectoryInfo(dirPath);
            var files = dir.GetFiles("*.lua", SearchOption.AllDirectories);
            foreach (var f in files)
            {
                //var data = File.ReadAllBytes(f.FullName);
                var fn = Path.GetFileNameWithoutExtension(f.Name);
                //_luaFileEditorMapping[fn] = f.FullName;

                Debug.Log($"LuaFile = {f.Name}");
            }
        */
    }

    // 删除目录以及子目录中的所有文件
    private void DeleteDir(string path_abs)
    {
        if (Directory.Exists(path_abs))
        {
            string[] fileList = System.IO.Directory.GetFileSystemEntries(path_abs);

            foreach (string file in fileList)
            {
                // 先当作目录处理如果存在这个目录就重新调用GetFileNum(string srcPath)
                string filePath = file;
                filePath = filePath.Replace(@"\", "/");
                string exsion = Path.GetExtension(filePath);
                if (exsion != ".meta")
                {
                    if (Directory.Exists(filePath))
                        Directory.Delete(filePath,true);
                    else
                        File.Delete(filePath);
                }

            }

        }
    }


    GameObject GetPrefabInstanceParent(GameObject obj, UnityEngine.Object prefabObj)
    {
        Transform parent;
        while (true)
        {
            parent = obj.transform.parent;
            if (parent == null || !PrefabUtility.GetPrefabObject(parent.gameObject).Equals(prefabObj))
                return obj;
            obj = parent.gameObject;
        }
    }


}
