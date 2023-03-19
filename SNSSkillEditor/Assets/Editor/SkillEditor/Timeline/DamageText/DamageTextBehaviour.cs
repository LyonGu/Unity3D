// using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SkillEditor.Timeline
{
    public class DamageTextBehaviour : BaseBehaviour
    {
        private GameObject damageText;

        private UnityEngine.Vector3 fromPos;

        private UnityEngine.Vector3 toPos;

        private double _startTime;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (damageText == null)
            {
                var damageTextData = (DamageText) GetData();
                var pos = UpdatePos();
                var damageTextPrefabInScene = SkillEditorManager.Instance.DamageTextPrefab;
                damageText = GameObject.Instantiate(damageTextPrefabInScene, pos, Quaternion.identity, damageTextPrefabInScene.transform.parent);
                var text = damageText.GetComponentInChildren<Text>();
                if(damageTextData.damageTextType == EnumConfig.damageTextType.normal)
                    text.color = Color.black;
                else
                    text.color = Color.red;
                
                
                damageText.SetActive(true);

                const int damageTotalNum = 10000;
                int damageRateNum = (int) (damageTotalNum * damageTextData.rate);
                text.text = damageRateNum.ToString();

                if (Application.isPlaying)
                {
                    // var doTween = damageText.GetComponent<DOTweenAnimation>();
                    // doTween.onComplete.AddListener(OnTweenComplete);
                }
                else
                {
                    fromPos = damageText.FindDirect("from").transform.position;
                    toPos = damageText.transform.position;
                    _startTime = EditorApplication.timeSinceStartup;
                    EditorApplication.update += ManualUpdatePos;
                }
            }
        }

        private void OnTweenComplete()
        {
            if (damageText != null)
            {
                Object.DestroyImmediate(damageText);
                damageText = null;
            }
        }

        private UnityEngine.Vector3 UpdatePos()
        {
            var root = GetSelfModelRoot();
            var screenPos = Camera.main.WorldToScreenPoint(root.position);
            UnityEngine.Vector3 pos = UnityEngine.Vector3.zero;
            Camera uiCamera = SkillEditorManager.Instance.UICamera;
            UnityEngine.RectTransformUtility.ScreenPointToWorldPointInRectangle(
                SkillEditorManager.Instance.DamageTextPrefab.GetComponent<RectTransform>(), screenPos, uiCamera, out pos);
            return pos;
        }

        private void ManualUpdatePos()
        {
            const float totalMoveTime = 2;
            const float totalFadeTime = 1;
            var now = EditorApplication.timeSinceStartup;
            var deltaTime = now - _startTime;
            if (deltaTime > totalMoveTime)
            {
                EditorApplication.update -= ManualUpdatePos;
                OnTweenComplete();
                return;
            }

            UnityEngine.Vector3 pos = UnityEngine.Vector3.Lerp(fromPos, toPos, (float) deltaTime / totalMoveTime);
            damageText.transform.position = pos;

            if (deltaTime > (totalMoveTime - totalFadeTime))
            {
                var canvasGroup = damageText.GetComponent<CanvasGroup>();
                float alpha = Mathf.Lerp(1, 0, (float) (deltaTime - (totalMoveTime - totalFadeTime)) / totalFadeTime);
                canvasGroup.alpha = alpha;
            }
        }
    }
}