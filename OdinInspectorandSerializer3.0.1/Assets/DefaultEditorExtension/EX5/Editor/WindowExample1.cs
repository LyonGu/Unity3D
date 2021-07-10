using UnityEngine;
using UnityEditor;

public class WindowExample1 : ScriptableWizard
{
    public string msg = "";

    //显示窗体
    [MenuItem("MyWindow/First Window")]
    private static void ShowWindow()
    {
        ScriptableWizard.DisplayWizard<WindowExample1>("WindowExample1", "确定", "取消");
    }

    //显示时调用
    private void OnEnable()
    {
        Debug.Log("OnEnable");
    }

    //更新时调用
    private void OnWizardUpdate()
    {
        Debug.Log("OnWizardUpdate");

        if (string.IsNullOrEmpty(msg))
        {
            errorString = "请输入信息内容";//错误提示
            helpString = "";//帮助提示
        }
        else
        {
            errorString = "";
            helpString = "请点击确认按钮";
        }
    }

    //点击确定按钮时调用
    private void OnWizardCreate()
    {
        Debug.Log("OnWizardCreate");
    }

    //点击第二个按钮时调用
    private void OnWizardOtherButton()
    {
        Debug.Log("OnWizardOtherButton");
    }

    //当ScriptableWizard需要更新其GUI时，将调用此函数以绘制内容
    //为GUI绘制提供自定义行为，默认行为是按垂直方向排列绘制所有公共属性字段
    //一般不重写该方法，按照默认绘制方法即可
    protected override bool DrawWizardGUI()
    {
        return base.DrawWizardGUI();
    }

    //隐藏时调用
    private void OnDisable()
    {
        Debug.Log("OnDisable");
    }

    //销毁时调用
    private void OnDestroy()
    {
        Debug.Log("OnDestroy");
    }
}