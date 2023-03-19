using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.Linq;
using System.Reflection;
using UnityEngine.Timeline;
using Object = System.Object;

namespace SkillEditor
{
    public static class SkillEditorUtil
    {
        public static T CloneObject<T>(T obj)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling =  TypeNameHandling.Auto};
            string json = JsonConvert.SerializeObject(obj, Formatting.Indented, settings);
            T target = JsonConvert.DeserializeObject<T>(json, settings);
            return target;
        }

        public static object CloneObject(object obj, Type t)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling =  TypeNameHandling.Auto};
            string json = JsonConvert.SerializeObject(obj, Formatting.Indented, settings);
            object target = JsonConvert.DeserializeObject(json, t, settings);
            return target;
        }
        
        public static void LoadWindowLayout(string path)
        {
            var assembly = typeof(EditorWindow).Assembly;
            var type = assembly.GetType("UnityEditor.WindowLayout");
            var method = type.GetMethod("LoadWindowLayout", new Type[] {typeof(string), typeof(bool), typeof(bool), typeof(bool)});
            method?.Invoke(null, new[] {(object) path, (object) false, (object) true, (object) true});
        }

        public static void SaveWindowLayout(string path)
        {
            var assembly = typeof(EditorWindow).Assembly;
            var type = assembly.GetType("UnityEditor.WindowLayout");
            var method = type.GetMethod("SaveWindowLayout", new Type[] {typeof(string)});
            method?.Invoke(null, new[] {(object) path});
        }

        public static AnimationClip GetAnimationClipByStateName(Animator animator, string stateName)
        {
            AnimationClip clip = null;

            var controller = animator.runtimeAnimatorController;
            if (controller is AnimatorOverrideController animatorOverrideController)
            {
                clip = animatorOverrideController[stateName];
            }
            else if (controller is AnimatorController animatorController)
            {
                foreach (var layer in animatorController.layers)
                {
                    var stateMachine = layer.stateMachine;
                    foreach (var childState in stateMachine.states)
                    {
                        var state = childState.state;
                        if (state.name == stateName)
                        {
                            clip = state.motion as AnimationClip;
                            break;
                        }
                    }
                }
            }
            return clip;
        }

        public static Animator GetAnimatorByTimelineClip(TimelineClip clip)
        {
            var track = clip.GetParentTrack().GetGroup();
            if (track != null)
            {
                if (track.name.Equals("attacker"))
                    return SkillEditorManager.Instance.AttackerGo?.GetComponentInChildren<Animator>();
                if (track.name.Equals("target"))
                    return SkillEditorManager.Instance.TargetGo?.GetComponentInChildren<Animator>();
            }
            return null;
        }


        //反射调用DoTweenAnimation
        // public static void PlayDoTweenInEditor(DG.Tweening.DOTweenAnimation src)
        // {
        //     var DoTweenPreviewManager = typeof(DG.DOTweenEditor.DOTweenPreviewManager);
        //     var assembly = DoTweenPreviewManager.Assembly;
        //     var type = assembly.GetType("DG.DOTweenEditor.DOTweenPreviewManager");
        //     var method = type.GetMethod("StartupGlobalPreview", BindingFlags.Static | BindingFlags.NonPublic);
        //     var method1 = type.GetMethod("AddAnimationToGlobalPreview", BindingFlags.Static | BindingFlags.NonPublic);
        //     method.Invoke(null, null);
        //     method1.Invoke(null, new[] {src});
        // }
        //
        // public static void StopDoTweenInEditor(GameObject obj)
        // {
        //     var DoTweenPreviewManager = typeof(DG.DOTweenEditor.DOTweenPreviewManager);
        //     var assembly = DoTweenPreviewManager.Assembly;
        //     var type = assembly.GetType("DG.DOTweenEditor.DOTweenPreviewManager");
        //     var method = type.GetMethod("StopPreview", new Type[] {typeof(GameObject)});
        //     method.Invoke(null, new []{obj});
        // }

        #region 与项目耦合的放在这里

        public static bool IsGameRunning()
        {
            return Application.isPlaying;
        }
        
        public static System.Text.Encoding UTF8 => new System.Text.UTF8Encoding(false);

        public static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings { TypeNameHandling =  TypeNameHandling.Auto};
        
        #endregion


    }
}