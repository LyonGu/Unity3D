using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Reflection;

namespace SkillEditor
{
    public static class SkillEditorMenuItem
    {
        
        [MenuItem("Skill/打开技能表现配置目录 %q")]
        public static void OpenSKillConfigDir()
        {
            string dir = Path.Combine(Application.streamingAssetsPath, "asset_base", "skillclient", "skillclient.json");
            EditorUtility.RevealInFinder(dir);
        }
        
        [MenuItem("Skill/保存当前作为技能编辑器默认布局")]
        private static void SaveSkillEditorLayout()
        {
            string toFilePath = Path.Combine(Application.dataPath, "Editor", "SkillEditor", "layout-new.wlt");
            SkillEditorUtil.SaveWindowLayout(toFilePath);
            Debug.Log("SaveSkillEditorLayout Suc");
        }
        
        [MenuItem("Skill/Export Template")]
        public static void ExportTemplate()
        {
            string path = Application.dataPath + "/Editor/SkillEditor/skills_template.json";
            SkillConfig conf = new SkillConfig();

            SkillDesc desc = new SkillDesc();
            desc.id = 1001;
            desc.des = "散弹枪";

            var defaultStage = desc.stages["default"];
            var attacker = defaultStage["attacker"];

            Dictionary<uint, bool> dic = new Dictionary<uint, bool>();
            
            var baseType = typeof(ItemBase);
            var types = baseType.Assembly.GetTypes().Where(t => t.BaseType == baseType);
            foreach (var type in types)
            {
                if (type.BaseType != typeof(object))
                {
                    var typeEnum = type.GetCustomAttribute<TypeEnumAttribute>().TypeEnum;
                    if (dic.TryGetValue(typeEnum, out var value))
                    {
                        Debug.LogError($"Repeat Type Enum !!! {type.Name}");
                        return;
                    }
                    dic.Add(typeEnum, true);
                    var obj = (ItemBase)Activator.CreateInstance(type);
                    attacker.Add(obj);
                }
            }
            conf.skills.Add(desc);

            var settings = new JsonSerializerSettings { TypeNameHandling =  TypeNameHandling.Auto};
            File.WriteAllText(path, JsonConvert.SerializeObject(conf, Formatting.Indented, settings), SkillEditorUtil.UTF8);
        }

        [MenuItem("Skill/Export EmmyLua")]
        public static void ExportEmmyLua()
        {
            // new SkillEmmylua().Export();
        }

        [MenuItem("Skill/Show PS Duration")]
        public static void Show()
        {
            GameObject go = Selection.activeObject as GameObject;
            var ps = go.GetComponent<ParticleSystem>();
            Debug.Log(ps.main.duration);
        }
    }
}