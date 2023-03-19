using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SkillEditor.Timeline
{
    public class MoveBehaviour : BaseBehaviour
    {
        private GameObject go2Move;
        private UnityEngine.Vector3 targetPos;
        private UnityEngine.Vector3 startPos;

        private bool IsUnderAttackerGroup()
        {
            return clip.GetParentTrack().GetGroup().name.Equals("attacker");
        }
        
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            go2Move = GetSelfModelRoot().gameObject;
            startPos = go2Move.transform.position;
            targetPos = UnityEngine.Vector3.negativeInfinity;
            
            var data = (Move) GetData();
            if (IsUnderAttackerGroup())
            {
                switch (data.directionType)
                {
                    case EnumConfig.MoveDirectionType.RoleDir:
                        targetPos = startPos + go2Move.transform.forward * data.distance;
                        break;
                    case EnumConfig.MoveDirectionType.SkillPos:
                        // 根据遥感里指定的位置来确定技能位置 编辑器下地方位置扔
                        targetPos = GetOtherModelRoot().transform.position;
                        break;
                    case EnumConfig.MoveDirectionType.Target:
                        Vector3 otherModelPos = GetOtherModelRoot().position;
                        UnityEngine.Vector3 direction = (startPos - otherModelPos).normalized;
                        targetPos = otherModelPos + direction * data.offset;
                        break;
                }
            }
            else
            {
                switch (data.directionType)
                {
                    case EnumConfig.MoveDirectionType.ConnectionDir:
                        UnityEngine.Vector3 direction = (GetOtherModelRoot().position - startPos).normalized;
                        targetPos = startPos + direction * data.distance;
                        break;
                    case EnumConfig.MoveDirectionType.SkillDir:
                        //计算Attacker到Target的连线作为技能方向。
                        //对Target来说技能方向的反方向作为正方向
                        UnityEngine.Vector3 dir = (GetOtherModelRoot().position - startPos).normalized;
                        targetPos = startPos + dir * data.distance;
                        break;
                }
            }

            if (targetPos == UnityEngine.Vector3.negativeInfinity)
            {
                Debug.LogError($"targetPos配置错误. Clip是否属于Attacker:{IsUnderAttackerGroup()}. directionType:{data.directionType}");
                targetPos = GetOtherModelRoot().position;
            }
            
            // if (data.useNotDistance)
            // {
            //     GameObject target = GameObject.Find("ProjectileTarget");
            //     targetPos = target.transform.position;
            // }
            // else
            // {
            //     UnityEngine.Vector3 direction = (GetOtherModelRoot().position - GetSelfModelRoot().position).normalized;
            //     targetPos = GetSelfModelRoot().position + direction * data.distance;
            // }
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (go2Move)
            {
                var time = GetTimeInClip(playable);
                var duration = clip.duration;
                // Version1 直接插值
                // var pos = UnityEngine.Vector3.Lerp(startPos, targetPos, time / (float)duration);
                
                // Version2 ApplyCurve
                var data = (Move) GetData();
                float rate = data.curve.Evaluate(time / (float) duration);
                var pos = UnityEngine.Vector3.Lerp(startPos, targetPos, rate);
                go2Move.transform.position = pos;
            }
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (go2Move)
            {
                go2Move.transform.position = startPos;
            }
        }
    }
}