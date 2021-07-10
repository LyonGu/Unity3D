using UnityEngine;
using UnityEditor;

public class ModificationValue : ScriptableWizard
{
    public float targetValue;

    [MenuItem("Tools/DefaultEditor/Create Wizard")]
    private static void CreateWizard()
    {
        DisplayWizard<ModificationValue>("统一修改", "修改并退出", "修改不退出");
    }

    /// <summary>
    /// 按下创建按钮时
    /// </summary>
    private void OnWizardCreate()
    {
        GameObject[] gos = Selection.gameObjects;  //Selection 选中的对象
        foreach (var go in gos)
        {
            Test _Test = go.GetComponent<Test>();
            if (_Test != null)
            {
                //记录撤销操作
                //Undo.RecordObject(_Test, "value");
                _Test.value = targetValue;
            }
        }
    }

    /// <summary>
    /// 按下其他按钮时
    /// </summary>
    private void OnWizardOtherButton()
    {
        Debug.Log("点击了其他按钮");
    }

    /// <summary>
    /// 当窗口中的数据更新时
    /// </summary>
    private void OnWizardUpdate()
    {
        Debug.Log("当窗口中的数据更新时");
    }

    /// <summary>
    /// 当选择的物体改变时
    /// </summary>
    private void OnSelectionChange()
    {
        Debug.Log("当选择的物体改变时");
        errorString = "";
        helpString = "";
        if (Selection.gameObjects.Length <= 0)
        {
            errorString = "没有选择预制体";
        }
        else
        {
            helpString = "当前选择了" + Selection.gameObjects.Length + "个预制体";
        }
    }
}


