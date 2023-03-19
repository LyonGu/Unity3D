using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using SkillStage = System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<SkillEditor.ItemBase>>;

namespace SkillEditor
{
    public class SkillMainWindow : OdinMenuEditorWindow
    {
        private static SkillMainWindow _instance;

        public static SkillMainWindow Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GetWindow<SkillMainWindow>();
                }

                return _instance;
            }
        }
        
        private Dictionary<string, OdinMenuItem> _skillGroupItems;
        private Dictionary<string, OdinMenuItem> _skillDescItems;
        private Dictionary<Dictionary<string, List<ItemBase>>, OdinMenuItem> _skillStageItems;

        private static OdinMenuItem _settingMenuItem = null;

        private string _searchStr;

        [MenuItem("Skill/打开技能编辑器 %w")]
        private static void OpenWindow()
        {
            // Instance.Show();
            SkillEditorManager.Instance.OpenSkillEditorScene();

            string path = Path.Combine(Application.dataPath, "Editor", "SkillEditor", "layout-new.wlt");
            SkillEditorUtil.LoadWindowLayout(path);
            Debug.Log("Open Skill Editor Success");
        }

        [MenuItem("Skill/打开SkillMainWindow")]
        private static void OpenWindow2()
        {
            Instance.Show();
        }
        
        protected override void OnDestroy()
        {
            _instance = null;
            base.OnDestroy();
        }

        private readonly Color _color = new Color(0, 1, 1, 0.5f);
        
        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree(false);
            _skillGroupItems = new Dictionary<string, OdinMenuItem>();
            _skillDescItems = new Dictionary<string, OdinMenuItem>();
            _skillStageItems = new Dictionary<SkillStage, OdinMenuItem>();

            if (string.IsNullOrEmpty(_searchStr))
            {
                var settingItem = tree.AddObjectAtPath("编辑器模型设置", new SkillModelEditor());
                tree.AddObjectAtPath("分组顺序", new SkillGroupOrderEditor(SkillEditorManager.Instance.Config.groups));
                _settingMenuItem = settingItem.ToArray()[0];

                _settingMenuItem.Style.SelectedColorDarkSkin = _color;
                _settingMenuItem.Style.SelectedColorLightSkin = _color;
                _settingMenuItem.Style.SelectedInactiveColorDarkSkin = _color;
                _settingMenuItem.Style.SelectedInactiveColorLightSkin = _color;

                foreach (var group in SkillEditorManager.Instance.Config.groups)
                {
                    var ret = tree.AddObjectAtPath(group.groupDesc, new SkillGroupEditor(group));
                    var skillGroupMenuItem = ret.ToArray()[0];
                    _skillGroupItems.Add(group.groupDesc, skillGroupMenuItem);
                    
                    skillGroupMenuItem.Style.SelectedColorDarkSkin = _color;
                    skillGroupMenuItem.Style.SelectedColorLightSkin = _color;
                    skillGroupMenuItem.Style.SelectedInactiveColorDarkSkin = _color;
                    skillGroupMenuItem.Style.SelectedInactiveColorLightSkin = _color;
                    skillGroupMenuItem.OnRightClick += OnRightClickSkillGroup;
                    
                    foreach (var skillDesc in SkillEditorManager.Instance.Config.skills)
                    {
                        if (skillDesc.groupIndex == SkillEditorManager.Instance.Config.groups.IndexOf(group))
                        {
                            string skillDescFullName = $"{skillDesc.id}:{skillDesc.des}";
                            var item = tree.AddObjectAtPath(group.groupDesc + "/" + skillDescFullName,
                                new SkillDescEditor(skillDesc));
                            var skillDescItem = item.ToArray()[1];
                            _skillDescItems.Add(skillDescFullName, skillDescItem);

                            skillDescItem.Style.SelectedColorDarkSkin = _color;
                            skillDescItem.Style.SelectedColorLightSkin = _color;
                            skillDescItem.Style.SelectedInactiveColorDarkSkin = _color;
                            skillDescItem.Style.SelectedInactiveColorLightSkin = _color;
                            skillDescItem.OnRightClick += OnRightClickSkillDesc;
                            
                            foreach (var pair in skillDesc.stages)
                            {
                                string stageName = pair.Key;
                                var stageItem = tree.AddObjectAtPath(group.groupDesc + "/" + skillDescFullName + "/" + stageName,
                                    new SkillStageEditor(pair.Value));
                                var stageMenuItem = stageItem.ToArray()[2];
                                _skillStageItems.Add(pair.Value, stageMenuItem);
                                
                                stageMenuItem.Style.SelectedColorDarkSkin = _color;
                                stageMenuItem.Style.SelectedColorLightSkin = _color;
                                stageMenuItem.Style.SelectedInactiveColorDarkSkin = _color;
                                stageMenuItem.Style.SelectedInactiveColorLightSkin = _color;
                                stageMenuItem.OnRightClick += OnRightClickSkillStage;
                            }
                        }
                    }
                }
                _settingMenuItem.Select();
            }
            else
            {
                foreach (var skillDesc in SkillEditorManager.Instance.Config.skills)
                {
                    string skillDescFullName = $"{skillDesc.id}:{skillDesc.des}";
                    if (skillDescFullName.Contains(_searchStr))
                    {
                        var item = tree.AddObjectAtPath(skillDescFullName, 
                            new SkillDescEditor(skillDesc));
                        var skillDescItem = item.ToArray()[0];
                        _skillDescItems.Add(skillDescFullName, skillDescItem);
                        
                        skillDescItem.Style.SelectedColorDarkSkin = _color;
                        skillDescItem.Style.SelectedColorLightSkin = _color;
                        skillDescItem.Style.SelectedInactiveColorDarkSkin = _color;
                        skillDescItem.Style.SelectedInactiveColorLightSkin = _color;
                        skillDescItem.OnRightClick += OnRightClickSkillDesc;
                        
                        foreach (var pair in skillDesc.stages)
                        {
                            string stageName = pair.Key;
                            var stageItem = tree.AddObjectAtPath(skillDescFullName + "/" + stageName,
                                new SkillStageEditor(pair.Value));
                            var stageMenuItem = stageItem.ToArray()[1];
                            _skillStageItems.Add(pair.Value, stageMenuItem);
                                
                            stageMenuItem.Style.SelectedColorDarkSkin = _color;
                            stageMenuItem.Style.SelectedColorLightSkin = _color;
                            stageMenuItem.Style.SelectedInactiveColorDarkSkin = _color;
                            stageMenuItem.Style.SelectedInactiveColorLightSkin = _color;
                            stageMenuItem.OnRightClick += OnRightClickSkillStage;
                        }
                    }
                }
            }
            
            
            tree.Selection.SelectionChanged += OnSelectItem;
            return tree;
        }
        

        protected override void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            _searchStr = EditorGUILayout.TextField("搜索", _searchStr);
            if (EditorGUI.EndChangeCheck())
            {
                ForceMenuTreeRebuild();
            }
            base.OnGUI();

            if (GUILayout.Button("重新加载"))
            {
                ReloadSKill();
            }
            
            if (GUILayout.Button("新增技能"))
            {
                AddSkill();
            }

            if (GUILayout.Button("新增分组"))
            {
                AddSkillGroup();
            }
            
            if (GUILayout.Button("保存导出"))
            {
                SaveExport();
            }
        }

        #region Button Function

        private void ReloadSKill()
        {
            if (EditorUtility.DisplayDialog("", "重新加载会导致当前所有未保存修改丢失，是否确认？", "确认", "取消"))
            {
                _searchStr = "";
                SkillEditorManager.Instance.Reload();
                ForceMenuTreeRebuild();
            }
        }
        
        private void AddSkillGroup()
        {
            string timeStamp = (new DateTimeOffset(DateTime.UtcNow)).ToUnixTimeSeconds().ToString();
            string groupDesc = "新分组" + timeStamp;
            if (!_skillGroupItems.ContainsKey(groupDesc))
            {
                var skillGroup = new SkillGroup {groupDesc = "新分组" + timeStamp}; 
                SkillEditorManager.Instance.Config.groups.Add(skillGroup);
                ForceMenuTreeRebuild(); 
                EditorApplication.delayCall += () =>
                {
                    var groupItem = GetSkillGroupItem(skillGroup.groupDesc);
                    MenuTree.ScrollToMenuItem(groupItem);
                    groupItem.Select();
                };
            }
        }

        private void AddSkill()
        {
            int skillId = SkillEditorManager.Instance.GetNewSkillId();
            SkillDesc skillDesc = new SkillDesc();
            skillDesc.id = skillId;
            skillDesc.des = "新技能";
            SkillEditorManager.Instance.Config.skills.Add(skillDesc);
            ForceMenuTreeRebuild();
            EditorApplication.delayCall += () =>
            {
                var skillDescItem = GetSkillDescItem(skillDesc);
                MenuTree.ScrollToMenuItem(skillDescItem);
                skillDescItem.Select();
            };
        }

        private void SaveExport()
        {
            SkillEditorManager.Instance.BeforeSaveSkill();
            SkillEditorManager.Instance.SaveSkill();
        }

        #endregion

        #region Right-Click

        private void OnRightClickSkillGroup(OdinMenuItem menuItem)
        {
            SkillGroupEditor obj = menuItem.Value as SkillGroupEditor;
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("删除分组"), false, () =>
            {
                obj?.DeleteGroup();
            });
            
            menu.AddItem(new GUIContent("粘贴技能到该分组"), false, () =>
            {
                obj?.PasteSkillDesc();
            });
            
            menu.ShowAsContext();
        }

        private void OnRightClickSkillDesc(OdinMenuItem menuItem)
        {
            SkillDescEditor obj = menuItem.Value as SkillDescEditor;
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("复制技能"), false, () =>
            {
                obj?.CopySkillDesc();
            });

            menu.AddItem(new GUIContent("删除技能"), false, () =>
            {
                obj?.DeleteSkillDesc();
            });
            
            menu.AddItem(new GUIContent("粘贴Stage到该技能"), false, () =>
            {
                obj?.PasteSkillStage();
            });
            
            menu.ShowAsContext();
        }

        private void OnRightClickSkillStage(OdinMenuItem menuItem)
        {
            SkillStageEditor obj = menuItem.Value as SkillStageEditor;
            GenericMenu menu = new GenericMenu();
            
            menu.AddItem(new GUIContent("复制Stage"), false, () =>
            {
                obj?.CopySkillStage();
            });
            
            menu.AddItem(new GUIContent("删除Stage"), false, () =>
            {
                obj?.DeleteSkillStage();
            });

            menu.ShowAsContext();
        }

        #endregion
        
        public OdinMenuItem GetSkillGroupItem(string groupName)
        {
            if (_skillGroupItems != null && _skillGroupItems.ContainsKey(groupName))
                return _skillGroupItems[groupName];

            return null;
        }

        public OdinMenuItem GetSkillGroupItem(int groupIndex)
        {
            if (groupIndex < SkillEditorManager.Instance.Config.groups.Count)
            {
                return GetSkillGroupItem(SkillEditorManager.Instance.Config.groups[groupIndex].groupDesc);
            }

            return null;
        }

        // public bool ChangeSkillGroupItemName(string oldName, string newName)
        // {
        //     if (_skillGroupItems != null && _skillGroupItems.ContainsKey(oldName))
        //     {
        //         var menuItem = _skillGroupItems[oldName];
        //         menuItem.Name = newName;
        //         _skillGroupItems.Remove(oldName);
        //         _skillGroupItems.Add(newName, menuItem);
        //
        //         return true;
        //     }
        //
        //     return false;
        // }

        public OdinMenuItem GetSkillDescItem(string descName)
        {
            if (_skillDescItems != null && _skillDescItems.ContainsKey(descName))
            {
                return _skillDescItems[descName];
            }

            return null;
        }

        public OdinMenuItem GetSkillDescItem(SkillDesc skillDesc)
        {
            string descName = $"{skillDesc.id}:{skillDesc.des}";
            if (_skillDescItems != null && _skillDescItems.ContainsKey(descName))
            {
                return _skillDescItems[descName];
            }

            return null;
        }

        public bool ChangeSkillDescItemName(string oldName, string newName)
        {
            if (_skillDescItems != null && _skillDescItems.ContainsKey(oldName))
            {
                var menuItem = _skillDescItems[oldName];
                menuItem.Name = newName;
                _skillDescItems.Remove(oldName);
                _skillDescItems.Add(newName, menuItem);

                return true;
            }

            return false;
        }

        public OdinMenuItem GetSkillStageItem(SkillStage stage)
        {
            if (_skillStageItems != null && _skillStageItems.ContainsKey(stage))
            {
                return _skillStageItems[stage];
            }

            return null;
        }

        public void OnSelectItem(SelectionChangedType type)
        {
            if (type == SelectionChangedType.ItemAdded)
            {
                if (MenuTree?.Selection?.SelectedValue != null)
                {
                    var obj = MenuTree.Selection.SelectedValue;
                    if (obj is SkillStageEditor stage)
                    {
                        stage.OnSelect();
                    }
                    else if (obj is SkillDescEditor skillDesc)
                    {
                        skillDesc.OnSelect();
                    }  
                }
            }
        }
    }
}