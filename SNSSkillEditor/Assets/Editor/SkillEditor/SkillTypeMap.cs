using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SkillEditor.Timeline;
using UnityEngine.Timeline;

namespace SkillEditor
{
    public static class SkillTypeMap
    {
        private static Dictionary<Type, Type> itemBase2Track = null;
        private static Dictionary<Type, Type> itemBase2Clip = null;

        private static void InitTypeMap()
        { 
            itemBase2Track = new Dictionary<Type, Type>();
            itemBase2Clip = new Dictionary<Type, Type>();

            var trackBaseType = typeof(BaseTrack<>);
            var types = trackBaseType.Assembly.GetTypes().Where(t => t.IsDefined(typeof(BindSkillTrackAttribute)));
            foreach (var type in types)
            { 
                var itemType = type.GetCustomAttribute<BindSkillTrackAttribute>();
                var clipType = type.GetCustomAttribute<TrackClipTypeAttribute>();
                if (itemType != null && clipType != null)
                {
                    itemBase2Track.Add(itemType.SkillTrackType, type);
                    itemBase2Clip.Add(itemType.SkillTrackType, clipType.inspectedType);
                }
            }
        }
        
        public static Type GetTrackType(Type itemBaseType)
        {
            if (itemBase2Track == null || itemBase2Track.Count == 0)
            {
                InitTypeMap();
            }

            if (itemBase2Track.TryGetValue(itemBaseType, out var ret))
            {
                return ret;
            }
            return null;
        }

        public static Type GetClipType(Type itemBaseType)
        {
            if (itemBase2Clip == null || itemBase2Clip.Count == 0)
            {
                InitTypeMap();
            }

            if (itemBase2Clip.TryGetValue(itemBaseType, out var ret))
            {
                return ret;
            }

            return null;
        }
    }
}