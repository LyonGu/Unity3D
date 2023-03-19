using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;

namespace SkillEditor
{
    public class SkillGroupEditor
    {
        private readonly SkillGroup _skillGroup;
        private string newGroupName;

        public SkillGroupEditor(SkillGroup value)
        {
            _skillGroup = value;
            newGroupName = _skillGroup.groupDesc;
        }

        [ShowInInspector]
        [PropertyOrder(0)]
        public string GroupDesc => _skillGroup.groupDesc;


        [HorizontalGroup("Rename")]
        [Button("重命名为")]
        public void RenameGroupAs()
        {
            _skillGroup.groupDesc = NewGroupName;
            SkillMainWindow.Instance.ForceMenuTreeRebuild();
            var focusOn = SkillMainWindow.Instance.GetSkillGroupItem(_skillGroup.groupDesc);
            SkillMainWindow.Instance.MenuTree.ScrollToMenuItem(focusOn);
            focusOn.Select();
        }

        [HorizontalGroup("Rename")]
        [ShowInInspector]
        [HideLabel]
        public string NewGroupName
        {
            get => newGroupName;
            set => newGroupName = value;
        }
        

        [Button("删除分组")]
        public void DeleteGroup()
        {
            if (SkillEditorManager.Instance.Config.groups.Count <= 1)
            {
                EditorUtility.DisplayDialog("", "只剩一个分组了，无法删除！", "确认");
                return;
            }

            if (EditorUtility.DisplayDialog("", "是否确认删除分组？(同时会删除该组所有技能！)", "确认", "取消"))
            {
                int index = SkillEditorManager.Instance.Config.groups.IndexOf(_skillGroup);
                List<SkillDesc> toRemove = new List<SkillDesc>();
                foreach (var skillDesc in SkillEditorManager.Instance.Config.skills)
                {
                    if (skillDesc.groupIndex == index)
                    {
                        toRemove.Add(skillDesc);
                    }
                    else if (skillDesc.groupIndex > index)
                    {
                        skillDesc.groupIndex--;
                    }
                }

                foreach (var skillDesc in toRemove)
                {
                    SkillEditorManager.Instance.Config.skills.Remove(skillDesc);
                }

                SkillEditorManager.Instance.Config.groups.Remove(_skillGroup);
                SkillMainWindow.Instance.ForceMenuTreeRebuild();
            }
        }

        public void PasteSkillDesc()
        {
            if (SkillEditorManager.Instance.SkillDescOnCopyBoard != null)
            {
                var skillDesc = SkillEditorUtil.CloneObject(SkillEditorManager.Instance.SkillDescOnCopyBoard);
                skillDesc.id = SkillEditorManager.Instance.GetNewSkillId();
                skillDesc.des += "(复制)";
                skillDesc.groupIndex = SkillEditorManager.Instance.Config.groups.IndexOf(_skillGroup);
                SkillEditorManager.Instance.Config.skills.Add(skillDesc);
                SkillMainWindow.Instance.ForceMenuTreeRebuild();
                EditorApplication.delayCall += () =>
                {
                    var menuItem = SkillMainWindow.Instance.GetSkillDescItem(skillDesc);
                    SkillMainWindow.Instance.MenuTree.ScrollToMenuItem(menuItem);
                    menuItem.Select();
                };
            }
        }
    }
}