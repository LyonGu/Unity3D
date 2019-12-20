//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;


////定制Serializable类的每个实例的GUI
//[CustomPropertyDrawer(typeof(TestCommponentData))]
//public class CommponentSaveDataDrawer : PropertyDrawer
//{
//    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//    {
//        using (new EditorGUI.PropertyScope(position, label, property))
//        {
//            position.height = EditorGUIUtility.singleLineHeight;

//            SerializedProperty compType = property.FindPropertyRelative("compType");
//            SerializedProperty properList = property.FindPropertyRelative("properList");
//            Rect rect = new Rect(position);
//            //rect.width = position.width * 0.5f;
//            //EditorGUI.PropertyField(rect, compType, new GUIContent("组件类型"));
//            //rect.y += 15.0f;
//            //EditorGUI.PropertyField(rect, properList, new GUIContent("xxxx"));
//            //properList.CountInProperty

//            //int count = properList.CountInProperty();
//            //for (int i = 1; i < count; i++)
//            //{
//            //    //Rect rect = new Rect(targetX, targetY, 15f * (EditorGUI.indentLevel + 1), gridHeight);
//            //    //绘制属性值
//            //    rect.y += 10.0f;
//            //    EditorGUI.PropertyField(rect, properList.GetArrayElementAtIndex(i), GUIContent.none);

//            //}


//            //EditorGUI.PropertyField(rect, properList, GUIContent.none);
            
//            EditorGUI.PropertyField(rect, properList, GUIContent.none);

//        }
//    }

//    //自定义高度
//    //public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//    //{
//    //    return base.GetPropertyHeight(property, label) * 40;
//    //}

//}
