using System;

namespace SkillEditor
{
    public class EnumConfig
    {
        [Serializable]
        public enum priority
        {
            strong = 2,
            general = 1,
            sneak = 0,
        }
        
        [Serializable]
        public enum attach
        {
            self_root = 0,
            other_root = 1,
            attach_node_self = 2,
            attach_node_other = 3,
        }
        
        [Serializable]
        public enum dirTarget
        {
            attacker = 0,
            defender = 1,
            server = 2,
        }
        
        [Serializable]
        public enum shape
        {
            rectangle = 0,
            round = 1,
            sector = 2,
        }
        
        [Serializable]
        public enum damageTextType
        {
            normal = 0,
            skill = 1,
        }
        
        [Serializable]
        public enum SkillEventType
        {
            LockSelf = 0,
        }
        
        [Serializable]
        public enum MoveDirectionType
        {
            RoleDir = 1,
            SkillPos = 2,
            Target = 3,
            ConnectionDir = 4,
            SkillDir = 5,
        }
        
        [Serializable]
        public enum Direction
        {
            Forward = 0,
            Reverse = 1,
        }

        [Serializable]
        public enum BuffUIType
        {
            Icon = 1,
            Text = 2,
        }
    }
}