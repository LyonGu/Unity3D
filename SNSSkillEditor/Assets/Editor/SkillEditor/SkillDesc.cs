using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SkillEditor
{
    
    [Serializable]
    public class SkillConfig
    {
        public List<SkillDesc> skills = new List<SkillDesc>();
        public int version = 1;
        
        public List<SkillGroup> groups = new List<SkillGroup>();
    }

    [Serializable]
    public class SkillGroup
    {
        public string groupDesc;
    }
    
    
    [Serializable]
    public class SkillDesc
    {
        public int id;
        public string des;
        public Dictionary<string, Dictionary<string, List<ItemBase>>> stages = new Dictionary<string, Dictionary<string, List<ItemBase>>>();
        public int groupIndex = 0;
        public string attackerPrefabPath;
        public string targetPrefabPath;
        public SkillDesc()
        {
            Dictionary<string, List<ItemBase>> defaultStage = new  Dictionary<string, List<ItemBase>>();
            
            List<ItemBase> attacker = new List<ItemBase>();
            List<ItemBase> target = new List<ItemBase>();
            defaultStage.Add("attacker", attacker);
            defaultStage.Add("target", target);
            
            stages.Add("default", defaultStage);
        }

        public string GetGroupDesc(SkillConfig config)
        {
            return config.groups[groupIndex].groupDesc;
        }

    }
    
    [Serializable]
    public struct Vector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3(float _x, float _y, float _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }
        
        public static implicit operator Vector3(UnityEngine.Vector3 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }
        
        public static implicit operator UnityEngine.Vector3(Vector3 v)
        {
            return new UnityEngine.Vector3(v.x, v.y, v.z);
        }

        public override string ToString()
        {
            return $"{x}#{y}#{z}";
        }
    }

    [Serializable]
    public struct EditorColor
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public EditorColor(float x, float y, float z, float w = 1)
        {
            r = x;
            g = y;
            b = z;
            a = w;
        }

        public static implicit operator EditorColor(Color color)
        {
            return new EditorColor(color.r, color.g, color.b, color.a);
        }

        public static implicit operator Color(EditorColor color)
        {
            return new Color(color.r, color.g, color.b, color.a);
        }

        public override string ToString()
        {
            return $"{r}#{g}#{b}#{a}";
        }
    }

    [Serializable]
    public class SpeedStretch
    {
        public float timeBegin;
        public float speed;

        public override string ToString()
        {
            return $"{timeBegin}#{speed}";
        }
    }
    
    [Serializable]
    public class ItemBase
    {
        public uint type;
        public string des;
        public float timeBegin = 0;
        public float time = 1f;
        public string trackName;

        public ItemBase()
        {
            Type realType = GetType();
            var attribute = realType.GetCustomAttribute<TypeEnumAttribute>();
            
            if (attribute != null)
                type = attribute.TypeEnum;
            else
                type = 0;
            
            des = realType.Name;
        }
    }
    
    [Serializable]
    [TypeEnum(0)]
    public class Animation : ItemBase
    {
        public string animName = "attack";
        //public AnimationCurve speedScale;
        public List<SpeedStretch> speedScale = new List<SpeedStretch>();
        public bool otherElementScale;
        public EnumConfig.priority priority = EnumConfig.priority.general;
        public float fadeTime = 0.1f;

        public Animation()
        {
            
        }
    }
    
    [Serializable]
    [TypeEnum(1)]
    public class Effect : ItemBase
    {
        public int effectId;
        public string effectPath = "";
        public EnumConfig.attach attach;
        public string attachNodeName = "";
        public EnumConfig.attach attachTarget;
        public string attachTargetNodeName = "";
        public Vector3 scale = UnityEngine.Vector3.one;
        public Vector3 offset = UnityEngine.Vector3.zero;
        public Vector3 rotation = UnityEngine.Vector3.zero;
        public bool isFollow;
        public bool isBallistic; //弹道技
        public bool destroyWhenOwnerDie = true;//当特效绑定的人物死亡的时候，特效随之销毁
        public bool destroyWhenSkillCancel = true;
    }

    [Serializable]
    [TypeEnum(2)]
    public class Move : ItemBase
    {
		public bool useNotDistance = false;
        public EnumConfig.MoveDirectionType directionType;
        public float distance;
        public float offset;
        public string animClipPath;
        public AnimationCurve curve = AnimationCurve.Linear(0,0,1,1);
    }
    
    [Serializable]
    [TypeEnum(3)]
    public class CameraShake : ItemBase
    {
        public float amplitudeGain;
        public float frequencyGain;
    }

    [Serializable]
    [TypeEnum(4)]
    public class GlowEffect : ItemBase
    {
        public EditorColor color;
        public EditorColor endColor;
        public float easeTime; //从color渐变到endColor的过渡时间
    }

    [Serializable]
    [TypeEnum(5)]
    public class AdjustDirection : ItemBase
    {
        public EnumConfig.dirTarget dirTarget = EnumConfig.dirTarget.defender;
        public float angle;
        public bool tweening;
    }

    [Serializable]
    [TypeEnum(6)]
    public class ModelShake : ItemBase
    {
        public Vector3 range;
        public float angularFrequency;
        public float damping;
    }

    [Serializable]
    [TypeEnum(7)]
    public class Alert : ItemBase
    {
        public EnumConfig.shape shape;
        public float length;
        public float width;
        public float angle;
        public float shiftDistance;
    }

    [Serializable]
    [TypeEnum(8)]
    public class DamageText : ItemBase
    {
        public EnumConfig.damageTextType damageTextType;
        public float rate = 1;
    }

    [Serializable, TypeEnum(9)]
    public class SkillAudio : ItemBase
    {
        public string bankName = "";
        public string eventName = "";
    }


    [Serializable, TypeEnum(10)]
    public class ProjectileEffect : ItemBase
    {
        public string effectPath = "";
        public string attachNodeName = "";
        public float height = 1f;
        public float gravity = 10f;
        public Vector3 scale = UnityEngine.Vector3.one;
        public Vector3 offset = UnityEngine.Vector3.zero;
    }

    [Serializable, TypeEnum(11)]
    public class SkillEvent : ItemBase
    {
        public EnumConfig.SkillEventType skillEventType = EnumConfig.SkillEventType.LockSelf;
        public string param = "";
    }

    [Serializable, TypeEnum(12)]
    public class TimeProgressTip : ItemBase
    {
        public EnumConfig.Direction direction;
        public string text = "";
    }
    
    [Serializable, TypeEnum(13)]
    public class KeyEvent : ItemBase
    {
    }
    
    [Serializable, TypeEnum(14)]
    public class AdjustPos : ItemBase
    {
    }
    
    [Serializable, TypeEnum(15)]
    public class AddBuff : ItemBase
    {
        public int buffId;
    }
    
    [Serializable, TypeEnum(16)]
    public class KeepDirection : ItemBase
    {
    }

    public class TypeEnumAttribute : Attribute
    {
        public uint TypeEnum { get; }

        public TypeEnumAttribute(uint value)
        {
            this.TypeEnum = value;
        }
        
    }

}