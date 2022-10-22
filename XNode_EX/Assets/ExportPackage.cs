using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ExportPackage
{
    #region 配置区
    static List<string> filterArr = new List<string>
    {
        "Assets",
        "ProjectSettings",
    };
    static string targetPackageName = "UltimateTemplate.unitypackage";
    static ExportPackageOptions flags = ExportPackageOptions.Interactive;
    #endregion


    [MenuItem("Custom/ExportAllProject")]
    static void OneKeyExportAllProject()
    {
        var projectContent = AssetDatabase.GetAllAssetPaths();
        var filteredPathLst = new List<string>();
        var index = -1;
        EditorApplication.update = () =>
        {
            index++;
            var cancel = EditorUtility.DisplayCancelableProgressBar("提取资源", "通过过滤列表提出想要的资源路径", index / (float)projectContent.Length);
            if (cancel || index >= projectContent.Length)
            {
                EditorApplication.update = null;
                EditorUtility.ClearProgressBar();
                AssetDatabase.ExportPackage(filteredPathLst.ToArray(),
                                   targetPackageName,
                                   flags);
                Debug.Log("Project Exported");
            }
            else
            {
                var path = projectContent[index];
                System.Predicate<string> match = (filter) =>
                {
                    return path.StartsWith(filter);
                };
                if (filterArr.FindIndex(match) != -1)
                {
                    filteredPathLst.Add(path);
                }
            }
        };
    }
}
