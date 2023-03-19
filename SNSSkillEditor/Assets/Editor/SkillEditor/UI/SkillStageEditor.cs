using UnityEditor;

using SkillStage = System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<SkillEditor.ItemBase>>;

namespace SkillEditor
{
    public class SkillStageEditor
    {
        private readonly SkillStage _skillStage;

        public SkillStageEditor(SkillStage stage)
        {
            _skillStage = stage;
        }

        public void OnSelect()
        {
            var stageItem = SkillMainWindow.Instance.GetSkillStageItem(_skillStage);
            var skillDesc = (stageItem.Parent.Value as SkillDescEditor)?.SkillDesc;
            
            SkillEditorManager.Instance.OnSelectSkillStage(skillDesc, _skillStage);
        }

        public void DeleteSkillStage()
        {
            var menuItem = SkillMainWindow.Instance.GetSkillStageItem(_skillStage);
            if (menuItem != null)
            {
                var skillDescMenuItem = menuItem.Parent;
                if (skillDescMenuItem != null)
                {
                    if (skillDescMenuItem.Value is SkillDescEditor skillDescItem)
                    {
                        if (EditorUtility.DisplayDialog("", "是否确认删除Stage?", "确认", "取消"))
                        {
                            skillDescItem.DeleteSkillStage(menuItem.Name);
                            string skillDescMenuItemName = skillDescMenuItem.Name;
                            SkillMainWindow.Instance.ForceMenuTreeRebuild();
                            EditorApplication.delayCall += () =>
                            {
                                var newMenuItem = SkillMainWindow.Instance.GetSkillDescItem(skillDescMenuItemName);
                                newMenuItem.Select();
                            };
                        }
                    }
                }
            }
        }

        public void CopySkillStage()
        {
            SkillEditorManager.Instance.SkillStageOnCopyBoard = this._skillStage;
        }
    }
}