using System;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace SkillEditor
{
    public class SkillModelEditor
    {
        private GameObject _attackerModel;
        private GameObject _targetModel;

        [AssetsOnly]
        [ShowInInspector()]
        public GameObject AttackerModel
        {
            get
            {
                if (_attackerModel == null)
                {
                    string attackerPrefabPath = PlayerPrefs.GetString("SkillEditorAttacker");
                    if (!String.IsNullOrEmpty(attackerPrefabPath))
                    {
                        _attackerModel = AssetDatabase.LoadAssetAtPath<GameObject>(attackerPrefabPath);
                    }
                }
                return _attackerModel;
            }
            set
            {
                if (_attackerModel != value)
                {
                    _attackerModel = value;
                    string attackerPrefabPath = AssetDatabase.GetAssetPath(_attackerModel);
                    PlayerPrefs.SetString("SkillEditorAttacker", attackerPrefabPath);
                    // SkillEditorManager.Instance.RefreshModel();
                }
                
            }
        }

        [AssetsOnly]
        [ShowInInspector()]
        public GameObject TargetModel
        {
            get
            {
                if (_targetModel == null)
                {
                    string attackerPrefabPath = PlayerPrefs.GetString("SkillEditorTarget");
                    if (!String.IsNullOrEmpty(attackerPrefabPath))
                    {
                        _targetModel = AssetDatabase.LoadAssetAtPath<GameObject>(attackerPrefabPath);
                    }
                }
                return _targetModel;
            }
            set
            {
                if (_targetModel != value)
                {
                    _targetModel = value;
                    string targetPrefabPath = AssetDatabase.GetAssetPath(_targetModel);
                    PlayerPrefs.SetString("SkillEditorTarget", targetPrefabPath);
                    // SkillEditorManager.Instance.RefreshModel();
                }
            }
        }
    }
}