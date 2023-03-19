// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Reflection;
// using System.Text;
// using Scriban;
// using UnityEditor;
// using UnityEngine;
//
// namespace SkillEditor
// {
//     public class SkillEmmylua
//     {
//         public void Export()
//         {
//             var baseType = typeof(ItemBase);
//             var types = baseType.Assembly.GetTypes().Where(t => t.BaseType == baseType);
//             foreach (var type in types)
//             {
//                 CollectType(type);
//             }
//
//             CollectEnums();
//
//             var filePath = Application.dataPath + "/Editor/SkillEditor/skill_emmylua.txt";
//             var luaPath = Application.dataPath + "/../Lua/skillclient/SkillClientGen.lua";
//             var template = Template.Parse(File.ReadAllText(filePath));
//             var result = template.Render(new
//             {
//                 classDatas = _classDatas, 
//                 enums = _enums,
//             }, member => member.Name);
//             File.WriteAllText(luaPath, result, new UTF8Encoding(false));
//             AssetDatabase.Refresh();
//         }
//
//         public class EnumData
//         {
//             public string name;
//
//             public class EnumObj
//             {
//                 public string name;
//                 public string value;
//             }
//             public List<EnumObj> list = new List<EnumObj>();
//         }
//
//         List<EnumData> _enums = new List<EnumData>();
//
//         private void CollectEnums()
//         {
//             {
//                 var enumData = new EnumData();
//                 _enums.Add(enumData);
//                 enumData.name = "type";
//                 foreach (var classData in _classDatas)
//                 {
//                     enumData.list.Add(new EnumData.EnumObj(){ name = classData.name, value = classData.enumIndex.ToString()});
//                 }
//             }
//             {
//                 var enumData = new EnumData();
//                 _enums.Add(enumData);
//                 enumData.name = "type2name";
//                 foreach (var classData in _classDatas)
//                 {
//                     enumData.list.Add(new EnumData.EnumObj(){ name = $"[{classData.enumIndex}]", value = $"\"skillclient.behavior.{classData.name}\""});
//                 }
//             }
//             var enums = typeof(EnumConfig).GetNestedTypes();
//             foreach (var enumDefine in enums)
//             {
//                 var enumData = new EnumData();
//                 _enums.Add(enumData);
//                 enumData.name = enumDefine.Name;
//                 foreach (int i in Enum.GetValues(enumDefine))
//                 {
//                     var name = Enum.GetName(enumDefine, i);
//                     enumData.list.Add(new EnumData.EnumObj() {name = name, value = i.ToString(),});
//                 }
//             }
//         }
//
//         public class ClassData
//         {
//             public string name => type.Name;
//
//             public int enumIndex => (int)type.GetCustomAttribute<TypeEnumAttribute>().TypeEnum;
//
//             public Type type { get; private set; }
//
//             public ClassData(Type type)
//             {
//                 this.type = type;
//             }
//
//             public class FieldData
//             {
//                 public string name;
//                 public string type;
//                 public string des;
//             }
//             public List<FieldData> fields = new List<FieldData>();
//         }
//
//         private List<ClassData> _classDatas = new List<ClassData>();
//
//         private void CollectType(Type useType)
//         {
//             List<Type> types = new List<Type>();
//             types.Add(useType);
//             // var baseType = useType.BaseType;
//             // while (baseType != typeof(object))
//             // {
//             //     types.Add(baseType);
//             //     baseType = baseType.BaseType;
//             // }
//             types.Reverse();
//             var classData = new ClassData(useType);
//             _classDatas.Add(classData);
//             foreach (var type in types)
//             {
//                 var fileds = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
//                 foreach (var field in fileds)
//                 {
//                     var fieldData = new ClassData.FieldData();
//                     fieldData.name = field.Name;
//                     fieldData.type = GetLuaType(field.FieldType);
//                     if (string.IsNullOrEmpty(fieldData.des))
//                     {
//                         fieldData.des = fieldData.name;
//                     }
//                     classData.fields.Add(fieldData);
//                 }
//                 var propertys = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
//                 foreach (var property in propertys)
//                 {
//                     var fieldData = new ClassData.FieldData();
//                     fieldData.name = property.Name;
//                     fieldData.type = GetLuaType(property.PropertyType);
//                     if (string.IsNullOrEmpty(fieldData.des))
//                     {
//                         fieldData.des = fieldData.name;
//                     }
//                     classData.fields.Add(fieldData);
//                 }
//             }
//         }
//
//         string GetLuaType(Type type)
//         {
//             switch (Type.GetTypeCode(type))
//             {
//                 case TypeCode.Byte:
//                 case TypeCode.SByte:
//                 case TypeCode.UInt16:
//                 case TypeCode.UInt32:
//                 case TypeCode.UInt64:
//                 case TypeCode.Char:
//                 case TypeCode.Int16:
//                 case TypeCode.Int32:
//                 case TypeCode.Int64:
//                 case TypeCode.Decimal:
//                 case TypeCode.Double:
//                 case TypeCode.Single:
//                     return "number";
//                 case TypeCode.Boolean:
//                     return "boolean";
//                 case TypeCode.String:
//                     return "string";
//             }
//             if (type == typeof(SkillEditor.Vector3))
//             {
//                 return "skillclient.vector3";
//             }
//             return GetDisplayName(type);
//         }
//
//         public string GetDisplayName(Type t)
//         {
//             if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
//             {
//                 return string.Format("{0}", GetLuaType(t.GetGenericArguments()[0]));
//             }
//             if (t.IsGenericType && t.Name.Contains('`'))
//             {
//                 return GetTypeFullName(t);
//             }
//             if (t.IsArray)
//             {
//                 return string.Format("{0}[{1}]", GetLuaType(t.GetElementType()), new string(',', t.GetArrayRank() - 1));
//             }
//             return GetTypeFullName(t);
//         }
//
//         private string GetTypeFullName(Type t)
//         {
//             var fullName = t.FullName;
//             if (string.IsNullOrEmpty(fullName))
//             {
//                 fullName = "any";
//             }
//             else
//             {
//                 fullName = fullName.Replace('+', '.');
//                 var index = fullName.IndexOf('`');
//                 if (index >= 0)
//                 {
//                     fullName = fullName.Remove(index);
//                 }
//             }
//             return fullName;
//         }
//     }
// }