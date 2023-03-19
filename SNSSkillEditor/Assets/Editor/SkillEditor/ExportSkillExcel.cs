using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace SkillEditor
{
    public static class ExportSkillExcel
    {
        private static String GetCommandLineArg(String paramName)
        {
            String[] commandLineArgs = Environment.GetCommandLineArgs();
            if (commandLineArgs != null)
            {
                Int32 iOption = Array.IndexOf(commandLineArgs, paramName);
                if (iOption >= 0 && iOption + 1 < commandLineArgs.Length)
                {
                    return commandLineArgs[iOption + 1];
                }
            }
            return null;
        }
        
        public static void ExportExcel()
        {
            var config = SkillEditorManager.Instance.LoadSkill();
            string configPath = GetCommandLineArg("--ExcelConfigPath");
            ExportSkillCsv(config, configPath);
            ExportSkillFResPath(config);
        }
        
        public static void ExportSkillFResPath(SkillConfig config)
        {
            var luaFResPath = Application.dataPath + "/../Lua/config/FResPath.lua";
            var lines = File.ReadAllLines(luaFResPath, new UTF8Encoding(false));
            var start = false;
            var type = 0;
            var beforeLines = new List<string>();
            var effectLines = new List<string>();
            var endLines = new List<string>();
            var resPathSet = new HashSet<string>();

            foreach (var line in lines)
            {
                if (line.TrimEnd().EndsWith("--- skill effect auto gen end"))
                {
                    type = 2;
                }

                switch (type)
                {
                    case 0:
                        beforeLines.Add(line);
                        break;
                    case 1:
                        //effectLines.Add(line);
                        break;
                    case 2:
                        endLines.Add(line);
                        break;
                }

                if (line.TrimEnd().EndsWith("--- skill effect auto gen start"))
                {
                    type = 1;
                }
            }

            foreach (var skillDesc in config.skills)
            {
                foreach (var stage in skillDesc.stages)
                {
                    foreach (var itemBaseList in stage.Value.Values)
                    {
                        foreach (var itemBase in itemBaseList)
                        {
                            if (itemBase is Effect effect)
                            {
                                string path = effect.effectPath;
                                if (!string.IsNullOrEmpty(path))
                                {
                                    if (!resPathSet.Contains(path))
                                    {
                                        string name = Path.GetFileNameWithoutExtension(path);
                                        effectLines.Add($"    {name} = \"{path}\",");
                                        resPathSet.Add(path);
                                    }
                                }
                            }
                            if (itemBase is ProjectileEffect pEffect)
                            {
                                string path = pEffect.effectPath;
                                if (!string.IsNullOrEmpty(path))
                                {
                                    if (!resPathSet.Contains(path))
                                    {
                                        string name = Path.GetFileNameWithoutExtension(path);
                                        effectLines.Add($"    {name} = \"{path}\",");
                                        resPathSet.Add(path);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            var outLine = new List<string>(beforeLines.Count + effectLines.Count + endLines.Count);
            outLine.AddRange(beforeLines);
            outLine.AddRange(effectLines);
            outLine.AddRange(endLines);
            File.WriteAllLines(luaFResPath, outLine);
        }

        [MenuItem("Skill/Set Config Dir Path")]
        public static void SetConfigDirPath()
        {
            var window = EditorWindow.GetWindow<ConfigPathWindow>();
            window.Show();
        }

        [MenuItem("Skill/本地导出技能表现Excel")]
        public static void _export()
        {
            var config = SkillEditorManager.Instance.LoadSkill();
            ExportSkillCsv(config);
        }

        public static void ExportSkillCsv(SkillConfig config, string configDirPath = null)
        {
            if(string.IsNullOrEmpty(configDirPath))
                configDirPath = ConfigPathWindow.ConfigDirPath;
            if (string.IsNullOrEmpty(configDirPath))
            {
                //EditorUtility.DisplayDialog("", "请先配置Config 路径！！！", "确认");
                return;
            }

            if (!Directory.Exists(configDirPath))
            {
                //EditorUtility.DisplayDialog("", "Config 路径配置错误！！！", "确认");
                return;
            }
            
            ExportMain(config, configDirPath);
            ExportStage(config, configDirPath);
            ExportBehaviour(config, configDirPath);
        }

        private static void ExportMain(SkillConfig config, string configDirPath)
        {
            //创建skill_editor_main
            var mainFile = File.Open(Path.Combine(configDirPath, "skill_editor_main.csv"), FileMode.OpenOrCreate,
                FileAccess.ReadWrite);
            mainFile.SetLength(0);
            mainFile.Flush();
            StreamWriter sw = new StreamWriter(mainFile, Encoding.UTF8);
            sw.Write("ID,名称,default,chant\n");
            sw.Write("int,string,ref@skill_editor_stage,ref@skill_editor_stage\n");
            sw.Write("id,desc,default,chant\n");
            sw.Write("S, ,S,S\n");
            foreach (var skillDesc in config.skills)
            {
                string skillId = skillDesc.id.ToString();
                string desc = skillDesc.des;
                string defaultStage = "";
                string chantStage = "";
                if (skillDesc.stages.ContainsKey("default"))
                    defaultStage = $"{skillId}_default";
                if (skillDesc.stages.ContainsKey("chant"))
                    chantStage = $"{skillId}_chant";
                
                sw.Write($"{skillId},{desc},{defaultStage},{chantStage}\n");
            }
            sw.Flush();
            mainFile.Flush();
            sw.Close();
            mainFile.Close();
        }
        private static void ExportStage(SkillConfig config, string configDirPath)
        {
            //创建skill_editor_stage
            var stageFile = File.Open(Path.Combine(configDirPath, "skill_editor_stage.csv"), FileMode.OpenOrCreate,
                FileAccess.ReadWrite);
            stageFile.SetLength(0);
            stageFile.Flush();
            var sw = new StreamWriter(stageFile, Encoding.UTF8);
            sw.Write("ID,StringID,AttackerBehaviours,TargetBehaviours\n");
            sw.Write("int,ref_string,ref@skill_editor_behaviour[],ref@skill_editor_behaviour[]\n");
            sw.Write("id,stringId,attacker_behaviours,target_behaviours\n");
            sw.Write("S, ,S,S\n");
            
            int count = 1;
            foreach (var skillDesc in config.skills)
            {
                string skillId = skillDesc.id.ToString();
                if (skillDesc.stages.TryGetValue("default", out var defaultBehaviours))
                {
                    string attackerBeh = "";
                    string targetBeh = "";
                    if (defaultBehaviours.TryGetValue("attacker", out var attackerBehaviours))
                    {
                        for (int i = 0; i < attackerBehaviours.Count; i++)
                        {
                            attackerBeh += $"{skillId}_default_attacker_{i};";
                        }
                    }
                    if (defaultBehaviours.TryGetValue("target", out var targetBehaviours))
                    {
                        for (int i = 0; i < targetBehaviours.Count; i++)
                        {
                            targetBeh += $"{skillId}_default_target_{i};";
                        }
                    }

                    if (attackerBeh.EndsWith(";"))
                    {
                        attackerBeh = attackerBeh.Substring(0, attackerBeh.Length - 1);
                    }
                    if (targetBeh.EndsWith(";"))
                    {
                        targetBeh = targetBeh.Substring(0, targetBeh.Length - 1);
                    }
                    
                    sw.Write($"{count},{skillId}_default,{attackerBeh},{targetBeh}\n");
                    count++;
                }
                
                if (skillDesc.stages.TryGetValue("chant", out var chantBehaviours))
                {
                    string attackerBeh = "";
                    string targetBeh = "";
                    if (chantBehaviours.TryGetValue("attacker", out var attackerBehaviours))
                    {
                        for (int i = 0; i < attackerBehaviours.Count; i++)
                        {
                            attackerBeh += $"{skillId}_chant_attacker_{i};";
                        }
                    }
                    if (chantBehaviours.TryGetValue("target", out var targetBehaviours))
                    {
                        for (int i = 0; i < targetBehaviours.Count; i++)
                        {
                            targetBeh += $"{skillId}_chant_target_{i};";
                        }
                    }
                    
                    if (attackerBeh.EndsWith(";"))
                    {
                        attackerBeh = attackerBeh.Substring(0, attackerBeh.Length - 1);
                    }
                    if (targetBeh.EndsWith(";"))
                    {
                        targetBeh = targetBeh.Substring(0, targetBeh.Length - 1);
                    }
                    
                    sw.Write($"{count},{skillId}_chant,{attackerBeh},{targetBeh}\n");
                    count++;
                }
            }
            sw.Flush();
            stageFile.Flush();
            sw.Close();
            stageFile.Close();
        }

        private static void ExportBehaviour(SkillConfig config, string configDirPath)
        {
            //创建skill_editor_behaviour
            var behaviourFile = File.Open(Path.Combine(configDirPath, "skill_editor_behaviour.csv"),
                FileMode.OpenOrCreate, FileAccess.ReadWrite);
            behaviourFile.SetLength(0);
            behaviourFile.Flush();

            var dic = CollectBehaviourParam();
            var sw = new StreamWriter(behaviourFile, Encoding.UTF8);
            string line1 = "ID,StringID,BehaviourType,";
            string line2 = "int,ref_string,int,";
            string line3 = "id,string_Id,BehaviourType,";
            string line4 = "S, ,S,";
            foreach (var pair in dic)
            {
                line1 += (pair.Key + ",");
                line2 += (pair.Value + ",");
                line3 += (pair.Key + ",");
                line4 += "S,";
            }

            line1 = line1.Substring(0, line1.Length - 1) + "\n";
            line2 = line2.Substring(0, line2.Length - 1) + "\n";
            line3 = line3.Substring(0, line3.Length - 1) + "\n";
            line4 = line4.Substring(0, line4.Length - 1) + "\n";
            sw.Write(line1);
            sw.Write(line2);
            sw.Write(line3);
            sw.Write(line4);

            int count = 1;
            foreach (var skillDesc in config.skills)
            {
                string skillId = skillDesc.id.ToString();
                foreach (var item in skillDesc.stages)
                {
                    var stage = item.Value;
                    var stageName = item.Key;
                    
                    if (stage.TryGetValue("attacker", out var attackerBehaviours))
                    {
                        for (int i = 0; i < attackerBehaviours.Count; i++)
                        {
                            var itemBase = attackerBehaviours[i];
                            StringBuilder line = new StringBuilder();
                            line.Append($"{count++},{skillId}_{stageName}_attacker_{i},");
                            AppendBehaviourField(itemBase, line, dic);
                            string toWrite = line.ToString();
                            toWrite = toWrite.Substring(0, toWrite.Length - 1) + "\n";
                            sw.Write(toWrite);
                        }
                    }
                    if (stage.TryGetValue("target", out var targetBehaviours))
                    {
                        for (int i = 0; i < targetBehaviours.Count; i++)
                        {
                            var itemBase = targetBehaviours[i];
                            StringBuilder line = new StringBuilder();
                            line.Append($"{count++},{skillId}_{stageName}_target_{i},");
                            AppendBehaviourField(itemBase, line, dic);
                            string toWrite = line.ToString();
                            toWrite = toWrite.Substring(0, toWrite.Length - 1) + "\n";
                            sw.Write(toWrite);
                        }
                    }
                }
            }

            sw.Flush();
            behaviourFile.Flush();
            sw.Close();
            behaviourFile.Close();
        }
        
        // 得到一个 字段名字-对应类型的映射
        private static Dictionary<string, string> CollectBehaviourParam()
        {
            Assembly ass = Assembly.GetAssembly(typeof(ItemBase));
            Type[] types = ass.GetTypes();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            
            //Add ItemBase-BaseClass Param
            foreach (var fieldInfo in typeof(ItemBase).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (GetTypeString(fieldInfo.FieldType, out string typeValue))
                {
                    string typeKey = fieldInfo.Name;
                    dic.Add(typeKey, typeValue);
                }
                else
                    Debug.Log($"需要定义ItemBase的字段{fieldInfo.Name}对应的输出类型");
            }
            //Add ItemBase-ChildClass Param
            foreach (var type in types)
            {
                if (!type.IsDefined(typeof(TypeEnumAttribute)))
                    continue;
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (var fieldInfo in fields)
                {
                    if (GetTypeString(fieldInfo.FieldType, out string typeValue))
                    {
                        //为了修改Behaviour表字段重名的问题！
                        string typeKey = $"{type.Name}_{fieldInfo.Name}";
                        dic.Add(typeKey, typeValue);
                    }
                    else
                        Debug.Log($"需要定义{type.Name}的字段{fieldInfo.Name}对应的输出类型");
                }
            }
            return dic;
        }
        
        private static Dictionary<Type, string> _typeNameDic;

        private static bool GetTypeString(Type fieldType, out string value)
        {
            if (_typeNameDic == null)
            {
                var typeInt = typeof(int);
                var typeUInt = typeof(uint);
                var typeFloat = typeof(float);
                var typeString = typeof(string);
                var typeBool = typeof(bool);
                var typeVector3 = typeof(Vector3);
                var typeColor = typeof(EditorColor);
                var typeAnimationCurve = typeof(AnimationCurve);

                _typeNameDic = new Dictionary<Type, string>()
                {
                    {typeInt, "int"},
                    {typeUInt, "int"},
                    {typeFloat, "float"},
                    {typeString, "string"},
                    {typeBool, "bool"},
                    {typeVector3, "string"},
                    {typeColor, "string"},
                    {typeAnimationCurve, "string"}
                };
            }
            if (_typeNameDic.ContainsKey(fieldType))
            {
                value = _typeNameDic[fieldType];
                return true;
            }
            
            //判断非基本类型字段
            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                value = "string[]";
                return true;
            }
            if (fieldType.IsEnum)
            {
                value = "int";
                return true;
            }
            
            //没有定义，抛出异常
            value = "Invalid";
            return false;
        }

        private static bool IsSpecialType(ItemBase itemBase, FieldInfo fieldInfo, out string ret)
        {
            if (fieldInfo.FieldType.IsGenericType && fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                StringBuilder sb = new StringBuilder();
                if (fieldInfo.GetValue(itemBase) is List<SpeedStretch> p)
                {
                    for (var i1 = 0; i1 < p.Count; i1++)
                    {
                        sb.Append(p[i1].ToString());
                        if (i1 != p.Count - 1)
                        {
                            sb.Append(";");
                        }
                    }
                    sb.Append(",");
                    ret = sb.ToString();
                    return true;
                }
            }
            ret = String.Empty;
            return false;
        }

        // dic里存了所有ItemBase类及其所有子类的字段。每一个数据对象都需要导出这些字段。
        // 如果是ItemBase父类或者自身类，那么导出对应的字段信息。
        // 如果不是自身类的数据，直接补上一个逗号即可。
        private static void AppendBehaviourField(ItemBase itemBase, StringBuilder line, Dictionary<string, string> dic)
        {
            var type = itemBase.GetType();
            var attribute = type.GetCustomAttribute<TypeEnumAttribute>();
            if (attribute != null)
                line.Append($"{attribute.TypeEnum},");
            else
                line.Append($"-1,");
            foreach (var pair in dic)
            {
                string typeName, fieldName;
                if (!pair.Key.Contains("_"))
                {
                    typeName = "ItemBase";
                    fieldName = pair.Key;
                }
                else
                {
                    var typeNameAndFieldName = pair.Key.Split('_');
                    typeName = typeNameAndFieldName[0];
                    fieldName = typeNameAndFieldName[1];
                }
                // 如果是自己或者自己基类ItemBase的数据，那么导出数据，否则不导出。
                if (!typeName.Equals(type.Name) && !typeName.Equals("ItemBase"))
                {
                    line.Append(",");
                    continue;
                }
                var fieldInfo = type.GetField(fieldName);
                if (fieldInfo == null)
                    throw new Exception("Shouldn't be here");
                
                //导出数据对象的数据
                var value = fieldInfo.GetValue(itemBase);
                if (value != null)
                {
                    if (value.GetType().IsEnum)
                        line.Append((int) value + ",");
                    else if (IsSpecialType(itemBase, fieldInfo, out string append))
                        line.Append(append);
                    else
                        line.Append(value.ToString() + ",");
                }
                else
                {
                    line.Append(",");
                }
            }
        }
    }

    public class ConfigPathWindow : EditorWindow
    {
        private static string EditorPrefKey = "Config-Path";
        
        public static string ConfigDirPath => EditorPrefs.GetString("Config-Path", "");
        
        public string Path;

        public void OnGUI()
        {
            EditorGUILayout.LabelField($"当前导出csv路径 {ConfigDirPath}");
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("选择导出csv路径", GUILayout.Width(150)))
            {
                Path = EditorUtility.OpenFolderPanel("选择导出csv文件的所在文件夹", Application.dataPath, "");
            }
            Path = GUILayout.TextField(Path);
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("确认"))
            {
                if (!string.IsNullOrEmpty(Path))
                {
                    Path = Path.Replace("\\", "/");
                    EditorPrefs.SetString("Config-Path", Path);
                    Debug.Log($"Set Config-Path:[{Path}] Suc");
                }
            }
        }
    }
}