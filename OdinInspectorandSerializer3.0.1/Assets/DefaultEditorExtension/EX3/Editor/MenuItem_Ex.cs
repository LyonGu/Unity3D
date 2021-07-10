using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MenuItem_Ex : Editor
{

    /*
     * [MenuItem(“MyTools/test1”,false,priority)]


     第一个参数用来表示菜单的路径；

    第二个参数用来判断是否是有效函数，是否需要显示；

    第三个参数priority是优先级，用来表示菜单按钮的先后顺序，默认值为1000。一般菜单中的分栏，数值相差大于10。

    注意需要是静态方法
     
     */

    [MenuItem("MyTool/DeleteAllObj1", true)] //不显示
    private static bool DeleteValidate()
    {
        if (Selection.objects.Length > 0)
            return true;
        else
            return false;
    }

    [MenuItem("MyTool/DeleteAllObj2", false)] //显示
    private static void MyToolDelete()
    {
        //Selection.objects 返回场景或者Project中选择的多个对象
        foreach (Object item in Selection.objects)
        {
            //记录删除操作，允许撤销
            Undo.DestroyObjectImmediate(item);
        }

        Debug.Log("删除选中物体，允许Ctrl+Z");
    }
}
