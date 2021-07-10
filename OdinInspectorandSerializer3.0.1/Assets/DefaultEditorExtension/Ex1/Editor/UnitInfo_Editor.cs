
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(UnitInfo))] //将本模块指定为UnitInfo组件的编辑器自定义模块
public class UnitInfo_Editor : Editor
{
    // Start is called before the first frame update

    public override void OnInspectorGUI() //对UnitInfo在Inspector中的绘制方式进行接管
    {
        base.OnInspectorGUI(); //绘制常规内容
        if (GUILayout.Button("从配置表刷新")) //添加按钮和功能——当组件上的按钮被按下时
        {
            UnitInfo unitInfo = (UnitInfo)target; //target就是当前操作对象
            Debug.Log($"ID {unitInfo.Settings.ID}");
        }
    }
}
