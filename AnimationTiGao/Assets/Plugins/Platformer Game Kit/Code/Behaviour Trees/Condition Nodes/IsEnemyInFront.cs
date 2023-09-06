// Platformer Game Kit // https://kybernetik.com.au/platformer // Copyright 2021-2023 Kybernetik //

using Animancer;
using Animancer.Units;
using PlatformerGameKit.Characters;
using System;
using UnityEngine;
using static Animancer.Validate;

//此节点：判断前方是否有敌人
/*
 * 检查角色的敌人是否在他们面前（对于怪物，玩家是他们的敌人）。一般用在Sequence中，后面跟TrySetState来攻击敌人
 */
namespace PlatformerGameKit.BehaviourTrees
{
    /// <summary>A <see cref="ConditionNode"/> which checks if an enemy is in front of the character.</summary>
    /// <remarks>
    /// Documentation:
    /// <see href="https://kybernetik.com.au/platformer/docs/characters/brains/behaviour/specific#conditions">
    /// Behaviour Tree Brains - Conditions</see>
    /// </remarks>
    /// https://kybernetik.com.au/platformer/api/PlatformerGameKit.BehaviourTrees/IsEnemyInFront
    /// 
    [Serializable]
    public sealed class IsEnemyInFront : ConditionNode
    {
        /************************************************************************************************************************/

        [SerializeField]
        [Meters(Rule = Value.IsNotNegative)]
        [Tooltip("The maximum distance within which to check (in meters)")]
        private float _Range = 1;

        /// <summary>The maximum distance within which to check (in meters).</summary>
        public ref float Range => ref _Range;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override bool Condition
        {
            get
            {
                var character = Context<Character>.Current;
                var rigidbody = character.Body.Rigidbody;

                var bounds = character.Body.Collider.bounds;
                //中心点往前方向平移_Range
                var center = (Vector2)bounds.center + character.MovementDirection * _Range;

                var filter = new ContactFilter2D
                {
                    layerMask = HitTrigger.HitLayers,
                    useLayerMask = true,
                };

                var colliders = ObjectPool.AcquireList<Collider2D>();
                //都是使用这个判断 Physics2D.OverlapBox
                Physics2D.OverlapBox(center, bounds.size, rigidbody.rotation, filter, colliders);
                for (int i = 0; i < colliders.Count; i++)
                {
                    var hit = new Hit(character.transform, character.Health.Team, 0, 0, default);
                    if (hit.CanHit(colliders[i]))
                    {
                        ObjectPool.Release(colliders);
                        return true;
                    }
                }
                ObjectPool.Release(colliders);
                return false;
            }
        }

        /************************************************************************************************************************/
    }
}
