//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;
//[CustomEditor(typeof(TestData))]
//public class TestDataEditor : Editor
//{
//    TestData data;

//    void OnEnable()
//    {
//        //获取当前编辑自定义Inspector的对象
//        data = (TestData)target;
//        var age = data.GetType().GetField("age");
//        var par = data.GetComponent<ParticleSystem>();


//        var d = data.host.GetType().GetField("duration");
//        //var obj1 = data.bornState[0];
//        //var pro1 = obj1.propertys[0];
//        //if (pro1.name == "startDelay")
//        //{
//        //    var type = par.main.duration.GetType();
//        //    if (type == typeof(int))
//        //        (pro1 as IntProperty).value = (int)par.main.duration;
//        //    if (type == typeof(float))
//        //        (pro1 as FloatProperty).value = par.main.duration;
//        //}

//        //Debug.Log(age.FieldType);
//    }

//    public override void OnInspectorGUI()
//    {
//        base.OnInspectorGUI();

//        //data.testDict.Add("2222", 1);
//        if (GUILayout.Button("Save Bug"))
//        {
//            Debug.Log("click btn ============");
//        }
//    }

//}
