using UnityEngine;

#if UNITY_EDITOR
namespace Sirenix.OdinInspector.Demos
{
    using UnityEditor;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.OdinInspector;
    using Sirenix.Utilities.Editor;
    using Sirenix.Utilities;

    public class BasicOdinEditorExampleWindow : OdinEditorWindow
    {
        [MenuItem("Tools/Odin Inspector/Demos/Odin Editor Window Demos/Basic Odin Editor Window")]
        private static void OpenWindow()
        {
            var window = GetWindow<BasicOdinEditorExampleWindow>();

            // Nifty little trick to quickly position the window in the middle of the editor.
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(700, 700);
        }

        [EnumToggleButtons]
        [InfoBox("Inherit from OdinEditorWindow instead of EditorWindow in order to create editor windows like you would inspectors - by exposing members and using attributes.")]
        public ViewTool SomeField;

        [HorizontalGroup("Group1",LabelWidth = 100)]
        public int Age;
        [HorizontalGroup("Group1",LabelWidth = 100)]
        public string Name;

   
        [Button("TestClickBtn")]
        public void TestClickBtn()
        {
            Debug.Log("Click TestClickBtn=============");
        }
        
        [GUIColor(0,1,0)]
        [ButtonGroup("TestClickBtn")]
        public void TestClickBtn1()
        {
            Debug.Log("Click TestClickBtn1=============");
        }
        
        
        [ButtonGroup("TestClickBtn")]
        public void TestClickBtn2()
        {
            Debug.Log("Click TestClickBt2n=============");
        }
        
        [TitleGroup("Tabs")]
        [HorizontalGroup("Tabs/Split", Width = 0.5f)]
        [TabGroup("Tabs/Split/Parameters", "A")]
        public string NameA, NameB, NameC;

        [TabGroup("Tabs/Split/Parameters", "B")]
        public int ValueA, ValueB, ValueC;

        [TabGroup("Tabs/Split/Buttons", "Responsive")]
        [ResponsiveButtonGroup("Tabs/Split/Buttons/Responsive/ResponsiveButtons")]
        public void Hello() { }

        [ResponsiveButtonGroup("Tabs/Split/Buttons/Responsive/ResponsiveButtons")]
        public void World() { }

        [ResponsiveButtonGroup("Tabs/Split/Buttons/Responsive/ResponsiveButtons")]
        public void And() { }

        [ResponsiveButtonGroup("Tabs/Split/Buttons/Responsive/ResponsiveButtons")]
        public void Such() { }

        [Button]
        [TabGroup("Tabs/Split/Buttons", "More Tabs")]
        [TabGroup("Tabs/Split/Buttons/More Tabs/SubTabGroup", "A")]
        public void SubButtonA() { }

        [Button]
        [TabGroup("Tabs/Split/Buttons/More Tabs/SubTabGroup", "A")]
        public void SubButtonB() { }

        [Button(ButtonSizes.Gigantic)]
        [TabGroup("Tabs/Split/Buttons/More Tabs/SubTabGroup", "B")]
        public void SubButtonC() { }
    }
}
#endif
