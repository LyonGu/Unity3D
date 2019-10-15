
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(MyCustomDataUtil))]
public class MyCustoumDataEditor : Editor {

    MyCustomDataUtil mScript;
    /// <summary>
    /// 脚本激活的时候进入，target就是对应[CustomEditor(typeof(MyCustomDataUtil))]的MyCustomDataUtil类
    /// </summary>
    public void OnEnable()
    {
        mScript = target as MyCustomDataUtil;
        if (mScript.myData == null)
        {
            mScript.myData = new MyCustomData();
        }
    }

    /// <summary>
    /// 重载脚本的界面
    /// </summary>
    public override void OnInspectorGUI()
    {
        mScript.myData.mIndex = EditorGUILayout.TextField("场景配置名", mScript.myData.mIndex);
        mScript.myData.age = EditorGUILayout.IntField("年龄", mScript.myData.age);
        mScript.myData.spawnPos = EditorGUILayout.Vector3Field("出生点位置", mScript.myData.spawnPos);

        if (GUILayout.Button("导入"))
        {
            if (string.IsNullOrEmpty(mScript.myData.mIndex))
            {
                Debug.LogError("未输入配置名");
                return;
            }

            string path = "config/" + mScript.myData.mIndex;

            var configObj = Resources.Load(path) as MyCustomData;
            if (configObj != null)
            {
                configObj = Instantiate(configObj);
                configObj.name = mScript.myData.mIndex;
            }
            mScript.myData = configObj;
        }

        if (GUILayout.Button("导出"))
        {
            if (string.IsNullOrEmpty(mScript.myData.mIndex))
            {
                Debug.LogError("未输入配置名");
                return;
            }

            string path = "Assets/Resources/config/" + mScript.myData.mIndex + ".asset";

            if (File.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.SaveAssets();
            }


            AssetDatabase.CreateAsset(Instantiate(mScript.myData), "Assets/Resources/config/" + mScript.myData.mIndex + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

    }
}
