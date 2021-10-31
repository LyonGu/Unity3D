using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class RotationTest : MonoBehaviour
{
    //获取到旋转的正确数值
    public Vector3 GetInspectorRotationValueMethod(Transform transform)
    {
        // 获取原生值
        System.Type transformType = transform.GetType();
        PropertyInfo m_propertyInfo_rotationOrder = transformType.GetProperty("rotationOrder", BindingFlags.Instance | BindingFlags.NonPublic);
        object m_OldRotationOrder = m_propertyInfo_rotationOrder.GetValue(transform, null);
        MethodInfo m_methodInfo_GetLocalEulerAngles = transformType.GetMethod("GetLocalEulerAngles", BindingFlags.Instance | BindingFlags.NonPublic);
        object value = m_methodInfo_GetLocalEulerAngles.Invoke(transform, new object[] { m_OldRotationOrder });
        string temp = value.ToString();
        //将字符串第一个和最后一个去掉
        temp = temp.Remove(0, 1);
        temp = temp.Remove(temp.Length - 1, 1);
        //用‘，’号分割
        string[] tempVector3;
        tempVector3 = temp.Split(',');
        //将分割好的数据传给Vector3
        Vector3 vector3 = new Vector3(float.Parse(tempVector3[0]), float.Parse(tempVector3[1]), float.Parse(tempVector3[2]));
        return vector3;
    }


    public RectTransform rectTransform;
    // Start is called before the first frame update
    void Start()
    {
        Transform rectT = this.gameObject.GetComponent<Transform>();

        var EulerAngles2 = rectT.localRotation.eulerAngles;
        Debug.Log($"EulerAngles2 === {EulerAngles2.x} {EulerAngles2.y} {EulerAngles2.z}");
        //只有X轴旋转超过90度才会跟面板上显示不一样
        var EulerAngles = rectT.localEulerAngles;
        Debug.Log($"EulerAngles === {EulerAngles.x} {EulerAngles.y} {EulerAngles.z}");

        var EulerAngles1 = GetInspectorRotationValueMethod(this.transform);

        Debug.Log($"EulerAngles1 === {EulerAngles1.x} {EulerAngles1.y} {EulerAngles1.z}");

        //Debug.Log($"Rotation === {Rotation.x} {Rotation.y} {Rotation.z}");

        //第4象限显示为负数，例如设置300，面板上显示-60
        //rectT.localEulerAngles = new Vector3(120, 45, 78);

        

        var UIEulerAngles = rectTransform.localEulerAngles;
        Debug.Log($"UIEulerAngles === {UIEulerAngles.x} {UIEulerAngles.y} {UIEulerAngles.z}");
        var UIEulerAngles1 = GetInspectorRotationValueMethod(rectTransform);
        Debug.Log($"UIEulerAngles1 === {UIEulerAngles1.x} {UIEulerAngles1.y} {UIEulerAngles1.z}");

        rectTransform.localEulerAngles = new Vector3(120, 54, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
