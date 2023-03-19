using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SkillEditor.Timeline;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;
using SkillStage = System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<SkillEditor.ItemBase>>;

namespace SkillEditor
{
    public class SkillEditorManager
    {
        private static SkillEditorManager _instance;

        public static SkillEditorManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SkillEditorManager();
                return _instance;
            }
        }

        public SkillConfig Config;
        private SkillDesc SelectedSkillDesc;
        private SkillStage SelectedSkillStage;
        
        public SkillDesc SkillDescOnCopyBoard;
        public SkillStage SkillStageOnCopyBoard;
        public List<ItemBase> SkillTrackOnCopyBoard = new List<ItemBase>();
        
        private Scene SkillEditorScene;
        public GameObject AttackerGo;
        public GameObject TargetGo;

        public GameObject SceneRoot;
        public PlayableDirector Director;
        public Transform EffectRoot;
        public GameObject DamageTextPrefab;
        public Transform ModelRoot;
        public Camera UICamera;
        public GameObject WWise;

        private SkillTimelineAsset skillTimelineAsset;

        public readonly Dictionary<TrackAsset, float> TrackSpeedDic = new Dictionary<TrackAsset, float>();
        
        private SkillEditorManager()
        {
            Config = LoadSkill();
        }

        public void Reload()
        {
            Config = LoadSkill();
            SelectedSkillDesc = null;
            SelectedSkillStage = null;
            SkillDescOnCopyBoard = null;
            SkillStageOnCopyBoard = null;
            InitSceneField();
            ClearEffect();
            InitTimeline();
            RefreshModel();
            SkillMainWindow.Instance.ForceMenuTreeRebuild();
        }

        public void OpenSkillEditorScene()
        {
            if (!SkillEditorUtil.IsGameRunning())
            {
                var curScene = EditorSceneManager.GetActiveScene();
                if (!curScene.path.Equals(SkillEditorSettings.EditorScenePath))
                {
                    SkillEditorScene = EditorSceneManager.OpenScene(SkillEditorSettings.EditorScenePath);
                    SceneRoot = SkillEditorScene.GetRootGameObjects()[0];
                }
                else
                {
                    SkillEditorScene = curScene;
                    SceneRoot = SkillEditorScene.GetRootGameObjects()[0];
                }
            }
            else
            {
                if (!SkillEditorScene.IsValid())
                {
                    LoadSceneParameters parameters = new LoadSceneParameters(LoadSceneMode.Additive);
                    EditorSceneManager.LoadSceneInPlayMode(SkillEditorSettings.EditorScenePath, parameters);
                }
            }
            InitSceneField();
            Config = LoadSkill();
            SelectedSkillDesc = null;
            SelectedSkillStage = null;
            SkillDescOnCopyBoard = null;
            SkillStageOnCopyBoard = null;
        }

        private void InitSceneField()
        {
            var curScene = EditorSceneManager.GetActiveScene();
            if (!curScene.path.Equals(SkillEditorSettings.EditorScenePath))
            {
                throw new Exception("技能编辑器必须在技能编辑器场景下");
            }
            SkillEditorScene = curScene;
            SceneRoot = SkillEditorScene.GetRootGameObjects()[0];

            Director = SceneRoot.GetComponentInChildren<PlayableDirector>();
            EffectRoot = SceneRoot.FindDirect("Effect").transform;
            DamageTextPrefab = SceneRoot.FindDirect("PlaceHolder/Canvas/DamageText");
            ModelRoot = SceneRoot.FindDirect("Model").transform;
            UICamera = SceneRoot.FindDirect("PlaceHolder/UICamera").GetComponent<Camera>();
            WWise = SceneRoot.FindDirect("PlaceHolder/Wwise");
        }
        
        
        
        private void InitTimeline()
        {
            skillTimelineAsset = ScriptableObject.CreateInstance<SkillTimelineAsset>();
            Director.playableAsset = skillTimelineAsset;
            TimelineUtil.InitTimeline(Director);
        }

        private void BuildTimelineData(Dictionary<string, List<ItemBase>> stage)
        {
            foreach (var pair in stage)
            {
                var groupTrack = skillTimelineAsset.CreateTrack<GroupTrack>();
                groupTrack.name = pair.Key;
                groupTrack.SetCollapsed(false);
                Dictionary<string, TrackAsset> dic = new Dictionary<string, TrackAsset>();
                foreach (var skillClip in pair.Value.ToArray())
                {
                    var track = CreateSubTrack(skillClip, dic);
                    if (track != null)
                    {
                        track.SetGroup(groupTrack);
                    }
                }
            }
        }
        
        private TrackAsset CreateSubTrack(ItemBase skillTrack, Dictionary<string, TrackAsset> dic)
        {
            var type = skillTrack.GetType();
            var trackType = SkillTypeMap.GetTrackType(type);
            var clipType = SkillTypeMap.GetClipType(type);
            string key = type.Name + skillTrack.trackName;

            if (!dic.TryGetValue(key, out TrackAsset trackAsset))
            {
                trackAsset = skillTimelineAsset.CreateTrack(trackType, null, skillTrack.trackName);
                dic.Add(key, trackAsset);
            }

            TimelineClip clip = TimelineUtil.CreateTimelineClip(trackAsset, clipType);
            if (clip != null)
            {
                var baseClip = clip.asset;
                baseClip.GetType().GetField("timelineClip").SetValue(baseClip, clip);
                baseClip.GetType().GetField("data").SetValue(baseClip, skillTrack);
                baseClip.GetType().GetMethod("InitWithData")?.Invoke(baseClip, null);
            }
            return trackAsset;
        }

        bool IsTrackNameDuplicate(string trackName)
        {
            foreach (var outputTrack in skillTimelineAsset.GetOutputTracks())
            {
                if (outputTrack.name.Equals(trackName))
                {
                    return true;
                }
            }
            return false;
        }
        
        public void CreateSubTrackByPaste(GroupTrack groupTrack)
        {
            //使用第一个ItemBase作为模板，生成对应的Track。
            var itemBaseTemplate = SkillTrackOnCopyBoard[0];
            var type = itemBaseTemplate.GetType();
            var trackType = SkillTypeMap.GetTrackType(type);
            var clipType = SkillTypeMap.GetClipType(type);
            
            string newTrackName = itemBaseTemplate.trackName;
            while (IsTrackNameDuplicate(newTrackName))
            {
                newTrackName += "(Clone)";
            }
            TrackAsset trackAsset = skillTimelineAsset.CreateTrack(trackType, null, newTrackName);
            trackAsset.SetGroup(groupTrack);
            
            //生成Track完成，开始生成各个Clip片段
            var copySkillTrack = SkillEditorUtil.CloneObject(SkillTrackOnCopyBoard);
            foreach (var itemBase in copySkillTrack)
            {
                itemBase.trackName = newTrackName;
            }
            foreach (var itemBase in copySkillTrack)
            {
                TimelineClip clip = TimelineUtil.CreateTimelineClip(trackAsset, clipType);
                var baseClip = clip.asset;
                baseClip.GetType().GetField("timelineClip").SetValue(baseClip, clip);
                baseClip.GetType().GetField("data").SetValue(baseClip, itemBase);
                baseClip.GetType().GetMethod("InitWithData")?.Invoke(baseClip, null);
            }
            
            //刷新Timeline窗口
            TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved);
            Selection.activeObject = trackAsset;
        }
        
        
        private void ClearEffect()
        {
            while (EffectRoot.childCount > 0)
            {
                GameObject.DestroyImmediate(EffectRoot.GetChild(0).gameObject);
            }
        }
        
        public void RefreshModel()
        {
            //Destroy Old GameObject
            var modelRoot = SceneRoot.transform.Find("Model");
            var attackerRoot = modelRoot.Find("Attacker");
            var targetRoot = modelRoot.Find("Target");
            while (attackerRoot.childCount > 0)
            {
                Object.DestroyImmediate(attackerRoot.GetChild(0).gameObject);
            }
            while (targetRoot.childCount > 0)
            {
                Object.DestroyImmediate(targetRoot.GetChild(0).gameObject);
            }
            
            //Get Attacker & Target PrefabPath
            string attackerPrefabPath = PlayerPrefs.GetString("SkillEditorAttacker");
            string targetPrefabPath = PlayerPrefs.GetString("SkillEditorTarget");
            if (SelectedSkillDesc != null)
            {
                if (!string.IsNullOrEmpty(SelectedSkillDesc.attackerPrefabPath))
                    attackerPrefabPath = SelectedSkillDesc.attackerPrefabPath;
                if (!string.IsNullOrEmpty(SelectedSkillDesc.targetPrefabPath))
                    targetPrefabPath = SelectedSkillDesc.targetPrefabPath;
            }
            
            //Instantiate
            if (!string.IsNullOrEmpty(attackerPrefabPath))
            {
                var attackerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(attackerPrefabPath);
                if (attackerPrefab != null)
                {
                    AttackerGo = Object.Instantiate(attackerPrefab);
                    AttackerGo.name = "attacker";
                    AttackerGo.SetParent(attackerRoot.gameObject, false);
                }
               
            }
            if (!string.IsNullOrEmpty(targetPrefabPath))
            {
                var targetPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(targetPrefabPath);
                if (targetPrefab != null)
                {
                    TargetGo = Object.Instantiate(targetPrefab);
                    TargetGo.name = "target";
                    TargetGo.SetParent(targetRoot.gameObject, false);
                }
            }
            RebindAnimator();
        }

        public void RebindAnimator()
        {
            // var modelMgr = SceneRoot.FindDirect("Model").GetComponent<SkillEditorModelMgr>();

            Animator attackerAnimator = AttackerGo.GetComponentInChildren<Animator>();
            Animator targetAnimator = TargetGo.GetComponentInChildren<Animator>();
            if (attackerAnimator == null || targetAnimator == null)
            {
                return;
            }
            var tracks = skillTimelineAsset.GetOutputTracks();
            foreach (var track in tracks)
            {
                if (track is SkillEditor.Timeline.AnimationTrack)
                {
                    var groupTrack = track.GetGroup();
                    if (groupTrack.name == "attacker")
                    {
                        Director.SetGenericBinding(track, attackerAnimator);
                    }
                    else if (groupTrack.name == "target")
                    {
                        Director.SetGenericBinding(track, targetAnimator);
                    }
                }
            }
        }
        
        // public void OnSelectSkillStage(SkillDesc desc, SkillStage stage)
        // {
        //     SelectedSkillDesc = desc;
        //     SelectedSkillStage = stage;
        //     
        //     OpenSkillEditorScene();
        //     
        //     var action = new Action(() =>
        //     {
        //         ReInitScene();
        //         if(SelectedSkillStage == null)
        //             return;
        //
        //         BuildTimelineData(SelectedSkillStage);
        //         RefreshModel();
        //     });
        //
        //     if (SkillEditorUtil.IsGameRunning())
        //     {
        //         SkillEditorUtil.ExecuteInMainThread(() =>
        //         {
        //             SkillEditorScene = EditorSceneManager.GetSceneByPath(SkillEditorSettings.EditorScenePath);
        //             SceneRoot = SkillEditorScene.GetRootGameObjects()[0];
        //             var cameras = SceneRoot.GetComponentsInChildren<Camera>();
        //             foreach (var camera in cameras)
        //             {
        //                 camera.gameObject.SetActive(false);
        //             }
        //
        //             foreach (var light in SceneRoot.GetComponentsInChildren<Light>())
        //             {
        //                 light.gameObject.SetActive(false);
        //             }
        //
        //             foreach (var eventSystem in SceneRoot.GetComponentsInChildren<EventSystem>())
        //             {
        //                 eventSystem.gameObject.SetActive(false);
        //             }
        //             
        //             action.Invoke();
        //         });
        //     }
        //     else
        //     {
        //         action.Invoke();
        //     }
        // }
        
        public void OnSelectSkillStage(SkillDesc desc, SkillStage stage)
        {
            SelectedSkillDesc = desc;
            SelectedSkillStage = stage;
            InitSceneField();
            ClearEffect();
            InitTimeline();
            BuildTimelineData(SelectedSkillStage);
            RefreshModel(); //绑定RebindAnimator需要TimelineAsset，所以必须放在InitTimeline BuildTimelineData之后
        }
        
        public int GetNewSkillId()
        {
            int skillId = 0;
            foreach (var skillDesc in Config.skills)
            {
                if (skillDesc.id > skillId)
                {
                    skillId = skillDesc.id;
                }
            }
            return skillId + 1;
        }

        public void AddClipToSelectedSkillStage(string groupTrackName, ItemBase data)
        {
            if (SelectedSkillStage != null)
            {
                SelectedSkillStage[groupTrackName].Add(data);
            }
        }

        public void SyncSelectedSkillStage(Dictionary<string, List<ItemBase>> skillData)
        {
            if (SelectedSkillStage != null)
            {
                SelectedSkillStage.Clear();
                foreach (var keyValuePair in skillData)
                {
                    SelectedSkillStage[keyValuePair.Key] = keyValuePair.Value;
                }
                // SelectedSkillStage["attacker"].Clear();
                // SelectedSkillStage["attacker"].AddRange(skillData["attacker"]);
                // SelectedSkillStage["target"].Clear();
                // SelectedSkillStage["target"].AddRange(skillData["target"]);
            }
        }

        #region Speed

        public void SetCurrentSpeed(TimelineClip clip, float speed)
        {
            
        }
        
        
        
        #endregion
        
        #region 1、Load SkillConfig By Json  2、Save SkillConfig To Json/Excel/FResPath.lua
        
        public SkillConfig LoadSkill()
        {
            string value = File.ReadAllText(SkillEditorSettings.SkillJsonPath, SkillEditorUtil.UTF8);
            SkillConfig config = JsonConvert.DeserializeObject<SkillConfig>(value, SkillEditorUtil.JsonSettings);
            config.skills.Sort((x, y) => x.id - y.id);
            if (config.groups.Count <= 0)
            {
                config.groups.Add(new SkillGroup
                {
                    groupDesc = "默认分组",
                });
            }
            foreach (var skillDesc in config.skills)
            {
                if (skillDesc.groupIndex >= config.groups.Count)
                {
                    skillDesc.groupIndex = 0;
                }
            }
            return config;
        }

        
        public void BeforeSaveSkill()
        {
            BeforeSaveSkill_AddKeyEvent();
            BeforeSaveSkill_ResortSpeedScale();
        }
        
        //如果Target轨道有除了KeyEvent之外的至少一个数据，那么还需要增加一个KeyEventClip，时间是0秒开始。
        private void BeforeSaveSkill_AddKeyEvent()
        {
            var KeyEventType = (new KeyEvent()).type;
            foreach (var skillDesc in Config.skills)
            {
                foreach (var stage in skillDesc.stages.Values)
                {
                    if (stage.TryGetValue("target", out List<ItemBase> targetItemBases))
                    {
                        if (targetItemBases.Count == 0)
                            continue;

                        bool containsKeyEvent = false;
                        foreach (var targetItemBase in targetItemBases)
                        {
                            if (targetItemBase.type == KeyEventType)
                            {
                                containsKeyEvent = true;
                                break;
                            }
                        }
                        if (!containsKeyEvent)
                        {
                            targetItemBases.Add(new KeyEvent(){trackName = "KeyEventTrack(AddByCode)"});
                        }
                    }
                }
            }
        }
        
        // 当前编辑的技能，如果有Animation轨道，且存在速度缩放，那么在保存之前重新排序导出
        private void BeforeSaveSkill_ResortSpeedScale()
        {
            //全部保存修改 测试用
            // void func(SkillStage stage)
            // {
            //     foreach (var listItemBase in stage.Values)
            //     {
            //         foreach (var itemBase in listItemBase)
            //         {
            //             //如果是Animation那么重排序speedScale字段
            //             if (itemBase is SkillEditor.Animation animationData)
            //             {
            //                 animationData.speedScale?.Sort((a, b) =>
            //                 {
            //                     if (a.timeBegin >= b.timeBegin)
            //                         return 1;
            //                     else
            //                         return -1;
            //                 });
            //             }
            //         }
            //     }
            // }
            // foreach (var skillDesc in Config.skills)
            // {
            //     foreach (var stage in skillDesc.stages.Values)
            //     {
            //         func(stage);
            //     }
            // }
            
            if (SelectedSkillStage == null)
            {
                return;
            }
            foreach (var listItemBase in SelectedSkillStage.Values)
            {
                foreach (var itemBase in listItemBase)
                {
                    //如果是Animation那么重排序speedScale字段
                    if (itemBase is SkillEditor.Animation animationData)
                    {
                        animationData.speedScale?.Sort((a, b) =>
                        {
                            if (a.timeBegin >= b.timeBegin)
                                return 1;
                            else
                                return -1;
                        });
                    }
                }
            }
        }
        
        public void SaveSkill()
        {
            SkillConfig toSerializeConfig = Config;
            
            //Write Json
            string value = JsonConvert.SerializeObject(toSerializeConfig, Formatting.Indented, SkillEditorUtil.JsonSettings);
            File.WriteAllText(SkillEditorSettings.SkillJsonPath, value, SkillEditorUtil.UTF8);
            Debug.Log("Save Skill Json Success");
            //Write Excel
            ExportSkillExcel.ExportSkillCsv(toSerializeConfig);
            ExportSkillExcel.ExportSkillFResPath(toSerializeConfig);
            Debug.Log("Save Skill Excel Success");
        }
        
        #endregion
       
    }
}