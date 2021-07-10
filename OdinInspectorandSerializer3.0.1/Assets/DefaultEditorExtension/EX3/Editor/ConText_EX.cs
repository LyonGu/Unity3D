using UnityEngine;
using UnityEditor;
public class ConText_EX : Editor
{
    //给某组件添加右键菜单选项
    //给Rigidbody添加右键菜单选项
    [MenuItem("CONTEXT/Rigidbody/Init")]
    private static void RigidbodyInit()
    {
        //TODO
        Debug.Log("RigidbodyInit");
    }
}
