// Platformer Game Kit // https://kybernetik.com.au/platformer // Copyright 2021-2023 Kybernetik //

using Animancer.Units;
using PlatformerGameKit.Characters;
using System;
using UnityEngine;
using static Animancer.Validate;

/*
 *
 *     检查角色面前是否有可以站立的地面。通常用于Sequence中，然后是 TurnAround。
 * 
 */
namespace PlatformerGameKit.BehaviourTrees
{
    /// <summary>A <see cref="ConditionNode"/> which checks if there is ground in front of the character.</summary>
    /// <remarks>
    /// Documentation:
    /// <see href="https://kybernetik.com.au/platformer/docs/characters/brains/behaviour/specific#conditions">
    /// Behaviour Tree Brains - Conditions</see>
    /// </remarks>
    /// https://kybernetik.com.au/platformer/api/PlatformerGameKit.BehaviourTrees/IsGroundInFront
    /// 
    [Serializable]
    public sealed class IsGroundInFront : ConditionNode
    {
        /************************************************************************************************************************/

        [SerializeField]
        [Meters(Rule = Value.IsNotNegative)]
        [Tooltip("The maximum distance within which to check (in meters)")]
        private float _Range = 1;

        /// <summary>The maximum distance within which to check (in meters).</summary>
        public ref float Range => ref _Range;

        [SerializeField]
        [Tooltip("The layers which count as ground")]
        private LayerMask _Layers = Physics2D.DefaultRaycastLayers;

        /// <summary>The layers which count as ground.</summary>
        public ref LayerMask Layers => ref _Layers;

#if UNITY_EDITOR
        [SerializeField]
        [Seconds(Rule = Value.IsNotNegative)]
        [Tooltip(Strings.DebugLineDurationTooltip)]
        private float _DebugLineDuration;

        /// <summary>[Editor-Only] Determines how long scene view debug lines are shown for this object.</summary>
        public ref float DebugLineDuration => ref _DebugLineDuration;
#endif

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override bool Condition
        {
            get
            {
                var character = Context<Character>.Current;
                var body = character.Body;
                var stepHeight = Mathf.Max(body.StepHeight, Physics2D.defaultContactOffset * 2);

                var origin = body.Position;
                origin.x += character.MovementDirectionX * _Range;
                origin.y += stepHeight;

                var velocity = body.Velocity;
                if (Vector2.Dot(velocity, character.MovementDirection) > 0)
                    origin += velocity * Time.deltaTime;

                var distance = stepHeight * 2;
                //每一个FixUpdate里向下打一条射线检测
                if (Physics2D.Raycast(origin, Vector2.down, distance, _Layers))
                {
#if UNITY_EDITOR
                    PlatformerUtilities.DrawRay(origin, Vector2.down * distance, Color.blue, _DebugLineDuration);
#endif
                    return true;
                }
                else
                {
#if UNITY_EDITOR
                    PlatformerUtilities.DrawRay(origin, Vector2.down * distance, Color.red, _DebugLineDuration);
#endif
                    return false;
                }
            }
        }

        /************************************************************************************************************************/
    }
}
