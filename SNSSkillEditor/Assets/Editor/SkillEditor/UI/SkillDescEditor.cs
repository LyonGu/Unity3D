using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace SkillEditor
{
    [Serializable]
    public enum SkillStageType
    {
        @default = 1,
        chant = 2,
        channel = 3,
    }

    // public enum StageEnum
    // {
    //     @default = 1,
    //     chant = 2,
    //     channel = 3,
    // }
    
    
     public class SkillDescEditor
    {
        private readonly SkillDesc _skillDesc;

        public SkillDesc SkillDesc => _skillDesc;

        private List<ValueDropdownItem<int>> _groupDesc = new ValueDropdownList<int>();
        public SkillDescEditor(SkillDesc value)
        {
            _skillDesc = value;
        }
        
        [ShowInInspector]
        [ValidateInput("MustBeNumber", "必须为数字。")]
        public string Id
        {
            get => _skillDesc.id.ToString();
            set
            {
                int oldId = _skillDesc.id;
                if (Int32.TryParse(value, out int result))
                {
                    _skillDesc.id = result;
                    SkillMainWindow.Instance.ChangeSkillDescItemName($"{oldId}:{_skillDesc.des}", $"{value}:{_skillDesc.des}");
                }
            }
        }
        
        private bool MustBeNumber(string id)
        {
            if (!Int32.TryParse(id, out int result))
                return false;

            return true;
        }

        [ShowInInspector]
        public string Desc
        {
            get => _skillDesc.des;
            set
            {
                string oldDesc = _skillDesc.des;
                _skillDesc.des = value;
                SkillMainWindow.Instance.ChangeSkillDescItemName($"{_skillDesc.id}:{oldDesc}", $"{_skillDesc.id}:{value}");
            }
        }

        [ShowInInspector]
        [ValueDropdown("_groupDesc")]
        public int Group
        {
            get
            {
                _groupDesc = new ValueDropdownList<int>();
                for (int i = 0; i < SkillEditorManager.Instance.Config.groups.Count; i++)
                {
                    var group = SkillEditorManager.Instance.Config.groups[i];
                    _groupDesc.Add(new ValueDropdownItem<int>(group.groupDesc, i));
                }
                return _skillDesc.groupIndex;
            } 
            set
            {
                if (_skillDesc.groupIndex == value)
                    return;

                int oldGroupIndex = _skillDesc.groupIndex;
                _skillDesc.groupIndex = value;
                var skillDescItem = SkillMainWindow.Instance.GetSkillDescItem(_skillDesc);
                var oldSkillGroupItem = SkillMainWindow.Instance.GetSkillGroupItem(oldGroupIndex);
                oldSkillGroupItem.ChildMenuItems.Remove(skillDescItem);
                var newSkillGroupItem = SkillMainWindow.Instance.GetSkillGroupItem(value);
                newSkillGroupItem.ChildMenuItems.Add(skillDescItem);
                newSkillGroupItem.ChildMenuItems.Sort(((itemA, itemB) => String.CompareOrdinal(itemA.Name, itemB.Name)));
                SkillMainWindow.Instance.MenuTree.ScrollToMenuItem(skillDescItem);
            }
        }

        private SkillStageType[] _stages;
        
        [ShowInInspector]
        public SkillStageType[] stages
        {
            get
            {
                if (_stages == null)
                {
                    var stageKeys = _skillDesc.stages.Keys.ToArray();
                    _stages = new SkillStageType[stageKeys.Length];
                    for (int i = 0; i < stageKeys.Length; i++)
                    {
                        Enum.TryParse(stageKeys[i], out SkillStageType stage);
                        _stages[i] = stage;
                    }
                }
                
                return _stages;
            }
            set
            {
                _stages = value;
            }
        }

        [Button("确认修改Stage")]
        public void ConfirmStageChange()
        {
            int enumCountOfSkillStageType = Enum.GetValues(typeof(SkillStageType)).Length;
            if (_stages.Length > enumCountOfSkillStageType)
            {
                EditorUtility.DisplayDialog("", "Stage 数量超出了定义限制！", "确认");
                return;
            }

            int min = (int) SkillStageType.@default;
            int max = (int) SkillStageType.channel;
            foreach (var stage in _stages)
            {
                int value = (int) stage;
                if (value < min || value > max)
                {
                    EditorUtility.DisplayDialog("", "存在非法的Stage！", "确认");
                    return;
                }
            }
            
            Dictionary<SkillStageType, bool> dic = new Dictionary<SkillStageType, bool>();
            foreach (var stage in _stages)
            {
                if (dic.TryGetValue(stage, out var value))
                {
                    EditorUtility.DisplayDialog("", "Stage 有重复！", "确认");
                    return;
                }
                dic.Add(stage, true);
            }
            
            var newStages = new Dictionary<string, Dictionary<string, List<ItemBase>>>();
            var orgStageKeys = _skillDesc.stages.Keys.ToArray();
            int orgKeyCount = orgStageKeys.Length;

            for (int i = 0; i < orgKeyCount; i++)
            {
                if (i < _stages.Length)
                {
                    newStages.Add(_stages[i].ToString(), _skillDesc.stages[orgStageKeys[i]]);
                }
            }

            for (int i = orgKeyCount; i < _stages.Length; i++)
            {
                var stage = new Dictionary<string, List<ItemBase>>();
                stage.Add("attacker", new List<ItemBase>());
                stage.Add("target", new List<ItemBase>());
                    
                newStages.Add(_stages[i].ToString(), stage);
            }

            _skillDesc.stages = newStages;

            
            SkillMainWindow.Instance.ForceMenuTreeRebuild();
            var item = SkillMainWindow.Instance.GetSkillDescItem(_skillDesc);
            item.Select();
            SkillMainWindow.Instance.MenuTree.ScrollToMenuItem(item);
        }
        
        [ShowInInspector]
        [PropertyOrder(10)]
        public GameObject AttackerPrefab
        {
            get
            {
                if (!string.IsNullOrEmpty(_skillDesc.attackerPrefabPath))
                {
                    return AssetDatabase.LoadAssetAtPath<GameObject>(_skillDesc.attackerPrefabPath);
                }

                return null;
            }
            set
            {
                string path = null;
                if (value != null)
                {
                    path = AssetDatabase.GetAssetPath(value);
                }

                _skillDesc.attackerPrefabPath = path;
                SkillEditorManager.Instance.RefreshModel();
            }
        }

        [ShowInInspector]
        [PropertyOrder(11)]
        public GameObject TargetPrefab
        {
            get
            {
                if (!string.IsNullOrEmpty(_skillDesc.targetPrefabPath))
                {
                    return AssetDatabase.LoadAssetAtPath<GameObject>(_skillDesc.targetPrefabPath);
                }

                return null;
            }

            set
            {
                string path = null;
                if (value != null)
                {
                    path = AssetDatabase.GetAssetPath(value);
                }

                _skillDesc.targetPrefabPath = path;
                SkillEditorManager.Instance.RefreshModel();
            }
        }

        [Button("删除技能")]
        [PropertyOrder(12)]
        public void DeleteSkillDesc()
        {
            if (EditorUtility.DisplayDialog("", "是否确认删除技能？", "确认", "取消"))
            {
                var skillDescItem = SkillMainWindow.Instance.GetSkillDescItem(_skillDesc);
                skillDescItem.Deselect();
                var skillGroupItem = SkillMainWindow.Instance.GetSkillGroupItem(_skillDesc.groupIndex);
                skillGroupItem.ChildMenuItems.Remove(skillDescItem);
                SkillEditorManager.Instance.Config.skills.Remove(_skillDesc);
            }
        }
        
        public void OnSelect()
        {
            if (_skillDesc.stages.TryGetValue("chant", out var chantStage))
            {
                SkillEditorManager.Instance.OnSelectSkillStage(_skillDesc, chantStage);
            }
            else if (_skillDesc.stages.TryGetValue("default", out var defaultStage))
            {
                SkillEditorManager.Instance.OnSelectSkillStage(_skillDesc, defaultStage);
            }
            else if (_skillDesc.stages.TryGetValue("channel", out var channelStage))
            {
                SkillEditorManager.Instance.OnSelectSkillStage(_skillDesc, channelStage);
            }
            else
            {
                SkillEditorManager.Instance.OnSelectSkillStage(null, null);
            }
        }

        public void CopySkillDesc()
        {
            SkillEditorManager.Instance.SkillDescOnCopyBoard = this._skillDesc;
        }

        public void PasteSkillStage()
        {
            if (SkillEditorManager.Instance.SkillStageOnCopyBoard != null)
            {
                int enumCountOfSkillStageType = Enum.GetValues(typeof(SkillStageType)).Length;
                
                var skillStage = SkillEditorUtil.CloneObject(SkillEditorManager.Instance.SkillStageOnCopyBoard);
                if (_skillDesc.stages.Keys.Count >= enumCountOfSkillStageType)
                {
                    EditorUtility.DisplayDialog("", "该技能的Stage数量已达上限！", "确认");
                    return;
                }

                for (int i = 0; i < enumCountOfSkillStageType; i++)
                {
                    var stageName = ((SkillStageType) i).ToString().ToLower();
                    if (!_skillDesc.stages.ContainsKey(stageName))
                    {
                        _skillDesc.stages.Add(stageName, skillStage);
                        
                        SkillMainWindow.Instance.ForceMenuTreeRebuild();
                        EditorApplication.delayCall += () =>
                        {
                            var menuItem = SkillMainWindow.Instance.GetSkillStageItem(skillStage);
                            SkillMainWindow.Instance.MenuTree.ScrollToMenuItem(menuItem);
                            menuItem.Select();
                        };
                        
                        return;
                    }
                }
            }
            else
            {
                EditorUtility.DisplayDialog("", "请先复制Stage!!!", "确认");
            }
        }

        public void DeleteSkillStage(string stageName)
        {
            if (_skillDesc.stages.ContainsKey(stageName))
            {
                _skillDesc.stages.Remove(stageName);
            }
        }
    }
}