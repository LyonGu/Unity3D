using UnityEngine;
using UnityEditor;


//方案一
//[CustomEditor(typeof(InspectorExample))] //将本模块指定为InspectorExample组件的编辑器自定义模块
//public class InspectorExampleEditor : Editor
//{
//    //target指该编辑器类绘制的目标类，需要将它强转为目标类
//    private InspectorExample _target { get { return target as InspectorExample; } }

//    //GUI重新绘制
//    public override void OnInspectorGUI()
//    {
//        //EditorGUILayout.LabelField("IntValue",_target.intValue.ToString(),EditorStyles.boldLabel);
//        //_target.intValue = EditorGUILayout.IntSlider(new GUIContent("Slider"),_target.intValue, 0, 10);
//        //_target.floatValue = EditorGUILayout.Slider(new GUIContent("FloatValue"), _target.floatValue, 0, 10);
//        _target.intValue = EditorGUILayout.IntField("IntValue", _target.intValue);
//        _target.floatValue = EditorGUILayout.FloatField("FloatValue", _target.floatValue);
//        _target.stringValue = EditorGUILayout.TextField("StringValue", _target.stringValue);
//        _target.boolValue = EditorGUILayout.Toggle("BoolValue", _target.boolValue);
//        _target.vector3Value = EditorGUILayout.Vector3Field("Vector3Value", _target.vector3Value);
//        _target.enumValue = (Course)EditorGUILayout.EnumPopup("EnumValue", (Course)_target.enumValue);
//        _target.colorValue = EditorGUILayout.ColorField(new GUIContent("ColorValue"), _target.colorValue);
//        _target.textureValue = (Texture)EditorGUILayout.ObjectField("TextureValue", _target.textureValue, typeof(Texture), true);
//    }
//}

[CustomEditor(typeof(InspectorExample))]
public class InspectorExampleEditor : Editor
{
    //定义序列化属性
    private SerializedProperty intValue;
    private SerializedProperty floatValue;
    private SerializedProperty stringValue;
    private SerializedProperty boolValue;
    private SerializedProperty vector3Value;
    private SerializedProperty enumValue;
    private SerializedProperty colorValue;
    private SerializedProperty textureValue;

    private void OnEnable()
    {
        //通过名字查找被序列化属性。
        intValue = serializedObject.FindProperty("intValue");
        floatValue = serializedObject.FindProperty("floatValue");
        stringValue = serializedObject.FindProperty("stringValue");
        boolValue = serializedObject.FindProperty("boolValue");
        vector3Value = serializedObject.FindProperty("vector3Value");
        enumValue = serializedObject.FindProperty("enumValue");
        colorValue = serializedObject.FindProperty("colorValue");
        textureValue = serializedObject.FindProperty("textureValue");
    }

    public override void OnInspectorGUI()
    {
        //表示更新序列化物体
        serializedObject.Update();
        EditorGUILayout.PropertyField(intValue);
        EditorGUILayout.PropertyField(floatValue);
        EditorGUILayout.PropertyField(stringValue);
        EditorGUILayout.PropertyField(boolValue);
        EditorGUILayout.PropertyField(vector3Value);
        EditorGUILayout.PropertyField(enumValue);
        EditorGUILayout.PropertyField(colorValue);
        EditorGUILayout.PropertyField(textureValue);
       
        //水平和垂直布局，注意这是一个方法对，Begin和End不能少
        //EditorGUILayout.BeginVertical("box");
        ////TODO
        //EditorGUILayout.EndVertical();

        EditorGUILayout.BeginHorizontal("box");
        GUILayout.Label("让你快人几步");
        EditorGUILayout.EndHorizontal();
      

        //应用修改的属性值，不加的话，Inspector面板的值修改不了
        serializedObject.ApplyModifiedProperties();
    }

    /*
        P.S. 第二种绘制方式相较于第一种，显示的效果是差不多的。虽然脚本内容多了一点，但是方式比较简单。不用根据每个变量的数据类型选择相对应的属性API绘制。
     */
}