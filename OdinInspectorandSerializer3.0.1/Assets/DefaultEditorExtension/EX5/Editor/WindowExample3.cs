using UnityEngine;
using UnityEditor;

public class WindowExample3 : EditorWindow
{
    private static WindowExample3 window;
    private PopWindowExample popWindow = new PopWindowExample();
    private Rect buttonRect;

    //显示窗体
    [MenuItem("MyWindow/Third Window")]
    private static void ShowWindow()
    {
        window = EditorWindow.GetWindow<WindowExample3>("Window Example 3");
        window.Show();
    }

    //绘制窗体内容
    private void OnGUI()
    {
        GUILayout.Label("Popup example", EditorStyles.boldLabel);
        if (GUILayout.Button("Popup Options", GUILayout.Width(200)))
        {
            PopupWindow.Show(buttonRect, popWindow);
        }
        //获取GUILayout最后用于控件的矩形
        if (Event.current.type == EventType.Repaint)
            buttonRect = GUILayoutUtility.GetLastRect();
    }
}

public class PopWindowExample : PopupWindowContent
{
    bool toggle = true;

    //开启弹窗时调用
    public override void OnOpen()
    {
        Debug.Log("OnOpen");
    }

    //绘制弹窗内容
    public override void OnGUI(Rect rect)
    {
        EditorGUILayout.LabelField("PopWindow");
        toggle = EditorGUILayout.Toggle("Toggle", toggle);
    }

    //关闭弹窗时调用
    public override void OnClose()
    {
        Debug.Log("OnClose");
    }

    public override Vector2 GetWindowSize()
    {
        //设置弹窗的尺寸
        return new Vector2(200, 100);
    }
}