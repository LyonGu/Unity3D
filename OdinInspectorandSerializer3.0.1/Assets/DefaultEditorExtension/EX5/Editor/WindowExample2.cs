using UnityEngine;
using UnityEditor;

public class WindowExample2 : EditorWindow
{
    private static WindowExample2 window;//窗体实例

    //显示窗体
    [MenuItem("MyWindow/Second Window")]
    private static void ShowWindow()
    {
        window = EditorWindow.GetWindow<WindowExample2>("Window Example");
        window.Show();
    }

    //显示时调用
    private void OnEnable()
    {
        Debug.Log("OnEnable");
    }

    //绘制窗体内容
    private void OnGUI()
    {
        EditorGUILayout.LabelField("Your Second Window", EditorStyles.boldLabel);
        if (GUILayout.Button("1"))
        {

        }
    }

    //固定帧数调用
    private void Update()
    {
        Debug.Log("Update");
        
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