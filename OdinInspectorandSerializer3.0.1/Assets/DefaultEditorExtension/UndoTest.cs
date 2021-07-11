using UnityEditor;
using UnityEngine;

public class UndoTest
{
    [MenuItem("Tools/Create Obj")]
    private static void CreateObj()
    {
        GameObject newObj = new GameObject("Undo");
        Undo.RegisterCreatedObjectUndo(newObj, "CreateObj");
    }

    [MenuItem("Tools/Move Obj")]
    private static void MoveObj()
    {
        //获取选中的场景对象
        Transform trans = Selection.activeGameObject.transform;
        if (trans)
        {
            Undo.RecordObject(trans, "MoveObj");
            trans.position += Vector3.up;
        }
    }

    [MenuItem("Tools/AddComponent Obj")]
    private static void AddComponentObj()
    {
        //获取选中的场景对象
        GameObject selectedObj = Selection.activeGameObject;
        if (selectedObj)
        {
            Undo.AddComponent(selectedObj, typeof(Rigidbody));
        }
    }

    [MenuItem("Tools/Destroy Obj")]
    private static void DestroyObj()
    {
        //获取选中的场景对象
        GameObject selectedObj = Selection.activeGameObject;
        if (selectedObj)
        {
            Undo.DestroyObjectImmediate(selectedObj);
        }
    }

    [MenuItem("Tools/SetParent Obj")]
    private static void SetParentObj()
    {
        //获取选中的场景对象
        Transform trans = Selection.activeGameObject.transform;
        Transform root = Camera.main.transform;
        if (trans)
        {
            Undo.SetTransformParent(trans, root, trans.name);
        }
    }
}