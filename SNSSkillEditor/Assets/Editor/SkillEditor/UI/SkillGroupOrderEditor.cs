using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace SkillEditor
{
    public class SkillGroupOrderEditor
    {
        private readonly List<SkillGroup> groups;
        
        public SkillGroupOrderEditor(List<SkillGroup> value)
        {
            groups = value;
            SetDefaultGroupNames();
        }

        [ShowInInspector]
        public List<string> GroupNames { get; set; }

        [InfoBox("这个界面只用来修改分组顺序。不要在此处重命名分组或者增删分组。如果有改动可以点按钮还原到默认状态")]
        [PropertySpace(SpaceBefore = 10)]
        [Button("还原分组顺序")]
        public void SetDefaultGroupNames()
        {
            GroupNames = new List<string>(groups.Count());
            foreach (var t in groups)
            {
                GroupNames.Add(t.groupDesc);
            }
        }

        [Button("确认修改分组顺序")]
        public void ConfirmGroupOrderChange()
        {
            if (groups.Count != GroupNames.Count)
            {
                EditorUtility.DisplayDialog("", "不能增删分组", "ok");
                return;
            }
            foreach (var skillGroup in groups)
            {
                if (!GroupNames.Contains(skillGroup.groupDesc))
                {
                    EditorUtility.DisplayDialog("", "不能修改分组命名", "ok");
                    return;
                }
            }
            Dictionary<int, int> oldIndex2NewIndex = new Dictionary<int, int>();
            List<SkillGroup> newGroups = new List<SkillGroup>(groups.Count);
            newGroups.AddRange(groups);
            for (var index = 0; index < groups.Count; index++)
            {
                var skillGroup = groups[index];
                int newIndex = GroupNames.IndexOf(skillGroup.groupDesc);
                oldIndex2NewIndex.Add(index, newIndex);
                newGroups[newIndex] = skillGroup;
            }
            SkillEditorManager.Instance.Config.groups = newGroups;
            foreach (var configSkill in SkillEditorManager.Instance.Config.skills)
            {
                int newIndex = oldIndex2NewIndex[configSkill.groupIndex];
                configSkill.groupIndex = newIndex;
            }
            SkillMainWindow.Instance.ForceMenuTreeRebuild();
        }
        
        
        
        
      
    }
}