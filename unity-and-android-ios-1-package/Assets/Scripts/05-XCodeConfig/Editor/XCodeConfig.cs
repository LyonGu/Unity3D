using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

public class XCodeConfig : MonoBehaviour
{


    [PostProcessBuild]
    private static void OnBuildFinsh(BuildTarget buildTarget , string buildPath) {
        Debug.Log(" 构建完成: " + buildTarget + " 路径: " + buildPath);

#if UNITY_IPHONE
        SetXCodeProject(buildPath);
#endif
    }

    public static void SetXCodeProject(string buildPath) {

        PBXProject project = new PBXProject();
        project.ReadFromString(File.ReadAllText(PBXProject.GetPBXProjectPath(buildPath)));

        string targetGuid = project.GetUnityMainTargetGuid();
        // 添加了一个库 
        project.AddFrameworkToProject(targetGuid, "AVKit.framework", false);
        // 添加了一个权限 
        PlistDocument plist = new PlistDocument();
        plist.ReadFromString(File.ReadAllText(Path.Combine(buildPath, "Info.plist")));

        plist.root.SetString("NSCameraUsageDescription", "弦风课堂请求使用相机!");
        plist.WriteToFile(Path.Combine(buildPath, "Info.plist"));

        // 设置签名证书 
        project.SetBuildProperty(targetGuid, "CODE_SIGN_IDENTITY", "test123");

        string fileGuid = project.FindFileGuidByProjectPath("MainApp/main.mm");
        project.SetCompileFlagsForFile(targetGuid, fileGuid, new List<string>() {"xxx" });

        project.WriteToFile(PBXProject.GetPBXProjectPath(buildPath));
    }

}
