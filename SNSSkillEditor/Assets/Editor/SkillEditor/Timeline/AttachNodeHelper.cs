using System.Collections.Generic;

namespace SkillEditor.Timeline
{
    //绑点相关 主要用于特效
    public class AttachNodeHelper
    {
        public enum AttachNode
        {
            none = 0,
            chest = 1,
            lefthand = 2,
            righthand = 3,
            head = 4,
        }
        
        private static Dictionary<AttachNode, string> node2Name = new Dictionary<AttachNode, string>()
        {
            {AttachNode.none, "none"},
            {AttachNode.chest, "chest"},
            {AttachNode.lefthand, "left hand"},
            {AttachNode.righthand, "right hand"},
            {AttachNode.head, "head"},
        };
        
        public static AttachNode GetNodeByName(string name)
        {
            foreach (var keyValuePair in node2Name)
            {
                if (name.Equals(keyValuePair.Value))
                {
                    return keyValuePair.Key;
                }
            }
            return AttachNode.none;
        }
        
        public static string GetNameByNode(AttachNode node)
        {
            if (node2Name.TryGetValue(node, out string name))
            {
                return name;
            }

            return node2Name[AttachNode.none];
        }

        public static bool IsAttachSelfOrSelfNode(SkillEditor.EnumConfig.attach value)
        {
            return value == EnumConfig.attach.self_root || value == EnumConfig.attach.attach_node_self;
        }
        
        public static bool IsAttachOtherOrOtherNode(SkillEditor.EnumConfig.attach value)
        {
            return value == EnumConfig.attach.other_root || value == EnumConfig.attach.attach_node_other;
        }
    }
}