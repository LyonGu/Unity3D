using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using UnityEditorInternal;
using UnityEngine.UI;

//public CustomEditor(Type inspectedType, bool editorForChildClasses);   bool editorForChildClasses可以让我们定义此编辑器内容在子类中是否有效，默认为false；
[CustomEditor(typeof(UIAnimationHxp), true)]
public class UIAnimationHxpEditor : Editor
{
    UIAnimationHxp data;
    private ReorderableList m_compList;
    private ReorderableList m_compSequenceList;

    private float spaceHeight = 20.0f;
    private bool isExcuteSave = false;



    void CheckSavePrefab(GameObject instance)
    {
        if(!isExcuteSave)
            CheckConfigDatas();
        isExcuteSave = false;
    }

    bool CheckConfigDatas()
    {
        var selectObj = Selection.activeGameObject;
        if (selectObj != null)
        {
            UIAnimationHxp anit = selectObj.GetComponent<UIAnimationHxp>();
            if (anit != null)
            {
                //var enterAnimation = anit as UIEnterAnimation;
                var scriptName = "UIAnimationHxp";
                if (anit.GetType() == typeof(UIEnterAnimation))
                {
                    scriptName = "UIEnterAnimation";
                }
                else if (anit.GetType() == typeof(UIExitAnimation))
                {
                    scriptName = "UIExitAnimation";
                }
                Debug.Log($"Apply save {scriptName} prefab======" + selectObj.name);

                string msg = string.Empty;
                bool isOK = data.CheckDatas(out msg);
                if (!isOK)
                {
                    EditorUtility.DisplayDialog("动作配置数据错误", msg, "确定");
                }
                return isOK;

            }
        }
        return true;
    }

    private void OnDisable()
    {
        PrefabUtility.prefabInstanceUpdated -= CheckSavePrefab;
    }

    private void OnDestroy()
    {
        PrefabUtility.prefabInstanceUpdated -= CheckSavePrefab;
    }
    void OnEnable()
    {


        //这个是个静态方法，所有的prefab保存操作
        PrefabUtility.prefabInstanceUpdated -= CheckSavePrefab;
        PrefabUtility.prefabInstanceUpdated += CheckSavePrefab;

        //获取当前编辑自定义Inspector的对象
        data = (UIAnimationHxp)target;
        m_compList = new ReorderableList(serializedObject,
            serializedObject.FindProperty("animationDataConfig"),
            true, true, true, true);

        m_compList.drawElementCallback = DrawNameElement;

        m_compList.drawHeaderCallback = (Rect rect) =>
        {
            GUI.Label(rect, "-------------------------动作配置数据--------------------------");
        };

        m_compList.elementHeightCallback = (int index) =>
        {

            var element = m_compList.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty compType = element.FindPropertyRelative("animationType");
            UIAnimationHxp.AnimationType animationT = (UIAnimationHxp.AnimationType)compType.intValue;
            bool isReative = false;
            switch (animationT)
            {
                case UIAnimationHxp.AnimationType.MoveBy:
                case UIAnimationHxp.AnimationType.ScaleBy:
                case UIAnimationHxp.AnimationType.RotateBy:
                case UIAnimationHxp.AnimationType.GradientColor:
                    isReative = true;
                    break;

            }

            float totoalH = 210;
            SerializedProperty uIAnimationControlType = element.FindPropertyRelative("uIAnimationControlType");
            if (uIAnimationControlType.intValue == (int)UIAnimationHxp.UIAnimationControlType.AnimationCurve)
            {
                totoalH += spaceHeight;
            }

            if (isReative)
                totoalH -= spaceHeight;
            SerializedProperty isLoop = element.FindPropertyRelative("isLoop");
            if(isLoop.boolValue)
                totoalH += spaceHeight;
            return totoalH;

        };

        m_compList.onAddCallback = (list) =>
        {
            SerializedProperty addedElement;
            // if something is selected add after that element otherwise on the end
            var count = list.count;
            list.serializedProperty.InsertArrayElementAtIndex(count);
            addedElement = list.serializedProperty.GetArrayElementAtIndex(count);


            //默认值设置
            var animationType = addedElement.FindPropertyRelative("animationType");
            animationType.intValue =(int)UIAnimationHxp.AnimationType.MoveTo;
            var uIAnimationControlType = addedElement.FindPropertyRelative("uIAnimationControlType");
            uIAnimationControlType.intValue = (int)UIAnimationHxp.UIAnimationControlType.InSine;

            var animationTime = addedElement.FindPropertyRelative("animationTime");
            animationTime.floatValue = 1.0f;

            var startValue = addedElement.FindPropertyRelative("startValue");
            startValue.vector3Value = Vector3.zero;

            var startColor = addedElement.FindPropertyRelative("startColor");
            startColor.colorValue = Color.white;
            var endColor = addedElement.FindPropertyRelative("endColor");
            endColor.colorValue = Color.white;
            var startAlpha = addedElement.FindPropertyRelative("startAlpha");
            startAlpha.floatValue = 0.0f;
            var endAlpha = addedElement.FindPropertyRelative("endAlpha");
            endAlpha.floatValue = 1.0f;

        };



        ////deathComList
        m_compSequenceList = new ReorderableList(serializedObject,
            serializedObject.FindProperty("SequenceAniList"),
            true, true, true, true);

        m_compSequenceList.drawElementCallback = DrawNameElementSequence;

        m_compSequenceList.drawHeaderCallback = (Rect rect) =>
        {
            GUI.Label(rect, "-------------------------顺序播放对列--------------------------");
        };

        m_compSequenceList.elementHeightCallback = (int index) =>
        {

            var element = m_compSequenceList.serializedProperty.GetArrayElementAtIndex(index);
            return 20;

        };


        //m_compList.elementHeight = 150; //后面改成动态调整
    }
    private void DrawNameElement(Rect rect, int index, bool selected, bool focused)
    {
        SerializedProperty itemData = m_compList.serializedProperty.GetArrayElementAtIndex(index);

        Rect gameObjectRect = new Rect(rect);
        gameObjectRect.y += spaceHeight;
        SerializedProperty gameObject = itemData.FindPropertyRelative("animationId");
        EditorGUI.PropertyField(gameObjectRect, gameObject, new GUIContent("动作ID"), true);

        Rect comTypeRect = new Rect(gameObjectRect);
        comTypeRect.y += spaceHeight;
        SerializedProperty compType = itemData.FindPropertyRelative("animationType");

           
        EditorGUI.PropertyField(comTypeRect, compType, new GUIContent("动作类型"), true);



        UIAnimationHxp.AnimationType animationT = (UIAnimationHxp.AnimationType)compType.intValue;

        switch (animationT)
        {
            case UIAnimationHxp.AnimationType.MoveTo:
                   
                comTypeRect = new Rect(comTypeRect);
                comTypeRect.y += spaceHeight;
                compType = itemData.FindPropertyRelative("startValue");
                EditorGUI.PropertyField(comTypeRect, compType, new GUIContent("起始位置"), true);

                comTypeRect = new Rect(comTypeRect);
                comTypeRect.y += spaceHeight;
                compType = itemData.FindPropertyRelative("endValue");
                EditorGUI.PropertyField(comTypeRect, compType, new GUIContent("目标位置"), true);
                break;
            case UIAnimationHxp.AnimationType.MoveBy:
                comTypeRect = new Rect(comTypeRect);
                comTypeRect.y += spaceHeight;
                compType = itemData.FindPropertyRelative("endValue");
                EditorGUI.PropertyField(comTypeRect, compType, new GUIContent("位移相对值"), true);
                break;
            case UIAnimationHxp.AnimationType.ScaleTo:
                comTypeRect = new Rect(comTypeRect);
                comTypeRect.y += spaceHeight;
                compType = itemData.FindPropertyRelative("startValue");
                EditorGUI.PropertyField(comTypeRect, compType, new GUIContent("起始缩放"), true);

                comTypeRect = new Rect(comTypeRect);
                comTypeRect.y += spaceHeight;
                compType = itemData.FindPropertyRelative("endValue");
                EditorGUI.PropertyField(comTypeRect, compType, new GUIContent("目标缩放"), true);
                break;
            case UIAnimationHxp.AnimationType.ScaleBy:
                comTypeRect = new Rect(comTypeRect);
                comTypeRect.y += spaceHeight;
                compType = itemData.FindPropertyRelative("endValue");
                EditorGUI.PropertyField(comTypeRect, compType, new GUIContent("缩放相对值"), true);
                break;
            case UIAnimationHxp.AnimationType.RotateTo:

                comTypeRect = new Rect(comTypeRect);
                comTypeRect.y += spaceHeight;
                compType = itemData.FindPropertyRelative("startValue");
                EditorGUI.PropertyField(comTypeRect, compType, new GUIContent("起始旋转"), true);

                comTypeRect = new Rect(comTypeRect);
                comTypeRect.y += spaceHeight;
                compType = itemData.FindPropertyRelative("endValue");
                EditorGUI.PropertyField(comTypeRect, compType, new GUIContent("目标旋转"), true);
                break;
            case UIAnimationHxp.AnimationType.RotateBy:
                comTypeRect = new Rect(comTypeRect);
                comTypeRect.y += spaceHeight;
                compType = itemData.FindPropertyRelative("endValue");
                EditorGUI.PropertyField(comTypeRect, compType, new GUIContent("旋转相对值"), true);
                break;
            case UIAnimationHxp.AnimationType.Fade:

                var OcomTypeRect = new Rect(comTypeRect);
                comTypeRect = new Rect(comTypeRect);
                comTypeRect.y += spaceHeight;
                compType = itemData.FindPropertyRelative("startAlpha");

                OcomTypeRect.y = comTypeRect.y;
                OcomTypeRect.height = 18;
                EditorGUI.Slider(OcomTypeRect, compType, 0, 1, new GUIContent("初始透明度"));

                comTypeRect = new Rect(comTypeRect);
                comTypeRect.y += spaceHeight;
                OcomTypeRect.y = comTypeRect.y;
                compType = itemData.FindPropertyRelative("endAlpha");
                EditorGUI.Slider(OcomTypeRect, compType, 0, 1, new GUIContent("目标透明度"));
                break;
            case UIAnimationHxp.AnimationType.Color:
                comTypeRect = new Rect(comTypeRect);
                comTypeRect.y += spaceHeight;
                compType = itemData.FindPropertyRelative("startColor");
                EditorGUI.PropertyField(comTypeRect, compType, new GUIContent("初始颜色"), true);

                comTypeRect = new Rect(comTypeRect);
                comTypeRect.y += spaceHeight;
                compType = itemData.FindPropertyRelative("endColor");
                EditorGUI.PropertyField(comTypeRect, compType, new GUIContent("目标颜色"), true);
                break;
            case UIAnimationHxp.AnimationType.GradientColor:
                comTypeRect = new Rect(comTypeRect);
                comTypeRect.y += spaceHeight;
                compType = itemData.FindPropertyRelative("gradientColor");
                EditorGUI.PropertyField(comTypeRect, compType, new GUIContent("颜色渐变编辑器"), true);
                break;

        }
            
        comTypeRect = new Rect(comTypeRect);
        comTypeRect.y += spaceHeight;
        compType = itemData.FindPropertyRelative("animationTime");
        EditorGUI.PropertyField(comTypeRect, compType, new GUIContent("动作总时间"), true);

        comTypeRect = new Rect(comTypeRect);
        comTypeRect.y += spaceHeight;
        compType = itemData.FindPropertyRelative("uIAnimationControlType");
        EditorGUI.PropertyField(comTypeRect, compType, new GUIContent("动作控制类型"), true);


        if (compType.intValue == (int)UIAnimationHxp.UIAnimationControlType.AnimationCurve)
        {
            comTypeRect = new Rect(comTypeRect);
            comTypeRect.y += spaceHeight;
            compType = itemData.FindPropertyRelative("animationCurve");
            EditorGUI.PropertyField(comTypeRect, compType, new GUIContent("自定义动作曲线"), true);
        }


        comTypeRect = new Rect(comTypeRect);
        comTypeRect.y += spaceHeight;
        compType = itemData.FindPropertyRelative("obj");
        EditorGUI.PropertyField(comTypeRect, compType, new GUIContent("目标对象"), true);

        if (!Application.isPlaying)
        {
            //设置目标初始值
            var objR = compType.objectReferenceValue as GameObject;
            if (objR != null)
            {
                var transCom = objR.transform;
                var imgCom = objR.GetComponent<Image>();
                var transValue = itemData.FindPropertyRelative("startValue");

                switch (animationT)
                {
                    case UIAnimationHxp.AnimationType.MoveTo:
                        if (transValue.vector3Value == Vector3.zero)
                            transValue.vector3Value = transCom.localPosition;
                        break;
                    case UIAnimationHxp.AnimationType.ScaleTo:
                        if (transValue.vector3Value == Vector3.zero)
                            transValue.vector3Value = transCom.localScale;
                        break;
                    case UIAnimationHxp.AnimationType.RotateTo:
                        if (transValue.vector3Value == Vector3.zero)
                            transValue.vector3Value = transCom.eulerAngles;
                        break;
                    case UIAnimationHxp.AnimationType.Color:
                        if (imgCom != null)
                        {
                            var startColor = itemData.FindPropertyRelative("startColor");
                            if (startColor.colorValue == Color.white)
                                startColor.colorValue = imgCom.color;
                        }
                        break;
                    case UIAnimationHxp.AnimationType.Fade:

                        if (imgCom != null)
                        {
                            var startAlpha = itemData.FindPropertyRelative("startAlpha");
                            if (startAlpha.floatValue == 0.0f)
                                startAlpha.floatValue = imgCom.color.a;
                        }
                        break;
                }
            }
        }
        

        comTypeRect = new Rect(comTypeRect);
        comTypeRect.y += spaceHeight;
        compType = itemData.FindPropertyRelative("isLoop");
        EditorGUI.PropertyField(comTypeRect, compType, new GUIContent("是否循环"), true);
        if (compType.boolValue)
        {
            comTypeRect = new Rect(comTypeRect);
            comTypeRect.y += spaceHeight;
            compType = itemData.FindPropertyRelative("loopType");
            EditorGUI.PropertyField(comTypeRect, compType, new GUIContent("循环类型"), true);
        }

        comTypeRect = new Rect(comTypeRect);
        comTypeRect.y += spaceHeight;
        compType = itemData.FindPropertyRelative("delayTime");
        EditorGUI.PropertyField(comTypeRect, compType, new GUIContent("动作延迟时间"), true);

    }

    private void DrawNameElementSequence(Rect rect, int index, bool selected, bool focused)
    {
        SerializedProperty itemData = m_compSequenceList.serializedProperty.GetArrayElementAtIndex(index);

        //float offset = 30.0f;

        Rect gameObjectRect = new Rect(rect);
        SerializedProperty gameObject = itemData.FindPropertyRelative("animationId");
        EditorGUI.PropertyField(gameObjectRect, gameObject, new GUIContent("动作ID"), true);

    }


    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("最大动作执行时间", GUILayout.MaxWidth(100));
        data.maxAnimationTime = EditorGUILayout.FloatField(data.maxAnimationTime, GUILayout.MaxWidth(40));
        EditorGUILayout.EndHorizontal();
        
        serializedObject.Update();
        m_compList.DoLayoutList();
        m_compSequenceList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();


        EditorGUILayout.LabelField("--------------------------------操作--------------------------------");
        EditorGUILayout.BeginHorizontal();

        bool isShowAutoPlay = true;
        if (data is UIEnterAnimation || data is UIExitAnimation)
            isShowAutoPlay = false;
        if (isShowAutoPlay)
        {
            EditorGUILayout.LabelField("是否自动播放", GUILayout.MaxWidth(80));
            data.isAutoPlay = EditorGUILayout.Toggle(data.isAutoPlay, GUILayout.MaxWidth(60));
        }
       
        if (GUILayout.Button("保存数据"))
        {
            bool isOk = CheckConfigDatas();
            if (isOk)
            {
                isExcuteSave = true;
                PrefabUtility.ApplyPrefabInstance(data.gameObject, InteractionMode.UserAction);
            }
        }
        if (GUILayout.Button("重置修改数据"))
        {
            PrefabUtility.RevertPrefabInstance(data.gameObject, InteractionMode.UserAction);
        }
       
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("测试动作Id", GUILayout.MaxWidth(70));
        data.debugAnimationId = EditorGUILayout.IntField(data.debugAnimationId, GUILayout.MaxWidth(40));
        GUILayout.Space(10);
        if (GUILayout.Button("PlayByAnimId", GUILayout.MaxWidth(120)))
        {
            data.PlayById(data.debugAnimationId);
        }
        GUILayout.Space(60);
        if (GUILayout.Button("PlayAll", GUILayout.MaxWidth(120)))
        {
            data.Play();
        }
        EditorGUILayout.EndHorizontal();






        //修改脚本属性就能生效到对应的prefab上
        Undo.RecordObject(data, "modify data value");
    }
}


